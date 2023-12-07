using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Diagnostics;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Diagnostics
{
    /// <summary>
    /// Represents a diagnostic sample that is a simple variable
    /// </summary>
    [XmlType(nameof(CdssValueDiagnosticSample), Namespace = "http://santedb.org/cdss")]
    public class CdssValueDiagnosticSample : CdssDiagnosticSample
    {

        /// <summary>
        /// Serialization ctor
        /// </summary>
        public CdssValueDiagnosticSample()
        {
        }

        /// <summary>
        /// Create a new 
        /// </summary>
        /// <param name="valueSample"></param>
        internal CdssValueDiagnosticSample(CdssDebugValueSample valueSample) : base(valueSample)
        {
            this.Name = valueSample.Name;
            this.Value = new CdssDiagnosticSampleValueWrapper(valueSample.Value);
        }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        [XmlElement("name"), JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        [XmlElement("value"), JsonProperty("value")]
        public CdssDiagnosticSampleValueWrapper Value { get; set; }
    }
}
