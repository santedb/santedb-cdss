﻿/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2022-5-30
 */
using DynamicExpresso;
using SanteDB;
using SanteDB.Core.Applets.ViewModel.Json;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.Roles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// Reperesents a then condition clause
    /// </summary>
    [XmlType(nameof(ProtocolThenClauseCollection), Namespace = "http://santedb.org/cdss")]
    public class ProtocolThenClauseCollection : DecisionSupportBaseElement
    {
        // JSON Serializer
        private static JsonViewModelSerializer s_serializer = new JsonViewModelSerializer();

        /// <summary>
        /// Actions to be performed
        /// </summary>
        [XmlElement("action", Type = typeof(ProtocolDataAction))]
        public List<ProtocolDataAction> Action { get; set; }

        /// <summary>
        /// Evaluate the actions
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Act> Evaluate(CdssContext<Patient> context)
        {
            List<Act> retVal = new List<Act>();
            Dictionary<String, Object> calculatedScopes = new Dictionary<string, object>()
            {
                { ".", context.Target }
            };

            foreach (var itm in this.Action)
            {
                Act act = null;
                if (itm.Element is String) // JSON
                {
                    itm.Element = s_serializer.DeSerialize<Act>(itm.Element as String);
                    // Load all concepts for the specified objects
                }
                act = (itm.Element as Act).Clone() as Act;
                act.Participations = (itm.Element as Act).Participations?.Select(o => o.Clone() as ActParticipation).ToList();
                act.Relationships = (itm.Element as Act).Relationships?.Select(o => o.Clone() as ActRelationship).ToList();
                act.Protocols = new List<ActProtocol>();// (itm.Element as Act).Protocols);
                // Now do the actions to the properties as stated
                foreach (var instr in itm.Do)
                {
                    instr.Evaluate(act, context, calculatedScopes);
                }

                // Assign this patient as the record target
                act.Key = act.Key ?? Guid.NewGuid();
                Guid pkey = Guid.NewGuid();

                act.LoadProperty(o => o.Participations).Add(new ActParticipation(ActParticipationKeys.RecordTarget, context.Target.Key) { ParticipationRole = new Core.Model.DataTypes.Concept() { Key = ActParticipationKeys.RecordTarget, Mnemonic = "RecordTarget" }, Key = pkey });
                // Add record target to the source for forward rules
                context.Target.LoadProperty(o => o.Participations).Add(new ActParticipation(ActParticipationKeys.RecordTarget, context.Target) { SourceEntity = act, ParticipationRole = new Core.Model.DataTypes.Concept() { Key = ActParticipationKeys.RecordTarget, Mnemonic = "RecordTarget" }, Key = pkey });
                act.CreationTime = DateTimeOffset.Now;
                // The act to the return value
                retVal.Add(act);
            }

            return retVal;
        }
    }

    /// <summary>
    /// Asset data action base
    /// </summary>
    [XmlType(nameof(ProtocolDataAction), Namespace = "http://santedb.org/cdss")]
    public class ProtocolDataAction
    {
        /// <summary>
        /// ctor
        /// </summary>
        public ProtocolDataAction()
        {
        }

        /// <summary>
        /// Gets the elements to be performed
        /// </summary>
        [XmlElement(nameof(Act), typeof(Act), Namespace = "http://santedb.org/model")]
        [XmlElement(nameof(TextObservation), typeof(TextObservation), Namespace = "http://santedb.org/model")]
        [XmlElement(nameof(SubstanceAdministration), typeof(SubstanceAdministration), Namespace = "http://santedb.org/model")]
        [XmlElement(nameof(QuantityObservation), typeof(QuantityObservation), Namespace = "http://santedb.org/model")]
        [XmlElement(nameof(CodedObservation), typeof(CodedObservation), Namespace = "http://santedb.org/model")]
        [XmlElement(nameof(PatientEncounter), typeof(PatientEncounter), Namespace = "http://santedb.org/model")]
        [XmlElement(nameof(Procedure), typeof(Procedure), Namespace = "http://santedb.org/model")]
        [XmlElement(nameof(DetectedIssue), typeof(DetectedIssue), Namespace = "http://santedb.org/issue")]
        [XmlElement("reference", typeof(CdssObjectReference))]
        [XmlElement("jsonModel", typeof(String))]
        public Object Element { get; set; }

        /// <summary>
        /// Associate the specified data for stuff that cannot be serialized
        /// </summary>
        [XmlElement("assign", typeof(PropertyAssignAction))]
        [XmlElement("add", typeof(PropertyAddAction))]
        public List<PropertyAction> Do { get; set; }
    }

    /// <summary>
    /// Associate data
    /// </summary>
    [XmlType(nameof(PropertyAction), Namespace = "http://santedb.org/cdss")]
    public abstract class PropertyAction : ProtocolDataAction
    {
        /// <summary>
        /// Action name
        /// </summary>
        public abstract String ActionName { get; }

        /// <summary>
        /// The name of the property
        /// </summary>
        [XmlAttribute("propertyName")]
        public String PropertyName { get; set; }

        /// <summary>
        /// Evaluate the expression
        /// </summary>
        /// <returns></returns>
        public abstract object Evaluate(Act act, CdssContext<Patient> context, IDictionary<String, Object> scopes);
    }

    /// <summary>
    /// Property assign value
    /// </summary>
    [XmlType(nameof(PropertyAssignAction), Namespace = "http://santedb.org/cdss")]
    public class PropertyAssignAction : PropertyAction
    {
        // The setter action
        private Lambda m_setter;

        // Select method
        private MethodInfo m_scopeSelectMethod;

        // Linq expression to select scope
        private Expression m_linqExpression;

        // Compiled expression
        private Delegate m_compiledExpression;

        /// <summary>
        /// Action name
        /// </summary>
        public override string ActionName
        {
            get
            {
                return "SetValue";
            }
        }

        /// <summary>
        /// Selection of scope
        /// </summary>
        [XmlAttribute("scope")]
        public String ScopeSelector { get; set; }

        /// <summary>
        /// Where filter for scope
        /// </summary>
        [XmlAttribute("where")]
        public String WhereFilter { get; set; }

        /// <summary>
        /// Value expression
        /// </summary>
        [XmlText()]
        public String ValueExpression { get; set; }

        /// <summary>
        /// Get the specified value
        /// </summary>
        public object GetValue(Act act, CdssContext<Patient> context, IDictionary<String, Object> scopes)
        {
            if (this.m_setter == null)
            {
                Func<String, Object> varFetch = (a) => context.Get(a);

                var interpretor = new Interpreter(InterpreterOptions.Default)
                    .Reference(typeof(TimeSpan))
                    .Reference(typeof(Guid))
                    .Reference(typeof(DateTimeOffset))
                    .Reference(typeof(Types));

                // Scope
                if (!String.IsNullOrEmpty(this.ScopeSelector) && this.m_setter == null)
                {
                    var scopeProperty = context.Target.GetType().GetRuntimeProperty(this.ScopeSelector);

                    if (scopeProperty == null)
                    {
                        return null; // no scope
                    }

                    // Where clause?
                    if (!String.IsNullOrEmpty(this.WhereFilter) && this.m_scopeSelectMethod == null)
                    {
                        var itemType = scopeProperty.PropertyType.GenericTypeArguments[0];
                        this.m_linqExpression = QueryExpressionParser.BuildLinqExpression(itemType, this.WhereFilter.ParseQueryString());
                        var predicateType = typeof(Func<,>).MakeGenericType(new Type[] { itemType, typeof(bool) });
                        var builderMethod = typeof(QueryExpressionParser).GetGenericMethod(nameof(QueryExpressionParser.BuildLinqExpression), new Type[] { itemType }, new Type[] { typeof(NameValueCollection) });
                        this.m_compiledExpression = (this.m_linqExpression as LambdaExpression).Compile();
                        // Call where clause
                        builderMethod = typeof(Expression).GetGenericMethod(nameof(Expression.Lambda), new Type[] { predicateType }, new Type[] { typeof(Expression), typeof(ParameterExpression[]) });
                        var firstMethod = typeof(Enumerable).GetGenericMethod("FirstOrDefault",
                               new Type[] { itemType },
                               new Type[] { scopeProperty.PropertyType, predicateType });

                        this.m_scopeSelectMethod = (MethodInfo)firstMethod;
                    }
                    interpretor = interpretor.Reference(this.m_scopeSelectMethod.ReturnType);
                    this.m_setter = interpretor.Parse(this.ValueExpression, new Parameter("_", this.m_scopeSelectMethod.ReturnType));
                }
                else
                {
                    this.m_setter = interpretor.Parse(this.ValueExpression, new Parameter("_", typeof(CdssContext<Patient>)));
                }
            }

            Object setValue = null;
            // Where clause?
            if (!String.IsNullOrEmpty(this.ScopeSelector))
            {
                // Is the scope selector key present?
                String scopeKey = $"{this.ScopeSelector}.{this.WhereFilter}";
                object scope = null;
                if (!scopes.TryGetValue(scopeKey, out scope))
                {
                    var scopeProperty = context.Target.GetType().GetRuntimeProperty(this.ScopeSelector);
                    var scopeValue = scopeProperty.GetValue(context.Target);
                    scope = scopeValue;
                    if (!String.IsNullOrEmpty(this.WhereFilter))
                    {
                        scope = this.m_scopeSelectMethod.Invoke(null, new Object[] { scopeValue, this.m_compiledExpression });
                    }

                    lock (scopes)
                    {
                        if (!scopes.ContainsKey(scopeKey))
                        {
                            scopes.Add(scopeKey, scope);
                        }
                    }
                }
                setValue = this.m_setter.Invoke(scope);
            }
            else
            {
                setValue = this.m_setter.Invoke(context);
            }

            return setValue;
        }

        /// <summary>
        /// Evaluate the specified action on the object
        /// </summary>
        public override object Evaluate(Act act, CdssContext<Patient> context, IDictionary<String, Object> scopes)
        {
            var propertyInfo = act.GetType().GetRuntimeProperty(this.PropertyName);

            if (this.Element != null)
            {
                propertyInfo.SetValue(act, this.Element);
            }
            else
            {
                var setValue = this.GetValue(act, context, scopes);

                //exp.TypeRegistry.RegisterSymbol("data", expressionParm);
                if (Core.Model.Map.MapUtil.TryConvert(setValue, propertyInfo.PropertyType, out setValue))
                {
                    propertyInfo.SetValue(act, setValue);
                }
            }

            return propertyInfo.GetValue(act);
        }
    }

    /// <summary>
    /// Add something to a property collection
    /// </summary>
    [XmlType(nameof(PropertyAddAction), Namespace = "http://santedb.org/cdss")]
    public class PropertyAddAction : PropertyAction
    {
        /// <summary>
        /// Evaluate
        /// </summary>
        public override object Evaluate(Act act, CdssContext<Patient> context, IDictionary<String, Object> scopes)
        {
            var value = act.GetType().GetRuntimeProperty(this.PropertyName) as IList;
            value?.Add(this.Element);
            return value;
        }

        /// <summary>
        /// Add action
        /// </summary>
        public override string ActionName
        {
            get
            {
                return "Add";
            }
        }
    }
}