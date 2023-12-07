using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Diagnostics;
using SanteDB.Cdss.Xml.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Diagnostics
{
    /// <summary>
    /// Represents a collection of an exception
    /// </summary>
    [XmlType(nameof(CdssExceptionDiagnosticSample), Namespace = "http://santedb.org/cdss")]
    public class CdssExceptionDiagnosticSample : CdssDiagnosticSample
    {

        /// <summary>
        /// Serialization ctor
        /// </summary>
        public CdssExceptionDiagnosticSample()
        {
        }

        /// <summary>
        /// Create a new exception sample from <paramref name="exceptionSample"/>
        /// </summary>
        internal CdssExceptionDiagnosticSample(CdssDebugExceptionSample exceptionSample) : base(exceptionSample)
        {
            if (exceptionSample.Exception is CdssEvaluationException cde)
            {
                this.Summary = cde.ToCdssStackTrace();
            }
            else
            {
                this.Summary = exceptionSample.Exception.ToHumanReadableString();
            }
            this.Detail = exceptionSample.Exception.ToString();
        }

        /// <summary>
        /// Gets or sets the summary information
        /// </summary>
        [XmlElement("summary"), JsonProperty("summary")]
        public String Summary { get; set; }

        /// <summary>
        /// Gets or sets the exception detail
        /// </summary>
        [XmlElement("detail"), JsonProperty("detail")]
        public String Detail { get; set; }
    }
}
