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
 * Date: 2023-11-27
 */
using SanteDB.Cdss.Xml.Model;
using SanteDB.Cdss.Xml.Model.Assets;
using SanteDB.Cdss.Xml.Model.Expressions;
using SanteDB.Core.Cdss;
using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SanteDB.Cdss.Xml
{
    /// <summary>
    /// Extension methods
    /// </summary>
    internal static class ExtensionMethods
    {

        /// <summary>
        /// Creates a CDSS reference lookup where the keys can be eiter referenced by "name" or "#id"
        /// </summary>
        /// <typeparam name="TElement">The element type of the input array</typeparam>
        /// <typeparam name="TValue">The value type in the dictionary</typeparam>
        /// <param name="me">The collection of <see cref="CdssBaseObjectDefinition"/></param>
        /// <param name="valueSelector">The value of the resulting dictionary</param>
        /// <returns>The constructed dictionary</returns>
        internal static IDictionary<String, TValue> ToCdssReferenceDictionary<TElement, TValue>(this IEnumerable<TElement> me, Func<TElement, TValue> valueSelector)
            where TElement : CdssBaseObjectDefinition
        {
            var retVal = new Dictionary<String, TValue>();
            foreach (var itm in me)
            {
                var value = valueSelector(itm);
                int priority = 0;
                if (value is CdssComputableAssetDefinition ccad)
                {
                    priority = ccad.Priority;
                }

                if (!String.IsNullOrEmpty(itm.Name))
                {
                    var key = itm.Name.ToLowerInvariant();
                    if (!retVal.TryGetValue(key, out var existing) ||
                        existing is CdssComputableAssetDefinition ccad2 && ccad2.Priority < priority)
                    {
                        retVal.Remove(key);
                        retVal.Add(key, value);

                    }
                }
                if (!String.IsNullOrEmpty(itm.Id))
                {
                    var key = $"#{itm.Id.ToLowerInvariant()}";
                    if (!retVal.TryGetValue(key, out var existing) ||
                        existing is CdssComputableAssetDefinition ccad2 && ccad2.Priority < priority)
                    {
                        retVal.Remove(key);
                        retVal.Add(key, value);

                    }
                }
            }
            return retVal;
        }


        internal static Expression<Func<Object, Object, Object>> GenerateComputableExpression(this CdssExpressionDefinition me, Type contextTargetObjectType)
        {

            contextTargetObjectType = contextTargetObjectType ?? CdssExecutionStackFrame.Current.Context.TargetType;

            var contextParameter = Expression.Parameter(typeof(CdssExecutionContext<>).MakeGenericType(contextTargetObjectType), CdssConstants.ContextVariableName);
            var scopeParameter = Expression.Parameter(typeof(IdentifiedData), CdssConstants.ScopedObjectVariableName);

            var expressionForValue = me.GenerateComputableExpression(CdssExecutionStackFrame.Current.Context, contextParameter, scopeParameter);
            if (!(expressionForValue is LambdaExpression))
            {
                expressionForValue = Expression.Lambda(expressionForValue, contextParameter, scopeParameter);
            }

            // Convert object parameters
            var contextObjParameter = Expression.Parameter(typeof(Object));
            var scopeObjParameter = Expression.Parameter(typeof(Object));
            expressionForValue = Expression.Convert(Expression.Invoke(
                    expressionForValue,
                    Expression.Call(
                        Expression.Convert(contextObjParameter, typeof(CdssExecutionContext)),
                        (MethodInfo)typeof(CdssExecutionContext).GetGenericMethod(nameof(CdssExecutionContext.Wrap), new Type[] { contextTargetObjectType }, Type.EmptyTypes)
                    ),
                    Expression.Convert(scopeObjParameter, scopeParameter.Type)
                ), typeof(Object));

            return Expression.Lambda<Func<object, object, object>>(
                expressionForValue,
                contextObjParameter,
                scopeObjParameter
            );

        }

        /// <summary>
        /// Determine whether a logic block applies to an object
        /// </summary>
        internal static IEnumerable<CdssDecisionLogicBlockDefinition> AppliesTo(this IEnumerable<CdssDecisionLogicBlockDefinition> decisionBlockDefinitions, ICdssExecutionContext contextToApply)
        {

            bool isForValidationOnly = contextToApply is CdssExecutionContext exe && exe.IsForValidation;

            if (CdssExecutionStackFrame.Current == null)
            {
                using (CdssExecutionStackFrame.Enter(contextToApply))
                {
                    return decisionBlockDefinitions.Where(o => o.Context.Type.IsAssignableFrom(contextToApply.Target.GetType()) && (isForValidationOnly || true.Equals(o.When?.Compute() ?? true) && o.Status != CdssObjectState.DontUse)).ToList();
                }
            }
            else
            {
                return decisionBlockDefinitions.Where(o => o.Context.Type.IsAssignableFrom(contextToApply.Target.GetType()) && (isForValidationOnly || true.Equals(o.When?.Compute() ?? true) && o.Status != CdssObjectState.DontUse)).ToList();
            }
        }


        /// <summary>
        /// Extends adding years to a date time to partial years
        /// </summary>
        public static DateTime AddYearsEx(this DateTime me, double years)
        {
            var nYears = (int)years;
            var nMonths = (int)((years - nYears) * 12);
            return me.AddYears(nYears).AddMonths(nMonths);
        }

        /// <summary>
        /// Extends adding years to a date time to partial years
        /// </summary>
        public static DateTimeOffset AddYearsEx(this DateTimeOffset me, double years)
        {
            var nYears = (int)years;
            var nMonths = (int)((years - nYears) * 12);
            return me.AddYears(nYears).AddMonths(nMonths);
        }


        /// <summary>
        /// Return the greater of <paramref name="firstValue"/> or <paramref name="secondValue"/>
        /// </summary>
        public static T GreaterOf<T>(this CdssExecutionContext me, T firstValue, T secondValue)
            where T : IComparable
        {
            if (firstValue.CompareTo((T)secondValue) > 0)
            {
                return firstValue;
            }
            else
            {
                return secondValue;
            }
        }
        
        /// <summary>
        /// Return the lesser of <paramref name="firstValue"/> or <paramref name="secondValue"/>
        /// </summary>
        public static T LesserOf<T>(this CdssExecutionContext me, T firstValue, T secondValue)
            where T : IComparable
        {
            if (firstValue.CompareTo(secondValue) < 0)
            {
                return firstValue;
            }
            else
            {
                return secondValue;
            }
        }

        /// <summary>
        /// Between INCLUSIVE
        /// </summary>
        public static bool Between<T>(this T me, T firstValue, T secondValue)
            where T : IComparable
        {
            return me.CompareTo(firstValue) >= 0 && me.CompareTo(secondValue) <= 0;
        }

    }
}
