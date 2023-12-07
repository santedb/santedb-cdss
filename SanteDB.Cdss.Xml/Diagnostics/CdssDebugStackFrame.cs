using SanteDB.Cdss.Xml.Model;
using SanteDB.Cdss.Xml.Model.Assets;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.i18n;
using SanteDB.Core.Model.Acts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SanteDB.Cdss.Xml.Diagnostics
{
    /// <summary>
    /// Represents a CDSS debug stack frame
    /// </summary>
    public class CdssDebugStackFrame 
    {
        // The execution frame
        private readonly CdssExecutionStackFrame m_executionFrame;
        // True if exit has been called
        private bool m_exited = false;
        // Samples collected
        private readonly LinkedList<CdssDebugSample> m_samples = new LinkedList<CdssDebugSample>();

        /// <summary>
        /// Creates a new execution stack frame
        /// </summary>
        private CdssDebugStackFrame(CdssExecutionStackFrame executionFrame, CdssDebugStackFrame parent)
        {
            this.ParentFrame = parent;
            this.m_executionFrame = executionFrame;
            this.EntryTime = DateTimeOffset.Now;
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
        /// Gets the entry time
        /// </summary>
        public DateTimeOffset EntryTime { get; }

        /// <summary>
        /// Gets the time that the frame was exited
        /// </summary>
        public DateTimeOffset ExitTime { get; private set; }

        /// <summary>
        /// Gets the source of the frame
        /// </summary>
        public CdssBaseObjectDefinition Source { get; }

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
            this.m_samples.AddLast(CdssDebugSample.Create(sampleName, value));
        }

        /// <summary>
        /// Add a fact computation asset to the object sample
        /// </summary>
        /// <param name="factName">The name of the fact that is being added</param>
        /// <param name="factAsset">The fact asset definition</param>
        /// <returns>The created fact debug sample</returns>
        public CdssDebugFactSample AddFact(String factName, CdssComputableAssetDefinition factAsset)
        {
            if (this.m_exited)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.WOULD_RESULT_INVALID_STATE, nameof(AddFact)));
            }
            var retVal = CdssDebugFactSample.Create(factName, factAsset);
            this.m_samples.AddLast(retVal);
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
            this.m_samples.AddLast(retVal);
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
            var retVal = CdssDebugSample.Create("proposal", proposedAct);
            this.m_samples.AddLast(retVal);
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
            var retVal = CdssDebugSample.Create("issue", issue);
            this.m_samples.AddLast(retVal);
        }
    }
}