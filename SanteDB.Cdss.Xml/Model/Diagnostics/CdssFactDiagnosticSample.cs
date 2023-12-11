using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Diagnostics
{
    /// <summary>
    /// Represents a fact calculation sample
    /// </summary>
    [XmlType(nameof(CdssFactDiagnosticSample), Namespace = "http://santedb.org/cdss")]
    public class CdssFactDiagnosticSample : CdssDiagnosticSample
    {

        /// <summary>
        /// Serialization ctor
        /// </summary>
        public CdssFactDiagnosticSample()
        {
        }

        /// <summary>
        /// Create a new fact diagnostic sample from <paramref name="factSample"/>
        /// </summary>
        internal CdssFactDiagnosticSample(CdssDebugFactSample factSample) : base(factSample)
        {
            this.FactName = factSample.FactName;
            this.Value = new CdssDiagnosticSampleValueWrapper(factSample.Value);
            this.FactDefinition = new CdssDiagnosticObjectReference(factSample.FactDefinition);
        }

        /// <summary>
        /// Gets or sets the fact name
        /// </summary>
        [XmlElement("name"), JsonProperty("name")]
        public String FactName { get; set; }

        /// <summary>
        /// Gets or sets the fact value
        /// </summary>
        [XmlElement("value"), JsonProperty("value")]
        public CdssDiagnosticSampleValueWrapper Value { get; set; }

        /// <summary>
        /// Gets or sets the definition of the fact that resulted in the sample being collected
        /// </summary>
        [XmlElement("factRef"), JsonProperty("factRef")]
        public CdssDiagnosticObjectReference FactDefinition { get; set; }


    }
}
