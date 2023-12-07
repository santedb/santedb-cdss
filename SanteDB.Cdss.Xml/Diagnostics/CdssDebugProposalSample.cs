using SanteDB.Core.Model.Acts;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Cdss.Xml.Diagnostics
{
    /// <summary>
    /// Represents a debug proposal sample
    /// </summary>
    public sealed class CdssDebugProposalSample : CdssDebugSample
    {

        private CdssDebugProposalSample(Act proposedAct)
        {
            this.Proposal = proposedAct;
        }

        /// <summary>
        /// Gets the proposal generated
        /// </summary>
        public Act Proposal { get; }

        /// <summary>
        /// Create a new debug proposal sample
        /// </summary>
        internal static CdssDebugProposalSample Create(Act proposedAct) => new CdssDebugProposalSample(proposedAct);
    }
}
