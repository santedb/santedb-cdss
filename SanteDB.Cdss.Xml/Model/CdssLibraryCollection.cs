using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// Library collection
    /// </summary>
    [XmlType(nameof(CdssLibraryCollection), Namespace = "http://santedb.org/cdss")]
    [XmlRoot(nameof(CdssLibraryCollection), Namespace = "http://santedb.org/cdss")]
    public class CdssLibraryCollection
    {

        private static XmlSerializer s_serializer = new XmlSerializer(typeof(CdssLibraryCollection));

        /// <summary>
        /// Library collection
        /// </summary>
        public CdssLibraryCollection()
        {
            this.Libraries = new List<CdssLibraryDefinition>();
        }

        /// <summary>
        /// Gets or sets the libraries
        /// </summary>
        [XmlElement("library"), JsonProperty("libraries")]
        public List<CdssLibraryDefinition> Libraries { get; set; }

        /// <summary>
        /// Save to stream
        /// </summary>
        public void Save(Stream stream) {
            s_serializer.Serialize(stream, this);
        }
    }
}
