using Newtonsoft.Json;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Cdss;
using System.Collections.Generic;
using System;
using System.Xml.Serialization;
using SanteDB.Cdss.Xml.Exceptions;

namespace SanteDB.Cdss.Xml.Model.Actions
{
    /// <summary>
    /// Raise an issue add it to the analysis
    /// </summary>
    [XmlType(nameof(CdssIssueActionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssIssueActionDefinition : CdssActionDefinition
    {

        /// <summary>
        /// The issue to raise
        /// </summary>
        [XmlElement("issue", Namespace = "http://santedb.org/issue"), JsonProperty("issue")]
        public DetectedIssue IssueToRaise { get; set; }

        /// <inheritdoc/>
        internal override void Execute()
        {
            base.ThrowIfInvalidState();

            using (CdssExecutionStackFrame.EnterChildFrame(this))
            {
                try
                {
                    var issue = new DetectedIssue(this.IssueToRaise.Priority, this.IssueToRaise.Id, this.IssueToRaise.Text, this.IssueToRaise.TypeKey, CdssExecutionStackFrame.Current.ScopedObject.ToString());
                    CdssExecutionStackFrame.Current.Context.PushIssue(this.IssueToRaise);
                }
                catch (Exception e) when (!(e is CdssEvaluationException))
                {
                    throw new CdssEvaluationException($"Error computing {this.Name ?? this.Id}", e);
                }
            }
        }


        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if (this.IssueToRaise == null)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.raise.issue", "Raise action requires a detected issue", Guid.Empty, this.ToString());
            }
            foreach (var itm in base.Validate(context))
            {
                itm.RefersTo = itm.RefersTo ?? this.ToString();
                yield return itm;
            }
        }
    }
}