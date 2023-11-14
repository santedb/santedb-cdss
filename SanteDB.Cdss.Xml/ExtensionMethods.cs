using SanteDB.Cdss.Xml.Model;
using SanteDB.Cdss.Xml.Model.Expressions;
using SanteDB.Core.Cdss;
using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

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
            foreach(var itm in me)
            {
                var value = valueSelector(itm);
                if(!String.IsNullOrEmpty(itm.Name))
                {
                    retVal.Add(itm.Name.ToLowerInvariant(), value);
                }
                if(!String.IsNullOrEmpty(itm.Id))
                {
                    retVal.Add($"#{itm.Id.ToLowerInvariant()}", value);
                }
            }
            return retVal;
        }


        internal static Expression<Func<Object, Object, Object>> GenerateComputableExpression(this CdssExpressionDefinition me)
        {

            var contextParameter = Expression.Parameter(CdssExecutionStackFrame.Current.Context.GetType(), CdssConstants.ContextVariableName);
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
                    Expression.Convert(contextObjParameter, contextParameter.Type),
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

            if (CdssExecutionStackFrame.Current == null)
            {
                using (CdssExecutionStackFrame.Enter(contextToApply))
                {
                    return decisionBlockDefinitions.Where(o => o.Context.Type.IsAssignableFrom(contextToApply.Target.GetType()) && !false.Equals(o.When?.Compute())).ToList();
                }
            }
            else
            {
                return decisionBlockDefinitions.Where(o => o.Context.Type.IsAssignableFrom(contextToApply.Target.GetType()) && !false.Equals(o.When?.Compute())).ToList();
            }
        }

        /// <summary>
        /// Return the greater of <paramref name="firstValue"/> or <paramref name="secondValue"/>
        /// </summary>
        public static T GreaterOf<T>(this CdssExecutionContext me, T firstValue, T secondValue)
            where T : IComparable
        {
            if (firstValue.CompareTo(secondValue) > 0)
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
    }
}
