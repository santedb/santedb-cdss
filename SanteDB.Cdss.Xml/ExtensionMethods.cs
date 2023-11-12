using SanteDB.Cdss.Xml.Model;
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
    }
}
