using Newtonsoft.Json;
using System;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// CDSS base object refernece
    /// </summary>
    [XmlType(nameof(CdssBaseObjectReference), Namespace = "http://santedb.org/cdss")]
    public class CdssBaseObjectReference
    {

        /// <summary>
        /// Reference to another object definition to be used in this object's place
        /// </summary>
        [XmlAttribute("ref"), JsonProperty("$ref")]
        public String Reference { get; set; }

    }
}