using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Cdss.Xml.Model.Expressions;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Model;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Actions
{
    /// <summary>
    /// Represents an action that assigns a property value
    /// </summary>
    [XmlType(nameof(CdssPropertyAssignActionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssPropertyAssignActionDefinition : CdssActionDefinition
    {

        // The compiled expression
        private Func<object, object, object> m_compiledExpression;

        /// <summary>
        /// View of the compiled expression source
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public String DebugView { get; private set; }

        /// <summary>
        /// Gets or sets the name of the property to assign
        /// </summary>
        [XmlAttribute("path"), JsonProperty("path")]
        public String Path { get; set; }

        /// <summary>
        /// Overwrite the property
        /// </summary>
        [XmlAttribute("overwrite"), JsonProperty("overwrite")]
        public bool OverwriteValue { get; set; }

        /// <summary>
        /// Expressions which this aggregate is made up of
        /// </summary>
        [XmlElement("csharp", typeof(CdssCsharpExpressionDefinition)),
            XmlElement("hdsi", typeof(CdssHdsiExpressionDefinition)),
            XmlElement("query", typeof(CdssQueryExpressionDefinition)),
            XmlElement("fact", typeof(CdssFactReferenceExpressionDefinition)),
            XmlElement("fixed", typeof(String)), JsonProperty("expression")]
        public Object ContainedExpression { get; set; }


        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if (this.ContainedExpression == null)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.assign.property", "Assign action requires a setter expression", Guid.Empty, this.ToReferenceString());
            }
            if (String.IsNullOrEmpty(this.Path))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.assign.path", "Assign action requires a path expression", Guid.Empty, this.ToReferenceString());
            }
            foreach (var itm in base.Validate(context).Union((this.ContainedExpression as CdssExpressionDefinition)?.Validate(context) ?? new DetectedIssue[0]))
            {
                itm.RefersTo = itm.RefersTo ?? this.ToReferenceString();
                yield return itm;
            }
        }

        /// <inheritdoc/>
        internal override void Execute()
        {
            base.ThrowIfInvalidState();

            using (CdssExecutionStackFrame.EnterChildFrame(this))
            {
                try
                {
                    switch (this.ContainedExpression)
                    {
                        case CdssExpressionDefinition exe:
                            if (this.m_compiledExpression == null)
                            {
                                var uncompiledExpression = exe.GenerateComputableExpression();
                                this.DebugView = uncompiledExpression.ToString();
                                this.m_compiledExpression = uncompiledExpression.Compile();
                            }
                            var value = this.m_compiledExpression(CdssExecutionStackFrame.Current.Context, CdssExecutionStackFrame.Current.ScopedObject);
                            CdssExecutionStackFrame.Current.ScopedObject.GetOrSetValueAtPath(this.Path, value, this.OverwriteValue);

                            break;
                        case String str:
                            CdssExecutionStackFrame.Current.ScopedObject.GetOrSetValueAtPath(this.Path, str, this.OverwriteValue);
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
                catch (Exception e) when (!(e is CdssEvaluationException))
                {
                    throw new CdssEvaluationException($"Error computing {this.Name ?? this.Id}", e);
                }
            }
        }
    }
}