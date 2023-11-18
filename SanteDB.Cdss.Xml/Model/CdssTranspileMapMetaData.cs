using Newtonsoft.Json;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// Contains metadata about transpiled source
    /// </summary>
    [XmlType(nameof(CdssTranspileMapMetaData), Namespace = "http://santedb.org/cdss")]
    public class CdssTranspileMapMetaData
    {
        public CdssTranspileMapMetaData()
        {
            
        }
        /// <summary>
        /// Creates a new transpilation metadata for original source
        /// </summary>
        public CdssTranspileMapMetaData(int startLine, int startColumn, int stopLine, int stopColumn)
        {
            this.StartPosition = $"{startLine},{startColumn}";
            this.EndPoisition = $"{stopLine},{stopColumn}";
        }

        /// <summary>
        /// Source file
        /// </summary>
        [XmlAttribute("source"), JsonProperty("source")]
        public string SourceFileName { get; set; }

        /// <summary>
        /// Gets or sets the line
        /// </summary>
        [XmlAttribute("start"), JsonProperty("start")]
        public string StartPosition { get; set; }

        /// <summary>
        /// Gets or sets the column
        /// </summary>
        [XmlAttribute("stop"), JsonProperty("stop")]
        public string EndPoisition { get; set; }

    }
}