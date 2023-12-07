using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Diagnostics;
using SanteDB.Core.Model;
using SanteDB.Core.Model.DataTypes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Diagnostics
{
    /// <summary>
    /// Represents a simple proposal diagnostic sample
    /// </summary>
    [XmlType(nameof(CdssProposalDiagnosticSample), Namespace = "http://santedb.org/cdss")]
    public class CdssProposalDiagnosticSample : CdssDiagnosticSample
    {

        /// <summary>
        /// Default ctor for serialization
        /// </summary>
        public CdssProposalDiagnosticSample()
        {
            
        }

        /// <summary>
        /// Create a new propsal sample from <paramref name="proposalSample"/>
        /// </summary>
        internal CdssProposalDiagnosticSample(CdssDebugProposalSample proposalSample) : base (proposalSample)
        {
            proposalSample.Proposal.Key = proposalSample.Proposal.Key ?? Guid.NewGuid();
            this.Proposal = new IdentifiedDataReference(proposalSample.Proposal);
        }

        /// <summary>
        /// Gets or sets the referenced proposal
        /// </summary>
        [XmlElement("value"), JsonProperty("value")]
        public IdentifiedDataReference Proposal { get; set; }
    }
}
