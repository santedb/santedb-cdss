using Newtonsoft.Json;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Cdss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Expressions
{
    /// <summary>
    /// CDSS fact reference 
    /// </summary>
    [XmlType(nameof(CdssFactReferenceExpressionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssFactReferenceExpressionDefinition : CdssExpressionDefinition
    {

        /// <summary>
        /// Reference to the fact name
        /// </summary>
        [XmlAttribute("ref"), JsonProperty("ref")]
        public String FactName { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if(String.IsNullOrEmpty(this.FactName))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.expression.fact.missingReference", "Fact reference expressions require a @ref attribute", Guid.Empty);
            }
            else if(!context.Facts.Contains(this.FactName.ToLowerInvariant()))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.expression.fact.notFound", $"Could not find a fact in scope named {this.FactName}", Guid.Empty);
            }
        }

        /// <summary>
        /// Generate the expression for the lookup of the fact
        /// </summary>
        internal override Expression GenerateComputableExpression(CdssExecutionContext cdssContext, params ParameterExpression[] parameters)
        {
            // We want to create an equivalent expression as : context[factName] call 
            var contextParameter = parameters.First(o => o.Name == CdssConstants.ContextVariableName || typeof(ICdssExecutionContext).IsAssignableFrom(o.Type));
            return Expression.Call(contextParameter, typeof(CdssExecutionContext).GetMethod(nameof(CdssExecutionContext.GetFact)), Expression.Constant(this.FactName));
        }
    }
}
