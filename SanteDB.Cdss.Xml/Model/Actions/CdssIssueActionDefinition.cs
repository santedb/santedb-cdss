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
        internal override void Execute(CdssContext cdssContext)
        {
            using(CdssExecutionContext.EnterChildContext(this))
            {
                var issue = new DetectedIssue(this.IssueToRaise.Priority, this.IssueToRaise.Id, this.IssueToRaise.Text, this.IssueToRaise.TypeKey, CdssExecutionContext.Current.ScopedObject.Key.GetValueOrDefault());
                cdssContext.PushIssue(this.IssueToRaise);
            }
        }
    }
}