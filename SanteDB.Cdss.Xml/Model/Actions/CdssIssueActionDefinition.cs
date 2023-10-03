using Newtonsoft.Json;
using SanteDB.Core.BusinessRules;
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
                cdssContext.PushIssue(this.IssueToRaise);
            }
        }
    }
}