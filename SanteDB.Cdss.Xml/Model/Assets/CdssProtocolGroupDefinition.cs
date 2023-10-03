using Newtonsoft.Json;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Assets
{

    /// <summary>
    /// Represents an individual grouping
    /// </summary>
    [XmlType(nameof(CdssProtocolGroupDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssProtocolGroupDefinition : CdssBaseObjectDefinition
    {

        /// <summary>
        /// Gets or sets the oid of the group
        /// </summary>
        [XmlAttribute("oid"), JsonProperty("oid")]       
        public string Oid { get; set; }

    }
}