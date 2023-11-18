using Newtonsoft.Json;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Data.Import;
using SanteDB.Core.Data.Import.Format;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// CDSS dataset definition
    /// </summary>
    [XmlType(nameof(CdssDatasetDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssDatasetDefinition : CdssBaseObjectDefinition
    {

        /// <summary>
        /// Reference data
        /// </summary>
        [XmlText(), JsonProperty("csv")]
        public string RawData { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if(string.IsNullOrEmpty(this.RawData))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.dataset.dataMissing", "Reference data provided in a CDSS library must contain CSV data", Guid.Empty, this.ToString());
            }
            if(String.IsNullOrEmpty(this.Id) || string.IsNullOrEmpty(this.Name))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.dataset.unidentified", "Reference data sets provided in CDSS libraries must contain a name", Guid.Empty, this.ToString());
            }
        }
    }
}