using Newtonsoft.Json;
using SanteDB.Core.BusinessRules;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Assets
{

    /// <summary>
    /// Represents an individual grouping
    /// </summary>
    [XmlType(nameof(CdssProtocolGroupDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssProtocolGroupDefinition : CdssBaseObjectDefinition
    {

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if(string.IsNullOrEmpty(this.Oid) && string.IsNullOrEmpty(this.Name))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.protocol.group.unidentified", "CDSS protocol groups must carry either OID or Name (or both)", Guid.Empty, this.ToReferenceString());
            }
        }
    }
}