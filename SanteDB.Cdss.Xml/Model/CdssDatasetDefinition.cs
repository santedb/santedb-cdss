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
    public class CdssDatasetDefinition : CdssBaseObjectDefinition, IEnumerable<IForeignDataRecord>
    {

        /// <summary>
        /// Reference data
        /// </summary>
        [XmlText, JsonProperty("data")]
        public string RawData { get; set; }

        /// <summary>
        /// Get enumerator for the data
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <summary>
        /// Get enumerator
        /// </summary>
        public IEnumerator<IForeignDataRecord> GetEnumerator()
        {
            using (var str = new MemoryStream(Encoding.UTF8.GetBytes(this.RawData)))
            {
                using (var dr = new CsvForeignDataFormat().Open(str).CreateReader())
                {
                    dr.MoveNext();
                    yield return dr;
                }
            }
        }
    }
}