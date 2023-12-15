using SanteDB.Cdss.Xml.Model;
using SanteDB.Cdss.Xml.Model.Assets;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.i18n;
using SanteDB.Core.Model.Acts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SanteDB.Cdss.Xml.Diagnostics
{
    /// <summary>
    /// Represents a CDSS debug stack frame
    /// </summary>
    public class CdssDebugStackFrame : CdssDebugSample
    {
        // The execution frame
        private readonly CdssExecutionStackFrame m_executionFrame;
        // True if exit has been called
        private bool m_exited = false;
        // Samples collected
        private readonly LinkedList<CdssDebugSample> m_activitySamples = new LinkedList<CdssDebugSample>();

        /// <summary>
        /// Serialization ctor
        /// </summary>
        public CdssDebugStackFrame()
        {
            
        }

        /// <summary>
        /// Creates a new execution stack frame
        /// </summary>
        private CdssDebugStackFrame(CdssExecutionStackFrame executionFrame, CdssDebugStackFrame parent)
        {
            this.ParentFrame = parent;
            parent?.m_activitySamples.AddLast(this); // link the parent to us
            this.m_executionFrame = executionFrame;
            this.Source = executionFrame.Owner;
        }

        /// <summary>
        /// Create a new debug execution stack frame
        /// </summary>
        /// <param name="executionStackFrame">The execution stack frame which this debug frame is based on</param>
        /// <param name="parent">The parent of this frame</param>
        /// <returns>The created stack frame</returns>
        internal static CdssDebugStackFrame Create(CdssExecutionStackFrame executionStackFrame, CdssDebugStackFrame parent) => new CdssDebugStackFrame(executionStackFrame, parent);

        /// <summary>
        /// Gets the frame entry
        /// </summary>
        public CdssDebugStackFrame ParentFrame { get; }


        /// <summary>
        /// Gets the time that the frame was exited
        /// </summary>
        public DateTimeOffset ExitTime { get; private set; }

        /// <summary>
        /// Gets the source of the frame
        /// </summary>
        public CdssBaseObjectDefinition Source { get; }

        /// <summary>
        /// Get samples
        /// </summary>
        public IEnumerable<CdssDebugSample> GetSamples() => this.m_activitySamples.ToArray();

        /// <summary>
        /// Exit the frame
        /// </summary>
        public void Exit()
        {
            this.m_exited = true;
            this.ExitTime = DateTimeOffset.Now;
        }

        /// <summary>
        /// Push a sample into the debug context
        /// </summary>
        /// <param name="sampleName">The name of the sample</param>
        /// <param name="value">The value of the sample</param>
        public void AddSample(String sampleName, object value)
        {
            if(this.m_exited)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.WOULD_RESULT_INVALID_STATE, nameof(AddSample)));
            }
            this.m_activitySamples.AddLast(CdssDebugValueSample.Create(sampleName, value, true));
        }


        /// <summary>
        /// Add a read sample
        /// </summary>
        public void AddRead(String sampleName, object value)
        {
            if (this.m_exited)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.WOULD_RESULT_INVALID_STATE, nameof(AddSample)));
            }
            // Is a read already the last object? if so we don't need to re-add it
            if (!(this.m_activitySamples.Last?.Value is CdssDebugValueSample cdvs && cdvs.Name == sampleName && value == cdvs.Value ||
                this.m_activitySamples.Last?.Value is CdssDebugFactSample cdfs && cdfs.FactName == sampleName && value == cdfs.Value))
            {
                this.m_activitySamples.AddLast(CdssDebugValueSample.Create(sampleName, value, false));
            }
        }

        /// <summary>
        /// Add a fact computation asset to the object sample
        /// </summary>
        /// <param name="factName">The name of the fact that is being added</param>
        /// <param name="factAsset">The fact asset definition</param>
        /// <param name="value">The value of the fact</param>
        /// <param name="computationMs">The number of milliseconds it took to compute the fact</param>
        /// <returns>The created fact debug sample</returns>
        public CdssDebugFactSample AddFact(String factName, CdssComputableAssetDefinition factAsset, object value, long computationMs)
        {
            if (this.m_exited)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.WOULD_RESULT_INVALID_STATE, nameof(AddFact)));
            }
            var retVal = CdssDebugFactSample.Create(factName, factAsset, value, computationMs);
            this.m_activitySamples.AddLast(retVal);
            return retVal;
        }

        /// <summary>
        /// Add an exception to the current stack frame
        /// </summary>
        /// <param name="exception">The exception that is causing the sample to be added</param>
        /// <returns>The debug exception sample</returns>
        public CdssDebugExceptionSample AddException(Exception exception)
        {
            if (this.m_exited)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.WOULD_RESULT_INVALID_STATE, nameof(AddException)));
            }
            var retVal = CdssDebugExceptionSample.Create(exception);
            this.m_activitySamples.AddLast(retVal);
            return retVal;  
        }

        /// <summary>
        /// Add a sample indicating a proposal was pushed
        /// </summary>
        internal void AddProposal(Act proposedAct)
        {
            if(this.m_exited)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.WOULD_RESULT_INVALID_STATE, nameof(AddProposal)));
            }
            var retVal = CdssDebugProposalSample.Create(proposedAct);
            this.m_activitySamples.AddLast(retVal);
        }

        /// <summary>
        /// Add issue to the debug output
        /// </summary>
        internal void AddIssue(DetectedIssue issue)
        {
            if (this.m_exited)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.WOULD_RESULT_INVALID_STATE, nameof(AddIssue)));
            }
            var retVal = CdssDebugIssueSample.Create(issue);
            this.m_activitySamples.AddLast(retVal);
        }

        /// <summary>
        /// Add an assignment of a property
        /// </summary>
        internal void AddAssignment(string propertyName, object value)
        {
            if (this.m_exited)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.WOULD_RESULT_INVALID_STATE, nameof(AddIssue)));
            }
            var retVal = CdssDebugPropertyAssignmentSample.Create(propertyName, value);
            this.m_activitySamples.AddLast(retVal);
        }
    }
}