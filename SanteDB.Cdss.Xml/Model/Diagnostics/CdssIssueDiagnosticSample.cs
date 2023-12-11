using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Diagnostics;
using SanteDB.Core.BusinessRules;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Diagnostics
{
    /// <summary>
    /// Represents a diagnostic sample for issues
    /// </summary>
    [XmlType(nameof(CdssIssueDiagnosticSample), Namespace = "http://santedb.org/cdss")]
    public class CdssIssueDiagnosticSample : CdssDiagnosticSample
    {

        /// <summary>
        /// Serialization ctor
        /// </summary>
        public CdssIssueDiagnosticSample()
        {
        }

        /// <summary>
        /// Creates a new diagnostic sample from <paramref name="issueSample"/>
        /// </summary>
        internal CdssIssueDiagnosticSample(CdssDebugIssueSample issueSample) : base(issueSample)
        {
            this.Issue = issueSample.Issue;
        }

        /// <summary>
        /// Gets or sets the detected issue
        /// </summary>
        [XmlElement("issue"), JsonProperty("issue")]
        public DetectedIssue Issue { get; set; }
    }
}
