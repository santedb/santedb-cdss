using DynamicExpresso;
using Newtonsoft.Json;
using SanteDB.Core.BusinessRules;
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
        /// Defualt ctor
        /// </summary>
        public CdssCsharpExpressionDefinition()
        {
                
        }

        /// <summary>
        /// Create a new CDSS C# expression based on <paramref name="expression"/>
        /// </summary>
        public CdssCsharpExpressionDefinition(String expression)
        {
            this.ExpressionValue = expression;
        }

        /// <summary>
        /// Expression value
        /// </summary>
        [XmlText, JsonProperty("expression")]
        public String ExpressionValue { get; set; }

        /// <inheritdoc/>
        internal override Expression GenerateComputableExpression(CdssExecutionContext cdssContext, params ParameterExpression[] parameters) => 
            cdssContext.GetExpressionInterpreter().Parse(this.ExpressionValue, parameters.Select(o=> new Parameter(o)).ToArray()).Expression;

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if(String.IsNullOrEmpty(this.ExpressionValue))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.expression.csharp.missingLogic", "C# expression logic missing", Guid.Empty);
            }
        }
    }
}
