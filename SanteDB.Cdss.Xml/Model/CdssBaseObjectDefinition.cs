using Newtonsoft.Json;
using SanteDB.Core.BusinessRules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// CDSS base object
    /// </summary>
    [XmlType(nameof(CdssBaseObjectDefinition), Namespace = "http://santedb.org/cdss")]
    public abstract class CdssBaseObjectDefinition  
    {

        /// <summary>
        /// A unique name which is used to reference the object in the base
        /// </summary>
        [XmlAttribute("id"), JsonProperty("id")]
        public String Id { get; set; }

        /// <summary>
        /// A descriptive name
        /// </summary>
        [XmlAttribute("name"), JsonProperty("name")]
        public String Name { get; set; }

        /// <summary>
        /// The UUID for the CDSS base object
        /// </summary>
        [XmlAttribute("uuid"), JsonProperty("uuid")]
        public Guid Uuid { get; set; }

        /// <summary>
        /// True if UUID is specified
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public bool UuidSpecified { get; set; }

        /// <summary>
        /// Gets or sets the status for the CDSS object
        /// </summary>
        [XmlElement("status"), JsonProperty("status")]
        public CdssObjectState Status { get; set; }

        /// <summary>
        /// True if status has been specified
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public bool StatusSpecified { get; set; }

        /// <summary>
        /// Gets or sets the metadata related to the cdss object
        /// </summary>
        [XmlElement("meta"), JsonProperty("meta")]
        public CdssObjectMetadata Metadata { get; set; }

        /// <summary>
        /// Gets or sets the transpiled metadata
        /// </summary>
        [XmlElement("source"), JsonProperty("source")]
        public CdssTranspileMapMetaData TranspileSourceReference { get; set; }

        /// <summary>
        /// Gets or sets the object identifier
        /// </summary>
        [XmlAttribute("oid"), JsonProperty("oid")]
        public String Oid { get; set; }

        /// <inheritdoc/>
        public override string ToString() => $"{this.GetType().Name} {this.Name} {(!String.IsNullOrEmpty(this.Id) ? $"(#{this.Id})" : "")}";

        /// <summary>
        /// Validate the object definition
        /// </summary>
        public abstract IEnumerable<DetectedIssue> Validate(CdssExecutionContext context);

        /// <summary>
        /// Represent this as a source code reference string
        /// </summary>
        /// <returns></returns>
        public string ToReferenceString() => $"{this.GetType().Name} {this.TranspileSourceReference?.SourceFileName ?? this.Name} @{this.TranspileSourceReference?.StartPosition}";

        /// <summary>
        /// Creat a shallow clone
        /// </summary>
        public CdssBaseObjectDefinition Clone() => (CdssBaseObjectDefinition)this.MemberwiseClone();
    }
}
