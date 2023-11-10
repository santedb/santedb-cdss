using Newtonsoft.Json;
using SanteDB.Core.BusinessRules;
using System;
using System.Collections.Generic;
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
        /// Gets or sets the status for the CDSS object
        /// </summary>
        [XmlElement("status"), JsonProperty("status")]
        public CdssObjectState Status { get; set; }

        /// <summary>
        /// Gets or sets the metadata related to the cdss object
        /// </summary>
        [XmlElement("meta"), JsonProperty("meta")]
        public CdssObjectMetadata Metadata { get; set; }

        /// <summary>
        /// Gets or sets the object identifier
        /// </summary>
        [XmlAttribute("oid"), JsonProperty("oid")]
        public String Oid { get; set; }

    }
}
