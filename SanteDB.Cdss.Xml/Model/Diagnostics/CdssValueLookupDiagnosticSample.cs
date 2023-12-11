using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Diagnostics;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Diagnostics
{
    /// <summary>
    /// Represents a lookup value
    /// </summary>
    [XmlType(nameof(CdssValueLookupDiagnosticSample), Namespace = "http://santedb.org/cdss")]
    public class CdssValueLookupDiagnosticSample : CdssDiagnosticSample
    {

        /// <summary>
        /// Default ctor
        /// </summary>
        public CdssValueLookupDiagnosticSample()
        {
        }

        internal CdssValueLookupDiagnosticSample(CdssDebugValueSample valueSample)
        {
            this.Value = new CdssDiagnosticSampleValueWrapper(valueSample.Value);
            this.Name = valueSample.Name;
        }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        [XmlAttribute("name"), JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        [XmlElement("value"), JsonProperty("value")]
        public CdssDiagnosticSampleValueWrapper Value { get; set; }
    }
}