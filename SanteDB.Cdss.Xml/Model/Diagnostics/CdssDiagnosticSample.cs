using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Diagnostics;
using SanteDB.Core.i18n;
using System;
using System.Linq;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Diagnostics
{
    /// <summary>
    /// Represents a diagnostic sample
    /// </summary>
    [XmlType(nameof(CdssDiagnosticSample), Namespace ="http://santedb.org/cdss")]
    public abstract class CdssDiagnosticSample
    {

        /// <summary>
        /// Serialization ctor
        /// </summary>
        public CdssDiagnosticSample()
        {
            
        }

        /// <summary>
        /// Creates a new sample object with <paramref name="sample"/> providing the data
        /// </summary>
        protected CdssDiagnosticSample(CdssDebugSample sample)
        {
            this.CollectionTime = sample.CollectionTime.DateTime;
        }

        /// <summary>
        /// Gets or sets the time that the colle
        /// </summary>
        [XmlAttribute("collectionTime"), JsonProperty("collectionTime")]
        public DateTime CollectionTime { get; set; }

        /// <summary>
        /// Determine if collection time is specified
        /// </summary>
        public bool ShouldSerializeCollectionTime() => this.CollectionTime != default(DateTime);

        /// <summary>
        /// Create an appropriate diagnostic sample object for the <paramref name="debugSample"/>
        /// </summary>
        internal static CdssDiagnosticSample Create(CdssDebugSample debugSample)
        {
            switch (debugSample)
            {
                case CdssDebugExceptionSample exc:
                    return new CdssExceptionDiagnosticSample(exc);
                case CdssDebugFactSample fact:
                    return new CdssFactDiagnosticSample(fact);
                case CdssDebugIssueSample iss:
                    return new CdssIssueDiagnosticSample(iss);
                case CdssDebugProposalSample prop:
                    return new CdssProposalDiagnosticSample(prop);
                case CdssDebugValueSample val:
                    if (val.IsWrite)
                    {
                        return new CdssValueDiagnosticSample(val);
                    }
                    else
                    {
                        return new CdssValueLookupDiagnosticSample(val);
                    }
                case CdssDebugStackFrame frame:
                    if(!frame.GetSamples().Any())
                    {
                        return null;
                    }
                    return new CdssDiagnosticFrame(frame);
                default:
                    throw new InvalidOperationException(ErrorMessages.MAP_INVALID_TYPE);
            }
        }
    }
}