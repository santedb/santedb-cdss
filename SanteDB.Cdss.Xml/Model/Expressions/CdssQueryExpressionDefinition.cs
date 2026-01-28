/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.Model.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Expressions
{
    /// <summary>
    /// Represents a specialized <see cref="CdssHdsiExpressionDefinition"/> which is used to back-query from a data source
    /// </summary>
    [XmlType(nameof(CdssQueryExpressionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssQueryExpressionDefinition : CdssExpressionDefinition
    {

        /// <summary>
        /// Gets or sets where the object should be pulled from
        /// </summary>
        [XmlAttribute("scope"), JsonProperty("scope")]
        public CdssHdsiExpressionScopeType Scope { get; set; }

        /// <summary>
        /// Get or sets the ordering expression
        /// </summary>
        [XmlAttribute("order-by"), JsonProperty("orderBy")]
        public String OrderByHdsi { get; set; }

        /// <summary>
        /// Gets or sets the selector expression (the value to emit)
        /// </summary>
        [XmlAttribute("select"), JsonProperty("select")]
        public String SelectHdsi { get; set; }

        /// <summary>
        /// Gets or sets the expression on the context which is used to gather the colleciton of objects filtered
        /// </summary>
        [XmlAttribute("source"), JsonProperty("source")]
        public String SourceCollectionHdsi { get; set; }

        /// <summary>
        /// Gets or sets the fact reference
        /// </summary>
        [XmlAttribute("fact"), JsonProperty("fact")]
        public String ScopedFact { get; set; }

        /// <summary>
        /// Gets or sets the selector function from the matching records
        /// </summary>
        [XmlAttribute("fn"), JsonProperty("fn")]
        public CdssCollectionSelectorType SelectorFunction { get; set; }


        /// <summary>
        /// Gets or sets the HDSI syntax expression
        /// </summary>
        [XmlText, JsonProperty("expression")]
        public String FilterHdsi { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if (String.IsNullOrEmpty(this.SourceCollectionHdsi))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.expression.query.missingSource", "Source from which query should be executed is missing", Guid.Empty, this.ToReferenceString());
            }
            if (String.IsNullOrEmpty(this.FilterHdsi))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.expression.query.missingFilter", "Filter expression must be provided for query statement", Guid.Empty, this.ToReferenceString());
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
                    if (!cdssContext.TryGetFact(this.ScopedFact, out var data))
                    {
                        throw new CdssEvaluationException(String.Format(ErrorMessages.OBJECT_NOT_FOUND, this.ScopedFact));
                    }

                    var factType = data?.GetType() ?? typeof(IdentifiedData);
                    scopedObjectExpression = Expression.Convert(Expression.Call(cdssParameter, typeof(CdssExecutionContext).GetMethod(nameof(CdssExecutionContext.GetFact)), Expression.Constant(this.ScopedFact)), factType);
                    break;
                case CdssHdsiExpressionScopeType.Context:
                    scopedObjectExpression = Expression.MakeMemberAccess(cdssParameter, (MemberInfo)cdssContext.GetType().GetProperty(nameof(ICdssExecutionContext.Target)));
                    break;
                default:
                    var scopedObjectType = CdssExecutionStackFrame.Current.ScopedObject.GetType();
                    scopedObjectExpression = Expression.Convert(scopedObjectParameter, scopedObjectType);
                    break;

            }

            // Next we want to select the source of the collection
            LambdaExpression sourceCollectionExpression = QueryExpressionParser.BuildPropertySelector(scopedObjectExpression.Type, this.SourceCollectionHdsi, returnNewObjectOnNull: false, forceLoad: true, collectionResolutionMethod: null);
            // 
            if (!typeof(IEnumerable).IsAssignableFrom(sourceCollectionExpression.ReturnType))
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.ARGUMENT_INCOMPATIBLE_TYPE, typeof(IEnumerable), sourceCollectionExpression.Type));
            }

            // Build a linq expression for our filter
            var elementType = sourceCollectionExpression.ReturnType.GetGenericArguments()[0];
            var sourceFilterExpression = QueryExpressionParser.BuildLinqExpression(elementType, this.FilterHdsi.ParseQueryString(), "p", variables: variableDictionary, alwaysCoalesce: true, lazyExpandVariables: true, forceLoad: true, safeNullable: true);

            // Now we want to invoke WHERE on the collection
            var argType = typeof(Func<,>).MakeGenericType(elementType, typeof(bool));
            var whereExpressionInvokation = Expression.Call(null, (MethodInfo)typeof(Enumerable).GetGenericMethod(nameof(Enumerable.Where), new Type[] { elementType }, new Type[] { typeof(IEnumerable<>).MakeGenericType(elementType), argType }),
                Expression.Invoke(sourceCollectionExpression, scopedObjectExpression), sourceFilterExpression);

            if (!String.IsNullOrEmpty(this.OrderByHdsi))
            {
                var orderByExpression = QueryExpressionParser.BuildPropertySelector(elementType, this.OrderByHdsi);
                whereExpressionInvokation = Expression.Call(null, (MethodInfo)typeof(Enumerable).GetGenericMethod(nameof(Enumerable.OrderBy), new Type[] { elementType, orderByExpression.ReturnType }, new Type[] { whereExpressionInvokation.Type, orderByExpression.Type }),
                    whereExpressionInvokation, orderByExpression);

            }
            // Collect the proper value
            if (!String.IsNullOrEmpty(this.SelectHdsi))
            {
                var resultSelectorExpression = QueryExpressionParser.BuildPropertySelector(elementType, this.SelectHdsi);
                whereExpressionInvokation = Expression.Call(null, (MethodInfo)typeof(Enumerable).GetGenericMethod(nameof(Enumerable.Select), new Type[] { elementType, resultSelectorExpression.ReturnType }, new Type[] { whereExpressionInvokation.Type, resultSelectorExpression.Type }),
                    whereExpressionInvokation, resultSelectorExpression);
                elementType = resultSelectorExpression.ReturnType;

            }

            // Now collapse
            MethodInfo aggregateMethod = null;
            switch (this.SelectorFunction)
            {
                case CdssCollectionSelectorType.Last:
                    aggregateMethod = (MethodInfo)typeof(Enumerable).GetGenericMethod(nameof(Enumerable.LastOrDefault), new Type[] { elementType }, new Type[] { whereExpressionInvokation.Type });
                    break;
                case CdssCollectionSelectorType.Single:
                    aggregateMethod = (MethodInfo)typeof(Enumerable).GetGenericMethod(nameof(Enumerable.SingleOrDefault), new Type[] { elementType }, new Type[] { whereExpressionInvokation.Type });
                    break;
                default:
                    aggregateMethod = (MethodInfo)typeof(Enumerable).GetGenericMethod(nameof(Enumerable.FirstOrDefault), new Type[] { elementType }, new Type[] { whereExpressionInvokation.Type });
                    break;
            }

            whereExpressionInvokation = Expression.Call(null, aggregateMethod, whereExpressionInvokation);

            
            return whereExpressionInvokation;

        }
    }
}
