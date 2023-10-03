using DynamicExpresso;
using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
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
        internal override Expression GenerateComputableExpression(CdssContext cdssContext, params ParameterExpression[] parameters) => 
            Expression.Invoke(cdssContext.GetExpressionInterpreter().Parse(this.ExpressionValue, parameters.Select(o=> new Parameter(o)).ToArray()).Expression, parameters);

    }
}
