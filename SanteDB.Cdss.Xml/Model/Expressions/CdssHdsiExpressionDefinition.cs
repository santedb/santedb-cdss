using Newtonsoft.Json;
using SanteDB.Core.Model.Query;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
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
        /// Gets or sets the HDSI syntax expression
        /// </summary>
        [XmlText, JsonProperty("expression")]
        public String ExpressionValue { get; set; }

        /// <inheritdoc/>
        internal override Expression GenerateComputableExpression<TContext>(CdssContext<TContext> cdssContext, ParameterExpression parameterExpression)
        {

            var variableDictionary = new Dictionary<String, Func<Object>>();
            foreach (var varRef in cdssContext.Variables)
            {
                variableDictionary.Add(varRef, () => CdssExecutionContext.Current?.GetValue(varRef));
            }

            Expression bodyExpression = null;
            if (this.ExpressionValue.Contains("="))
            {
                bodyExpression = QueryExpressionParser.BuildLinqExpression<TContext>(this.ExpressionValue.ParseQueryString(), "s", variableDictionary, safeNullable: true, forceLoad: true, lazyExpandVariables: true);
            }
            else
            {
                bodyExpression = QueryExpressionParser.BuildPropertySelector<TContext>(this.ExpressionValue, true);
            }

            return Expression.Invoke(bodyExpression, Expression.MakeMemberAccess(parameterExpression, typeof(CdssContext<TContext>).GetProperty(nameof(CdssContext<TContext>.Target))));
        }
    }
}
