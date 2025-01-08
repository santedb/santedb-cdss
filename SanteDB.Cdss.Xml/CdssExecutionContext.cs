/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 */
using DynamicExpresso;
using SanteDB.Cdss.Xml.Diagnostics;
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Cdss.Xml.Model;
using SanteDB.Cdss.Xml.Model.Assets;
using SanteDB.Core;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Cdss;
using SanteDB.Core.Data.Quality;
using SanteDB.Core.Exceptions;
using SanteDB.Core.i18n;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Map;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Model.Serialization;
using SanteDB.Core.Services;
using SharpCompress;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly ModelSerializationBinder m_serializationBinder = new ModelSerializationBinder();

        /// <summary>
        /// True if the context is for validation purposes
        /// </summary>
        public bool IsForValidation { get; }

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
        protected CdssExecutionContext(IdentifiedData scopedObject, IEnumerable<CdssLibraryDefinition> scopedLibraries = null, bool validationContext = false)
        {
            this.m_target = scopedObject;
            this.IsForValidation = validationContext;
            this.m_datasets = scopedLibraries?
                .SelectMany(o => o.Definitions)
                .OfType<CdssDatasetDefinition>()
                .ToCdssReferenceDictionary(o => new CdssReferenceDataset(o));
            // Temporarily assign all assets for any context as in scope
            scopedLibraries = scopedLibraries ?? new CdssLibraryDefinition[0];
            this.m_computableAssetsInScope = scopedLibraries?
                .SelectMany(o => o.Definitions)
                .OfType<CdssDecisionLogicBlockDefinition>()
                .Where(d => d.Context.Type.IsAssignableFrom(scopedObject.GetType()))
                .Where(o=>o.Definitions != null)
                .SelectMany(o => o.Definitions)
                .OfType<CdssComputableAssetDefinition>()
                .ToCdssReferenceDictionary(o => o);

            this.m_scopedLogicBlocks = scopedLibraries?
                .SelectMany(o => o.Definitions)
                .OfType<CdssDecisionLogicBlockDefinition>()
                .AppliesTo(this)
                .SelectMany(o => o.Definitions?.ToArray() ?? new CdssComputableAssetDefinition[0])
                .ToArray();

            this.m_computableAssetsInScope = this.m_scopedLogicBlocks?
                .OfType<CdssComputableAssetDefinition>()
                .ToCdssReferenceDictionary(o => o);

        }

        /// <summary>
        /// Perform a CDR query from the specified <paramref name="resourceType"/>
        /// </summary>
        /// <param name="resourceType">The type of resource</param>
        /// <param name="filterExpression">The filter expression to apply</param>
        /// <returns>A query result set for the CDR query</returns>
        public IQueryResultSet Lookup(String resourceType, String filterExpression)
        {
            var tResource = this.m_serializationBinder.BindToType(null, resourceType);
            if(tResource == null)
            {
                throw new ArgumentOutOfRangeException(string.Format(ErrorMessages.TYPE_NOT_FOUND, resourceType));
            }
            var tRepository = typeof(IRepositoryService<>).MakeGenericType(tResource);
            var repository = ApplicationServiceContext.Current.GetService(tRepository) as IRepositoryService;
            if (repository == null)
            {
                throw new InvalidOperationException(string.Format(ErrorMessages.SERVICE_NOT_FOUND, tRepository));
            }

            
            var vars = this.m_variables.Select(o=>o.Key)
                .Union(this.m_computableAssetsInScope.Select(o=>o.Key))
                .ToDictionaryIgnoringDuplicates<String, String, Func<Object>>(o => o, v => () => this.GetValue(v));
            var linqExpression = QueryExpressionParser.BuildLinqExpression(tResource, filterExpression.ParseQueryString(), "filter", vars);
            return repository.Find(linqExpression);
        }

        /// <summary>
        /// Get the datasets
        /// </summary>
        public CdssReferenceDataset GetDataSet(string idOrName)
        {
            var caseInsensitiveName = idOrName.ToLowerInvariant();
            if (!this.m_datasets.TryGetValue(caseInsensitiveName, out var retVal))
            {
                throw new KeyNotFoundException(idOrName);
            }
            return retVal;
        }

        /// <summary>
        /// Gets the debugger session which is assigned to this context
        /// </summary>
        public CdssDebugSessionData DebugSession { get; private set; }

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
        public IEnumerable<string> FactNames => this.m_computableAssetsInScope?.Where(o => o.Value is CdssComputableAssetDefinition).Select(o => o.Key);


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
                    Type = value?.GetType() ?? typeof(object),
                    Value = value
                };
                this.m_variables.Add(caseInsitiveName, registration);
            }

            registration.Value = value;
            this.DebugSession?.CurrentFrame.AddSample(parameterName, value);
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
                this.DebugSession?.CurrentFrame.AddRead(caseInsensitiveName, value);
                return true;
            }
            else if (this.m_computableAssetsInScope.TryGetValue(caseInsensitiveName, out var defn) && defn is CdssFactAssetDefinition)
            {
                try
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    value = defn.Compute();
                    sw.Stop();
                    var debugFact = this.DebugSession?.CurrentFrame.AddFact(caseInsensitiveName, defn, value, sw.ElapsedMilliseconds);
                    this.m_factCache.Add(caseInsensitiveName, value);
                }
                catch (Exception e)
                {
                    this.DebugSession?.CurrentFrame.AddException(e);
                    throw;
                }
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
            if (this.m_computableAssetsInScope.TryGetValue(caseInsensitiveName, out var defn) && defn is CdssModelAssetDefinition modelDefn)
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
                this.DebugSession?.CurrentFrame.AddRead(caseInsensitiveName, retVal);
                return retVal;
            }
            else if (this.TryGetFact(caseInsensitiveName, out retVal))
            {
                return retVal;
            }
            else if(parameterOrFactName.StartsWith("_") || parameterOrFactName.StartsWith("$")) // Control parameters are null when not presetn
            {
                return null;
            }
            else
            {
                throw new CdssEvaluationException(string.Format(ErrorMessages.REFERENCE_NOT_FOUND, parameterOrFactName));
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
                throw new CdssEvaluationException(string.Format(ErrorMessages.REFERENCE_NOT_FOUND, factName));
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
                this.DebugSession?.CurrentFrame.AddSample(variableName, value);
            }
        }

        /// <inheritdoc/>
        internal void Declare(CdssComputableAssetDefinition fact)
        {
            if (!string.IsNullOrEmpty(fact.Name))
            {
                this.m_computableAssetsInScope.Add(fact.Name.ToLowerInvariant(), fact);
            }
            if (!string.IsNullOrEmpty(fact.Id))
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
                    if (!entity.LoadProperty(o => o.Participations).Any(p => p.Act == proposedAct || p.ActKey == proposedAct.Key))
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
                    if (!act.LoadProperty(o => o.Relationships).Any(r => r.TargetAct == proposedAct || r.TargetActKey == proposedAct.Key))
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
            this.DebugSession?.CurrentFrame.AddProposal(proposedAct);
        }

        /// <summary>
        /// Push an alert
        /// </summary>
        internal void PushIssue(DetectedIssue issue)
        {
            this.m_detectedIssues.Add(issue);
            this.DebugSession?.CurrentFrame.AddIssue(issue);
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
                               .Reference(typeof(Trace))
                               .EnableAssignment(AssignmentOperators.None);

            // Add types
            typeof(Patient).Assembly.GetTypes().Where(t => typeof(IdentifiedData).IsAssignableFrom(t)).ForEach(t => expressionInterpreter.Reference(t));

            // Add delegates 
            Func<String, Int32> intFunc = (s) => this.Int(s);
            Func<String, Double> realFunc = (s) => this.Real(s);
            Func<String, Boolean> boolFunc = (s) => this.Bool(s);
            Func<String, DateTime> dateFunc = (s) => this.Date(s);
            Func<String, String> stringFunc = (s) => this.String(s);
            Func<String, Act> actFunc = (s) => this[s] as Act;
            Func<String, Entity> entityFunc = (s) => this[s] as Entity;
            Func<String, CdssReferenceDataset> datasetFunc = (s) => this.GetDataSet(s);
            expressionInterpreter.SetFunction("int", intFunc);
            expressionInterpreter.SetFunction("real", realFunc);
            expressionInterpreter.SetFunction("bool", boolFunc);
            expressionInterpreter.SetFunction("date", dateFunc);
            expressionInterpreter.SetFunction("string", stringFunc);
            expressionInterpreter.SetFunction("act", actFunc);
            expressionInterpreter.SetFunction("entity", entityFunc);
            expressionInterpreter.SetFunction("data", datasetFunc);
            
            return expressionInterpreter;
        }

        /// <summary>
        /// Create a debug context for the specified execution run
        /// </summary>
        /// <param name="forObject">The object which is to be created</param>
        /// <param name="scopedLibraries">The libraries which are scoped to the context</param>
        /// <returns>The created execution context with the debug information</returns>
        public static CdssExecutionContext CreateDebugContext(IdentifiedData forObject, IEnumerable<CdssLibraryDefinition> scopedLibraries)
        {
            var retVal = CreateContext(forObject, scopedLibraries);
            retVal.DebugSession = CdssDebugSessionData.Create(retVal, scopedLibraries);
            return retVal;
        }

        /// <summary>
        /// Create a context for the provided object
        /// </summary>
        /// <param name="forObject">The object which the context should be created for</param>
        /// <returns>The constructed context</returns>
        public static CdssExecutionContext CreateContext(IdentifiedData forObject, IEnumerable<CdssLibraryDefinition> scopedLibraries)
        {
            if (forObject == null)
            {
                throw new ArgumentNullException(nameof(forObject));
            }
            var cdssType = typeof(CdssExecutionContext<>).MakeGenericType(forObject.GetType());
            return (CdssExecutionContext)Activator.CreateInstance(cdssType, forObject, scopedLibraries, false);
        }

        /// <summary>
        /// Create a context for the provided object
        /// </summary>
        /// <param name="forObject">The object which the context should be created for</param>
        /// <returns>The constructed context</returns>
        public static CdssExecutionContext CreateValidationContext(IdentifiedData forObject, IEnumerable<CdssLibraryDefinition> scopedLibraries)
        {
            if (forObject == null)
            {
                throw new ArgumentNullException(nameof(forObject));
            }
            var cdssType = typeof(CdssExecutionContext<>).MakeGenericType(forObject.GetType());
            return (CdssExecutionContext)Activator.CreateInstance(cdssType, forObject, scopedLibraries, true);
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
            if (this.m_computableAssetsInScope.TryGetValue(ruleName.ToLowerInvariant(), out var computableAsset) && computableAsset is CdssRuleAssetDefinition defn)
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
            if (issues.Any(o => o.Priority == DetectedIssuePriorityType.Error))
            {
                throw new DetectedIssueException(issues);
            }
        }

        /// <summary>
        /// Get data from the context as a date
        /// </summary>
        public DateTime Date(string name) => this[name] is DateTime dt ? dt : throw new CdssEvaluationException(string.Format(ErrorMessages.ARGUMENT_INCOMPATIBLE_TYPE, typeof(DateTime), this[name].GetType()));
        /// <summary>
        /// Get data from the context as a string
        /// </summary>
        public String String(string name) => this[name]?.ToString();
        /// <summary>
        /// Get data from the context as a int
        /// </summary>
        public int Int(string name) => this[name] is int i ? i : throw new CdssEvaluationException(string.Format(ErrorMessages.ARGUMENT_INCOMPATIBLE_TYPE, typeof(int), this[name].GetType()));
        /// <summary>
        /// Get data from the context as a real
        /// </summary>
        public double Real(string name) => this[name] is double d ? d : throw new CdssEvaluationException(string.Format(ErrorMessages.ARGUMENT_INCOMPATIBLE_TYPE, typeof(double), this[name].GetType()));
        /// <summary>
        /// Get data from the context as a bool
        /// </summary>
        public bool Bool(string name) => this[name] is bool b ? b : throw new CdssEvaluationException(string.Format(ErrorMessages.ARGUMENT_INCOMPATIBLE_TYPE, typeof(bool), this[name].GetType()));
    }
    /// <summary>
    /// Parameter manager for the CDSS
    /// </summary>
    public class CdssExecutionContext<TTarget> : CdssExecutionContext
        where TTarget : IdentifiedData
    {


        /// <inheritdoc/>
        public CdssExecutionContext(TTarget target, IEnumerable<CdssLibraryDefinition> scopedLibraries = null, bool validationContext = false) : base(target, scopedLibraries, validationContext)
        {
        }

        /// <summary>
        /// Gets or sets the target of the context
        /// </summary>
        public TTarget Target => (TTarget)base.m_target;


    }
}