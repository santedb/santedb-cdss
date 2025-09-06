/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2024-6-21
 */
using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Cdss;
using SanteDB.Core.i18n;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Expressions
{
    /// <summary>
    /// An HDSI based expression
    /// </summary>
    [XmlType(nameof(CdssHdsiExpressionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssHdsiExpressionDefinition : CdssExpressionDefinition
    {

        /// <summary>
        /// Default CTOR
        /// </summary>
        public CdssHdsiExpressionDefinition()
        {
        }

        /// <summary>
        /// Create a new HDSI expression definition from a string
        /// </summary>
        /// <param name="hdsiExpression">The expression to set</param>
        public CdssHdsiExpressionDefinition(string hdsiExpression)
        {
            this.ExpressionValue = hdsiExpression;
        }

        /// <summary>
        /// Gets or sets where the object should be pulled from
        /// </summary>
        [XmlAttribute("scope"), JsonProperty("scope")]
        public CdssHdsiExpressionScopeType Scope { get; set; }

        /// <summary>
        /// Gets or sets the fact reference
        /// </summary>
        [XmlAttribute("fact"), JsonProperty("fact")]
        public String ScopedFact { get; set; }

        /// <summary>
        /// Negate the HDSI expression
        /// </summary>
        [XmlAttribute("negate"), JsonProperty("negate")]
        public bool IsNegated { get; set; }

        /// <summary>
        /// Gets or sets the HDSI syntax expression
        /// </summary>
        [XmlText, JsonProperty("expression")]
        public String ExpressionValue { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if (String.IsNullOrEmpty(this.ExpressionValue))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.expression.hdsi.missingExpression", "HDSI expression require a property selector or binary expression", Guid.Empty, this.ToReferenceString());
            }
            if(this.Scope == CdssHdsiExpressionScopeType.Fact && String.IsNullOrEmpty(this.ScopedFact))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.expression.hdsi.factRef", "HDSI scoped to fact must a fact reference", Guid.Empty, this.ToReferenceString());
            }

        }

        /// <inheritdoc/>
        internal override Expression GenerateComputableExpression(CdssExecutionContext cdssContext, params ParameterExpression[] parameters)
        {

            var cdssParameter = parameters.First(o => o.Name == CdssConstants.ContextVariableName);
            var scopedObjectParameter = parameters.First(o => o.Name == CdssConstants.ScopedObjectVariableName);

            var variableDictionary = new Dictionary<String, Func<Object>>();
            foreach (var varRef in cdssContext.Variables.Union(cdssContext.FactNames ?? new String[0]))
            {
                variableDictionary.Add(varRef, () => CdssExecutionStackFrame.Current?.GetValue(varRef));
            }

            Expression scopedObjectExpression = null;
            switch (this.Scope)
            {
                case CdssHdsiExpressionScopeType.Fact:
                    if(!cdssContext.TryGetFact(this.ScopedFact, out var data))
                    {
                        throw new CdssEvaluationException(String.Format(ErrorMessages.OBJECT_NOT_FOUND, this.ScopedFact));
                    }

                    var factType = data?.GetType() ?? typeof(IdentifiedData);
                    scopedObjectExpression = Expression.Convert(Expression.Call(cdssParameter, typeof(CdssExecutionContext).GetMethod(nameof(CdssExecutionContext.GetFact)), Expression.Constant(this.ScopedFact)), factType);
                    break;
                case CdssHdsiExpressionScopeType.Context:
                    scopedObjectExpression = Expression.MakeMemberAccess(cdssParameter, (MemberInfo)cdssParameter.Type.GetProperty(nameof(ICdssExecutionContext.Target)));
                    break;
                default:
                    var scopedObjectType = CdssExecutionStackFrame.Current.ScopedObject.GetType();
                    scopedObjectExpression = Expression.Convert(scopedObjectParameter, scopedObjectType);
                    break;

            }

            LambdaExpression bodyExpression = null;
            if (this.ExpressionValue.Contains("="))
            {
                bodyExpression = QueryExpressionParser.BuildLinqExpression(scopedObjectExpression.Type, this.ExpressionValue.ParseQueryString(), "s", variableDictionary, safeNullable: true, alwaysCoalesce: true, forceLoad: true, lazyExpandVariables: true);
                if (this.IsNegated)
                {
                    bodyExpression = Expression.Lambda(Expression.Not(bodyExpression.Body), bodyExpression.Parameters);
                }
            }
            else
            {
                bodyExpression = QueryExpressionParser.BuildPropertySelector(scopedObjectExpression.Type, this.ExpressionValue, true);
            }

            var retVal = Expression.Invoke(bodyExpression, scopedObjectExpression);
            return retVal;
        }
    }

    /// <summary>
    /// CDS expression scope
    /// </summary>
    [XmlType(nameof(CdssHdsiExpressionScopeType), Namespace = "http://santedb.org/cdss")]
    public enum CdssHdsiExpressionScopeType
    {
        /// <summary>
        /// Scoped to a specific fact
        /// </summary>
        [XmlEnum("fact")]
        Fact = 2,
        /// <summary>
        /// Get the value from the proposed object
        /// </summary>
        [XmlEnum("scopedObject")]
        CurrentObject = 1,
        /// <summary>
        /// Get the value from the current context object
        /// </summary>
        [XmlEnum("context")]
        Context = 0
    }
}
