using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Diagnostics;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Diagnostics
{
    /// <summary>
    /// Creates a cdss property assignment sample
    /// </summary>
    [XmlType(nameof(CdssPropertyAssignDiagnosticSample), Namespace = "http://santedb.org/cdss")]
    public class CdssPropertyAssignDiagnosticSample : CdssDiagnosticSample
    {
        /// <summary>
        /// Serialization ctor
        /// </summary>
        public CdssPropertyAssignDiagnosticSample()
        {
            
        }

        /// <summary>
        /// Create a new assign sample with data from <paramref name="sample"/>
        /// </summary>
        internal CdssPropertyAssignDiagnosticSample(CdssDebugPropertyAssignmentSample sample) : base(sample)
        {
            this.PropertyPath = sample.PropertyPath;
            this.Value = new CdssDiagnosticSampleValueWrapper(sample.Value);
        }

        /// <summary>
        /// Gets or sets the property path
        /// </summary>
        [XmlElement("path"), JsonProperty("path")]
        public string PropertyPath { get; set; }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        [XmlElement("value"), JsonProperty("value")]
        public CdssDiagnosticSampleValueWrapper Value { get; set; }
    }
}