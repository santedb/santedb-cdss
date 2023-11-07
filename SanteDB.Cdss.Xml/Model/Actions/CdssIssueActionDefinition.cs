using Newtonsoft.Json;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Cdss;
using System.Xml.Serialization;

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

            using(CdssExecutionStackFrame.EnterChildFrame(this))
            {
                var issue = new DetectedIssue(this.IssueToRaise.Priority, this.IssueToRaise.Id, this.IssueToRaise.Text, this.IssueToRaise.TypeKey, CdssExecutionStackFrame.Current.ScopedObject.Key.GetValueOrDefault());
                CdssExecutionStackFrame.Current.Context.PushIssue(this.IssueToRaise);
            }
        }
    }
}