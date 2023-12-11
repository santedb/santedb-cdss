using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// Metadata about an object in the CDSS subsystem
    /// </summary>
    [XmlType(nameof(CdssObjectMetadata), Namespace = "http://santedb.org/cdss")]
    public class CdssObjectMetadata
    {

        /// <summary>
        /// Gets or sets the authors for the object
        /// </summary>
        [XmlArray("authors"), XmlArrayItem("add"), JsonProperty("authors")]
        public List<string> Authors { get; set; }

        /// <summary>
        /// Gets or sets the version code of the CDSS object
        /// </summary>
        [XmlElement("version"), JsonProperty("version")]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the documentation of the object
        /// </summary>
        [XmlElement("documentation"), JsonProperty("documentation")]
        public string Documentation { get; set; }
    }
}