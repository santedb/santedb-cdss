using DynamicExpresso;
using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Cdss.Xml.i18n;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.i18n;
using SanteDB.Core.Model;
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
        internal override Expression GenerateComputableExpression(CdssExecutionContext cdssContext, params ParameterExpression[] parameters)
        {
            try
            {
                
                return cdssContext.GetExpressionInterpreter().Parse(this.ExpressionValue, parameters.Select(o => new Parameter(o)).ToArray()).Expression;
            }
            catch(Exception e)
            {
                throw new CdssEvaluationException($"{e.Message} - \"{this.ExpressionValue}\"", e);
            }
        }

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if(String.IsNullOrEmpty(this.ExpressionValue))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.expression.csharp.missingLogic", "C# expression logic missing", Guid.Empty);
            }
            else
            {
                var identifiers = new Interpreter(InterpreterOptions.Default)
                  .SetVariable("context", null, typeof(CdssExecutionContext))
                  .SetVariable("value", null, typeof(object))
                  .SetVariable("scopedObject", null, typeof(IdentifiedData))
                  .DetectIdentifiers(this.ExpressionValue);
                if (identifiers.UnknownIdentifiers.Any())
                {
                    yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.expression.csharp.unknownId", $"Unknown identifiers {String.Join(",", identifiers.UnknownIdentifiers.Select(o => o.ToString()))} in C# expression {this.ExpressionValue}", Guid.Empty, this.ToString());
                }
                else if (this.ExpressionValue.Count(o => o == '[') != this.ExpressionValue.Count(o => o == ']'))
                {
                    yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.expression.csharp.bracketMismatch", $"Missing indexer close/open bracket in {this.ExpressionValue}", Guid.Empty, this.ToString());
                }
                else if (this.ExpressionValue.Count(o => o == '(') != this.ExpressionValue.Count(o => o == ')'))
                {
                    yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.expression.csharp.bracketMismatch", $"Missing parentheses close/open in {this.ExpressionValue}", Guid.Empty, this.ToString());
                }
            }
        }
    }
}
