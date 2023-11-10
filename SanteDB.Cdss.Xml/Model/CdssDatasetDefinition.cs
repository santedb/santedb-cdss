using Newtonsoft.Json;
using SanteDB.Core.Data.Import;
using SanteDB.Core.Data.Import.Format;
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
        [XmlText, JsonProperty("csv")]
        public string RawData { get; set; }

    }
}