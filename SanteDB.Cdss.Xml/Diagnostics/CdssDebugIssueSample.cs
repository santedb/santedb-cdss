using SanteDB.Core.BusinessRules;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Cdss.Xml.Diagnostics
{
    /// <summary>
    /// Represents a raised issue
    /// </summary>
    public sealed class CdssDebugIssueSample : CdssDebugSample
    {

        private CdssDebugIssueSample(DetectedIssue raisedIssue)
        {
            this.Issue = raisedIssue;
        }

        /// <summary>
        /// Gets the issue that was raised
        /// </summary>
        public DetectedIssue Issue { get; }

        /// <summary>
        /// Create a new raised issue sample
        /// </summary>
        /// <param name="raisedIssue">The issue raised</param>
        internal static CdssDebugIssueSample Create(DetectedIssue raisedIssue) => new CdssDebugIssueSample(raisedIssue);
    }
}
