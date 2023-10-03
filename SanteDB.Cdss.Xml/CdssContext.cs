/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: fyfej
 * Date: 2023-5-19
 */
using DynamicExpresso;
using SanteDB.Cdss.Xml.Model.Assets;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Map;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SanteDB.Cdss.Xml
{

    /// <summary>
    /// Represents a base class for the cdss context
    /// </summary>
    public abstract class CdssContext
    {
        /// <summary>
        /// Parameter registration
        /// </summary>
        private class ParameterRegistration
        {
            /// <summary>
            /// Type of the parameter
            /// </summary>
            public Type Type { get; set; }

            /// <summary>
            /// Value of the parameter
            /// </summary>
            public object Value { get; set; }
        }

        // Target
        protected readonly IdentifiedData m_target;

        // Parameter values
        private readonly IDictionary<String, ParameterRegistration> m_variables = new Dictionary<String, ParameterRegistration>();
        private readonly IDictionary<String, Object> m_factCache = new ConcurrentDictionary<String, Object>();
        private readonly IDictionary<String, CdssComputableAssetDefinition> m_factDefinitions = new Dictionary<String, CdssComputableAssetDefinition>();
        private readonly ConcurrentBag<Act> m_proposedActions = new ConcurrentBag<Act>();
        private readonly ConcurrentBag<DetectedIssue> m_detectedIssues = new ConcurrentBag<DetectedIssue>();
        private readonly object m_lock = new object();

        /// <summary>
        /// Create a new context with specified target
        /// </summary>
        public CdssContext(IdentifiedData target)
        {
            this.m_target = target;
        }

        /// <summary>
        /// Get the variables
        /// </summary>
        public IEnumerable<String> Variables => this.m_variables.Keys;

        /// <summary>
        /// Get all proposals
        /// </summary>
        public IEnumerable<Act> Proposals => this.m_proposedActions.ToArray();

        /// <summary>
        /// Property indexer for variable name
        /// </summary>
        /// <param name="variableName">The name of the variable to fetch</param>
        /// <returns>The variable value (if it is declared) for <paramref name="variableName"/></returns>
        public object this[string variableName]
        {
            get => this.GetValue(variableName);
            set => this.SetValue(variableName, value);
        }

        /// <inheritdoc/>
        public void SetValue(String parameterName, object value)
        {
            if (!this.m_variables.TryGetValue(parameterName, out ParameterRegistration registration))
            {
                registration = new ParameterRegistration()
                {
                    Type = value.GetType(),
                    Value = value
                };
                this.m_variables.Add(parameterName, registration);
            }

            registration.Value = value;
        }

        /// <inheritdoc/>
        public object GetValue(String parameterOrFactName)
        {
            if (this.m_variables.TryGetValue(parameterOrFactName, out ParameterRegistration registration) &&
                MapUtil.TryConvert(registration.Value, registration.Type, out object retVal))
            {
                return retVal;
            }
            else if (this.m_factCache.TryGetValue(parameterOrFactName, out var cache))
            {
                return cache;
            }
            else if (this.m_factDefinitions.TryGetValue(parameterOrFactName, out var defn))
            {
                var value = defn.Compute(this);
                this.m_factCache.Add(parameterOrFactName, value);
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public TValue GetValue<TValue>(String parameterOrFactName) => (TValue)this.GetValue(parameterOrFactName);

        /// <inheritdoc/>
        internal void Declare(string variableName, Type variableType)
        {
            if (!this.m_variables.ContainsKey(variableName))
            {
                this.m_variables.Add(variableName, new ParameterRegistration()
                {
                    Type = variableType,
                    Value = null
                });
            }
        }

        /// <inheritdoc/>
        internal void Declare(CdssComputableAssetDefinition fact)
        {
            if (!String.IsNullOrEmpty(fact.Name))
            {
                this.m_factCache.Add(fact.Name, fact);
            }
            if (!String.IsNullOrEmpty(fact.Id))
            {
                this.m_factCache.Add($"#{fact.Id}", fact);
            }
        }

        /// <inheritdoc/>
        internal void ClearEvaluatedFacts()
        {
            this.m_factCache.Clear();
        }

        /// <summary>
        /// Push a proposal act 
        /// </summary>
        internal void PushProposal(Act proposedAct)
        {
            if (this.m_proposedActions.Contains(proposedAct))
            {
                return; // Already proposed
            }

            // Enusre properties are set correctly
            this.m_proposedActions.Add(proposedAct);
            proposedAct.MoodConceptKey = ActMoodKeys.Propose;
            proposedAct.Key = proposedAct.Key ?? Guid.NewGuid();
            proposedAct.StatusConceptKey = StatusKeys.New;

            switch (this.m_target)
            {
                case Entity entity:
                    if (!entity.Participations.Any(p => p.Act == proposedAct || p.ActKey == proposedAct.Key))
                    {
                        lock (this.m_lock)
                        {
                            var proposal = new ActParticipation(ActParticipationKeys.RecordTarget, entity)
                            {
                                Act = proposedAct
                            };
                            entity.Participations.Add(proposal);
                        }
                    }
                    break;
                case Act act:
                    if (!act.Relationships.Any(r => r.TargetAct == proposedAct || r.TargetActKey == proposedAct.Key))
                    {
                        lock (this.m_lock)
                        {
                            var proposal = new ActRelationship(ActRelationshipTypeKeys.HasComponent, proposedAct)
                            {
                                SourceEntity = act
                            };
                            act.Relationships.Add(proposal);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Push an alert
        /// </summary>
        internal void PushIssue(DetectedIssue issue)
        {
            this.m_detectedIssues.Add(issue);
        }


        /// <summary>
        /// Get an expression interpreter for this CDSS context
        /// </summary>
        /// <returns></returns>
        internal Interpreter GetExpressionInterpreter()
        {
            var expressionInterpreter = new Interpreter(InterpreterOptions.LambdaExpressions | InterpreterOptions.Default | InterpreterOptions.LateBindObject)
                               .Reference(typeof(TimeSpan))
                               .Reference(typeof(Guid))
                               .Reference(typeof(DateTimeOffset));
            return expressionInterpreter;
        }

        /// <summary>
        /// Create a context for the provided object
        /// </summary>
        /// <param name="forObject">The object which the context should be created for</param>
        /// <returns>The constructed context</returns>
        public static CdssContext CreateContext(IdentifiedData forObject)
        {
            if(forObject == null)
            {
                throw new ArgumentNullException(nameof(forObject));
            }
            var cdssType = typeof(CdssContext<>).MakeGenericType(forObject.GetType());
            return (CdssContext)Activator.CreateInstance(cdssType, forObject);
        }

    }
    /// <summary>
    /// Parameter manager for the CDSS
    /// </summary>
    public class CdssContext<TTarget>  : CdssContext
        where TTarget : IdentifiedData
    {
       
        
        /// <summary>
        /// Gets the target
        /// </summary>
        public CdssContext(TTarget target) : base(target)
        {
        }

        /// <summary>
        /// Gets or sets the target of the context
        /// </summary>
        public TTarget Target => (TTarget)base.m_target;


    }
}