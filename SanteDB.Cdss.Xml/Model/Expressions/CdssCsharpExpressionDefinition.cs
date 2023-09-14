using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Expressions
{

    /// <summary>
    /// C# based expression
    /// </summary>
    [XmlType(nameof(CdssCsharpExpressionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssCsharpExpressionDefinition : CdssExpressionDefinition
    {

        /// <summary>
        /// Expression value
        /// </summary>
        [XmlText, JsonProperty("expression")]
        public String ExpressionValue { get; set; }

        /// <inheritdoc/>
        internal override Expression GenerateComputableExpression<TContext>(CdssContext<TContext> cdssContext, ParameterExpression parameterExpression) => 
            Expression.Invoke(cdssContext.GetExpressionInterpreter().ParseAsExpression<Func<CdssContext<TContext>, object>>(this.ExpressionValue, "context"), parameterExpression);

    }
}
