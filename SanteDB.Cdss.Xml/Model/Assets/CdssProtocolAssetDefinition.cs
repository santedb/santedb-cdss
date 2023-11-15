using Newtonsoft.Json;
using SanteDB.Core.BusinessRules;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Assets
{
    /// <summary>
    /// CDSS clinical protocol definition
    /// </summary>
    /// <remarks>A protocol is essentially a rule, however the protocol signals to 
    /// the interpreter that the protocol is an "entry point" for each rule</remarks>
    [XmlType(nameof(CdssProtocolAssetDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssProtocolAssetDefinition : CdssRuleAssetDefinition
    {

        /// <summary>
        /// Gets or sets the scopes where this protocol should be applied
        /// </summary>
        [XmlArray("scopes"), XmlArrayItem("add"), JsonProperty("scopes")]
        public List<CdssProtocolGroupDefinition> Scopes { get; set; }

        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if(String.IsNullOrEmpty(this.Oid))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.protocol.oidMissing", "CDSS Protocols must carry an OID", Guid.Empty, this.ToString());
            }
            if(String.IsNullOrEmpty(this.Name))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.protocol.nameMissing", "CDSS Protocols must carry a NAME", Guid.Empty, this.ToString());
            }
            if(this.Uuid == Guid.Empty)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.protocol.uuidMissing", "CDSS Protocols must carry a UUID", Guid.Empty, this.ToString());
            }
            foreach (var itm in base.Validate(context))
            {
                yield return itm;
            }
        }
    }
}
