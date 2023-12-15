using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Diagnostics
{
    /// <summary>
    /// Represents a CDSS evaluation frame
    /// </summary>
    [XmlType(nameof(CdssDiagnosticFrame), Namespace = "http://santedb.org/cdss")]
    public class CdssDiagnosticFrame : CdssDiagnosticSample
    {

        /// <summary>
        /// Serialization ctor
        /// </summary>
        public CdssDiagnosticFrame()
        {
        }

        internal CdssDiagnosticFrame(CdssDebugStackFrame stackFrame) : base(stackFrame)
        {
            this.Exit = stackFrame.ExitTime.DateTime;
            if (stackFrame.Source != null)
            {
                this.Source = new CdssDiagnosticObjectReference(stackFrame.Source);
            }
            this.Samples = stackFrame.GetSamples().Select(o => CdssDiagnosticSample.Create(o)).OfType<CdssDiagnosticSample>().ToList();
        }

        /// <summary>
        /// Gets or sets teh time that the frame was exited
        /// </summary>
        [XmlAttribute("exitTime"), JsonProperty("exitTime")]
        public DateTime Exit { get; set; }

        /// <summary>
        /// Gets or sets the source
        /// </summary>
        [XmlElement("defn"), JsonProperty("defn")]
        public CdssDiagnosticObjectReference Source { get; set; }

        /// <summary>
        /// Gets or sets the samples collected within the frame
        /// </summary>
        [XmlArray("activities"), 
            XmlArrayItem("assign", typeof(CdssPropertyAssignDiagnosticSample)),
            XmlArrayItem("let", typeof(CdssValueDiagnosticSample)),
            XmlArrayItem("get", typeof(CdssValueLookupDiagnosticSample)),
            XmlArrayItem("fact", typeof(CdssFactDiagnosticSample)),
            XmlArrayItem("throw", typeof(CdssExceptionDiagnosticSample)),
            XmlArrayItem("propose", typeof(CdssProposalDiagnosticSample)),
            XmlArrayItem("raise", typeof(CdssIssueDiagnosticSample)),
            XmlArrayItem("compute", typeof(CdssDiagnosticFrame)),
        JsonProperty("activities")]
        public List<CdssDiagnosticSample> Samples { get; set; }

    }
}