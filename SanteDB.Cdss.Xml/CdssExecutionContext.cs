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
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Cdss.Xml.Model;
using SanteDB.Cdss.Xml.Model.Assets;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Cdss;
using SanteDB.Core.Exceptions;
using SanteDB.Core.i18n;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Map;
using SanteDB.Core.Model.Roles;
using SharpCompress;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SanteDB.Cdss.Xml
{
    /// <summary>
    /// Represents a base class for the cdss context
    /// </summary>
    public abstract class CdssExecutionContext : ICdssExecutionContext

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

        protected readonly IdentifiedData m_target;

        // Parameter values
        private readonly IDictionary<String, ParameterRegistration> m_variables = new Dictionary<String, ParameterRegistration>();
        private readonly IDictionary<String, Object> m_factCache = new ConcurrentDictionary<String, Object>();
        private readonly IDictionary<String, CdssComputableAssetDefinition> m_computableAssetsInScope;
        private readonly IDictionary<String, CdssReferenceDataset> m_datasets;
        private readonly CdssComputableAssetDefinition[] m_scopedLogicBlocks;
        private readonly List<Act> m_proposedActions = new List<Act>();
        private readonly List<DetectedIssue> m_detectedIssues = new List<DetectedIssue>();
        private readonly object m_lock = new object();

        /// <summary>
        /// Create a new context with specified target
        /// </summary>
        /// <param name="scopedLibraries">The libraries which are in scope of this context</param>
        /// <param name="scopedObject">The primary focal object for which decision support is being executed</param>
        public CdssExecutionContext(IdentifiedData scopedObject, IEnumerable<CdssLibraryDefinition> scopedLibraries = null)
        {
            this.m_target = scopedObject;
            this.m_datasets = scopedLibraries?
                .SelectMany(o => o.Definitions)
                .OfType<CdssDatasetDefinition>()
                .ToCdssReferenceDictionary(o => new CdssReferenceDataset(o));
            // Temporarily assign all assets for any context as in scope
            this.m_computableAssetsInScope = scopedLibraries
                .SelectMany(o => o.Definitions)
                .OfType<CdssDecisionLogicBlockDefinition>()
                .Where(d => d.Context.Type.IsAssignableFrom(scopedObject.GetType()) && d.When == null)
                .SelectMany(o => o.Definitions)
                .OfType<CdssComputableAssetDefinition>()
                .ToCdssReferenceDictionary(o=>o);

            this.m_scopedLogicBlocks = scopedLibraries?
                .SelectMany(o => o.Definitions)
                .OfType<CdssDecisionLogicBlockDefinition>()
                .AppliesTo(this)
                .SelectMany(o => o.Definitions)
                .ToArray();

            this.m_computableAssetsInScope = this.m_scopedLogicBlocks?
                .OfType<CdssComputableAssetDefinition>()
                .ToCdssReferenceDictionary(o => o);
            
        }

        /// <summary>
        /// Get the datasets
        /// </summary>
        public IDictionary<String, CdssReferenceDataset> DataSets => this.m_datasets;

        /// <summary>
        /// Get the variables
        /// </summary>
        public IEnumerable<String> Variables => this.m_variables.Keys;

        /// <summary>
        /// Get all proposals
        /// </summary>
        public IEnumerable<Act> Proposals => this.m_proposedActions.ToArray();

        /// <summary>
        /// Issues that were raised
        /// </summary>
        public IEnumerable<DetectedIssue> Issues => this.m_detectedIssues.ToArray();

        /// <summary>
        /// Get the target
        /// </summary>
        IdentifiedData ICdssExecutionContext.Target => this.m_target;

        /// <summary>
        /// Get the facts which can be referenced
        /// </summary>
        public IEnumerable<string> FactNames => this.m_computableAssetsInScope?.Where(o=>o.Value is CdssFactAssetDefinition).Select(o=>o.Key);


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
            var caseInsitiveName = parameterName.ToLowerInvariant(); // Case insensitive
            if (!this.m_variables.TryGetValue(caseInsitiveName, out ParameterRegistration registration))
            {
                registration = new ParameterRegistration()
                {
                    Type = value.GetType(),
                    Value = value
                };
                this.m_variables.Add(caseInsitiveName, registration);
            }

            registration.Value = value;
            this.ClearEvaluatedFacts();
        }

        /// <summary>
        /// Gets the fact named <paramref name="factName"/>
        /// </summary>
        internal bool TryGetFact(String factName, out object value)
        {
            var caseInsensitiveName = factName.ToLowerInvariant(); // Case insensitive
            if (this.m_factCache.TryGetValue(caseInsensitiveName, out value))
            {
                return true;
            }
            else if(this.m_computableAssetsInScope.TryGetValue(caseInsensitiveName, out var defn) && defn is CdssFactAssetDefinition factDefn)
            {
                value = defn.Compute();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Try to get a shared model from the CDSS execution context
        /// </summary>
        internal bool TryGetModel(String modelName, out object value)
        {
            var caseInsensitiveName = modelName.ToLowerInvariant();
            if(this.m_computableAssetsInScope.TryGetValue(caseInsensitiveName, out var defn) && defn is CdssModelAssetDefinition modelDefn)
            {
                value = modelDefn.Compute();
                return true;
            }
            value = null;
            return false;
        }
        /// <inheritdoc/>
        public object GetValue(String parameterOrFactName)
        {
            var caseInsensitiveName = parameterOrFactName.ToLowerInvariant(); // Case insensitive
            if (this.m_variables.TryGetValue(caseInsensitiveName, out ParameterRegistration registration) &&
                MapUtil.TryConvert(registration.Value, registration.Type, out object retVal))
            {
                return retVal;
            }
            else if (this.TryGetFact(caseInsensitiveName, out retVal))
            {
                return retVal;
            }
            else
            {
                throw new CdssEvaluationException(String.Format(ErrorMessages.REFERENCE_NOT_FOUND, parameterOrFactName));
            }
        }

        /// <summary>
        /// Get the value of the fact in the current context
        /// </summary>
        public object GetFact(String factName)
        {
            var caseInsensitiveName = factName.ToLowerInvariant(); // Case insensitive
            if (this.TryGetFact(caseInsensitiveName, out var retVal))
            {
                return retVal;
            }
            else
            {
                throw new CdssEvaluationException(String.Format(ErrorMessages.REFERENCE_NOT_FOUND, factName));
            }
        }

        /// <inheritdoc/>
        public TValue GetValue<TValue>(String parameterOrFactName) => (TValue)this.GetValue(parameterOrFactName);

        /// <inheritdoc/>
        internal void Declare<T>(string variableName, T value)
        {
            var caseInsensitiveName = variableName.ToLowerInvariant(); // Case insensitive

            if (!this.m_variables.ContainsKey(caseInsensitiveName))
            {
                this.m_variables.Add(caseInsensitiveName, new ParameterRegistration()
                {
                    Type = typeof(T),
                    Value = value
                });
            }
        }

        /// <inheritdoc/>
        internal void Declare(CdssComputableAssetDefinition fact)
        {
            if (!String.IsNullOrEmpty(fact.Name))
            {
                this.m_computableAssetsInScope.Add(fact.Name.ToLowerInvariant(), fact);
            }
            if (!String.IsNullOrEmpty(fact.Id))
            {
                this.m_computableAssetsInScope.Add($"#{fact.Id.ToLowerInvariant()}", fact);
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
                    // If the target of this context is an entity then set them as the record target
                    if (!entity.LoadProperty(o=>o.Participations).Any(p => p.Act == proposedAct || p.ActKey == proposedAct.Key))
                    {
                        lock (this.m_lock)
                        {
                            var proposal = new ActParticipation(ActParticipationKeys.RecordTarget, entity)
                            {
                                Act = proposedAct
                            };
                            entity.Participations.Add(proposal);
                        }

                        // Add the entity as a record target to the model
                        proposedAct.LoadProperty(o => o.Participations).Add(new ActParticipation(ActParticipationKeys.RecordTarget, entity.Key));
                    }
                    break;
                case Act act:
                    // If the target of this context is another act then set them as a component
                    if (!act.LoadProperty(o=>o.Relationships).Any(r => r.TargetAct == proposedAct || r.TargetActKey == proposedAct.Key))
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

            this.ClearEvaluatedFacts();
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
            var expressionInterpreter = new Interpreter(InterpreterOptions.Default)
                               .Reference(typeof(DateTimeOffset))
                               .Reference(typeof(ExtensionMethods))
                               .EnableAssignment(AssignmentOperators.None);

            // Add types
            typeof(Patient).Assembly.GetTypes().Where(t => typeof(IdentifiedData).IsAssignableFrom(t)).ForEach(t => expressionInterpreter.Reference(t));

            return expressionInterpreter;
        }

        /// <summary>
        /// Create a context for the provided object
        /// </summary>
        /// <param name="forObject">The object which the context should be created for</param>
        /// <returns>The constructed context</returns>
        public static CdssExecutionContext CreateContext(IdentifiedData forObject, IEnumerable<CdssLibraryDefinition> scopedLibraries)
        {
            if(forObject == null)
            {
                throw new ArgumentNullException(nameof(forObject));
            }
            var cdssType = typeof(CdssExecutionContext<>).MakeGenericType(forObject.GetType());
            return (CdssExecutionContext)Activator.CreateInstance(cdssType, forObject, scopedLibraries);
        }

        /// <summary>
        /// Removes <paramref name="variableName"/> from the context
        /// </summary>
        internal void DestroyValue(string variableName)
        {
            this.m_variables.Remove(variableName);
        }


        /// <summary>
        /// Gets a scoped fact definition named <paramref name="factName"/>
        /// </summary>
        internal bool TryGetFactDefinition(string factName, out CdssFactAssetDefinition definition)
        {
            if (this.m_computableAssetsInScope.TryGetValue(factName.ToLowerInvariant(), out var computableAsset) && computableAsset is CdssFactAssetDefinition defn)
            {
                definition = defn;
                return true;
            }
            definition = null;
            return false;
        }

        /// <summary>
        /// Gets a scoped rule definition named <paramref name="ruleName"/>
        /// </summary>
        internal bool TryGetRuleDefinition(string ruleName, out CdssRuleAssetDefinition definition)
        {
            if(this.m_computableAssetsInScope.TryGetValue(ruleName.ToLowerInvariant(), out var computableAsset) && computableAsset is CdssRuleAssetDefinition defn)
            {
                definition = defn;
                return true;
            }
            definition = null;
            return false;
        }


        /// <summary>
        /// Gets a scoped rule definition named <paramref name="ruleName"/>
        /// </summary>
        internal bool TryGetProtocolDefinition(string ruleName, out CdssProtocolAssetDefinition definition)
        {
            if (this.m_computableAssetsInScope.TryGetValue(ruleName.ToLowerInvariant(), out var computableAsset) && computableAsset is CdssProtocolAssetDefinition defn)
            {
                definition = defn;
                return true;
            }
            definition = null;
            return false;
        }

        /// <summary>
        /// Throw a <see cref="DetectedIssueException"/> if the scoped logic in this context is not valid
        /// </summary>
        internal void ThrowIfNotValid()
        {
            var issues = this.m_scopedLogicBlocks.SelectMany(o => o.Validate(this)).ToArray();
            if(issues.Any(o=>o.Priority == DetectedIssuePriorityType.Error))
            {
                throw new DetectedIssueException(issues);
            }
        }
    }
    /// <summary>
    /// Parameter manager for the CDSS
    /// </summary>
    public class CdssExecutionContext<TTarget>  : CdssExecutionContext
        where TTarget : IdentifiedData
    {
       
        
        /// <inheritdoc/>
        public CdssExecutionContext(TTarget target, IEnumerable<CdssLibraryDefinition> scopedLibraries = null) : base(target, scopedLibraries)
        {
        }

        /// <summary>
        /// Gets or sets the target of the context
        /// </summary>
        public TTarget Target => (TTarget)base.m_target;


    }
}