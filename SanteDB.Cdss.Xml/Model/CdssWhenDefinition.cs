using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Model.Assets;
using SanteDB.Cdss.Xml.Model.Expressions;
using System.Collections.Generic;
using System.Linq.Expressions;
using System;
using System.Xml.Serialization;
using SanteDB.Cdss.Xml.Model.Actions;
using SanteDB.Core.BusinessRules;
using System.Xml;
using SanteDB.Core.Model;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// Defines a WHEN condition
    /// </summary>
    [XmlType(nameof(CdssWhenDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssWhenDefinition : CdssBaseObjectDefinition
    {


        // The expression which has been calculated
        private Func<object, object, object> m_compiledExpression;

        /// <summary>
        /// Gets the expression of the fact
        /// </summary>
        [XmlElement("csharp", typeof(CdssCsharpExpressionDefinition)),
         XmlElement("hdsi", typeof(CdssHdsiExpressionDefinition)),
         XmlElement("xml", typeof(CdssXmlLinqExpressionDefinition)),
         XmlElement("query", typeof(CdssQueryExpressionDefinition)),
         XmlElement("all", typeof(CdssAllExpressionDefinition)),
         XmlElement("none", typeof(CdssNoneExpressionDefinition)),
         XmlElement("any", typeof(CdssAnyExpressionDefinition)),
         XmlElement("fact", typeof(CdssFactReferenceExpressionDefinition)),
         JsonProperty("logic")]
        public CdssExpressionDefinition WhenComputation { get; set; }

        /// <summary>
        /// Debug view
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public string DebugView { get; private set; }

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if (this.WhenComputation == null)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.when.definitionMissing", "When condition block is missing logic", Guid.Empty, this.ToString());
            }
            else
            {
                foreach(var itm in this.WhenComputation.Validate(context))
                {
                    itm.RefersTo = itm.RefersTo ?? this.ToString();
                    yield return itm;
                }
            }
        }

        /// <inheritdoc/>
        public bool Compute()
        {
            if (this.m_compiledExpression == null)
            {
                var uncompiledExpression = this.WhenComputation.GenerateComputableExpression();

                this.DebugView = uncompiledExpression.ToString();
                this.m_compiledExpression = uncompiledExpression.Compile();
            }

            using (CdssExecutionStackFrame.EnterChildFrame(this))
            {
                var result = m_compiledExpression(CdssExecutionStackFrame.Current.Context, CdssExecutionStackFrame.Current.ScopedObject);
                return (bool)result;
            }
        }
    }
}