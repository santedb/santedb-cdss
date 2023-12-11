using Newtonsoft.Json;
using SanteDB.Core.BusinessRules;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Diagnostics
{
    /// <summary>
    /// Represents a reference to an object
    /// </summary>
    [XmlType(nameof(CdssDiagnosticObjectReference), Namespace = "http://santedb.org/cdss")]
    public class CdssDiagnosticObjectReference : CdssBaseObjectDefinition
    {

        /// <summary>
        /// Serialization ctor
        /// </summary>
        public CdssDiagnosticObjectReference()
        {
            
        }

        /// <summary>
        /// Creates a new report for an object reference
        /// </summary>
        /// <param name="referencedObject">The object being referenced</param>
        internal CdssDiagnosticObjectReference(CdssBaseObjectDefinition referencedObject)
        {
            this.Id = referencedObject.Id;
            this.Name = referencedObject.Name;
            this.Oid = referencedObject.Oid;
            this.TranspileSourceReference = referencedObject.TranspileSourceReference;
            this.Uuid = referencedObject.Uuid;
            this.UuidSpecified = referencedObject.UuidSpecified;
            this.ReferenceType = referencedObject.GetType().Name;
        }

        /// <summary>
        /// Gets or sets the type that is being referenced
        /// </summary>
        [XmlAttribute("type"), JsonProperty("type")]
        public string ReferenceType { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            yield break;
        }
    }
}
