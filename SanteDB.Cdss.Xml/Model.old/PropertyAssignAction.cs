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
using Newtonsoft.Json;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.Roles;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using System;
using SanteDB.Core.Model.Entities;

namespace SanteDB.Cdss.Xml.Model
{

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
        [XmlAttribute("scope"), JsonProperty("scope")]
        public String ScopeSelector { get; set; }

        /// <summary>
        /// Where filter for scope
        /// </summary>
        [XmlAttribute("where"), JsonProperty("where")]
        public String WhereFilter { get; set; }

        /// <summary>
        /// Value expression
        /// </summary>
        [XmlText(), JsonProperty("value")]
        public String ValueExpression { get; set; }

        /// <summary>
        /// Get the specified value
        /// </summary>
        public object GetValue(Act act, CdssContext context, IDictionary<String, Object> scopes)
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
                    this.m_setter = interpretor.Parse(this.ValueExpression, new Parameter("_", typeof(CdssContext)));
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
        public override object Evaluate(Act act, CdssContext context, IDictionary<String, Object> scopes)
        {
            var propertyInfo = act.GetType().GetRuntimeProperty(this.Name);

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

}