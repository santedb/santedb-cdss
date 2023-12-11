using SanteDB.Cdss.Xml.Model;
using SanteDB.Cdss.Xml.Model.Diagnostics;
using SanteDB.Core.Cdss;
using SanteDB.Core.i18n;
using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Diagnostics
{

    /// <summary>
    /// Represents a diagnostic session
    /// </summary>
    public sealed class CdssDebugSessionData
    {


        // The current stack frame
        private CdssDebugStackFrame m_currentFrame;
        // The first entry frame
        private CdssDebugStackFrame m_entryFrame;

        /// <summary>
        /// Creates a CDSS debug session for the current context
        /// </summary>
        private CdssDebugSessionData(ICdssExecutionContext context, IEnumerable<CdssLibraryDefinition> libraries)
        {
            this.Start = DateTimeOffset.Now;
            this.Target = context.Target;
            this.Libraries = libraries;
        }

        /// <summary>
        /// Create a new CDSS debug session
        /// </summary>
        /// <param name="context">The context for which the execution context should be created</param>
        /// <param name="libraries">The library for which the execution context is created</param>
        /// <returns>The created diagnostic session</returns>
        internal static CdssDebugSessionData Create(CdssExecutionContext context, IEnumerable<CdssLibraryDefinition> libraries) => new CdssDebugSessionData(context, libraries);

        /// <summary>
        /// Indicates that the CDSS session is ended
        /// </summary>
        public void End() => this.Stop = DateTimeOffset.Now;

        /// <summary>
        /// Gets the start time of the session
        /// </summary>
        public DateTimeOffset Start { get; }

        /// <summary>
        /// Get the CDSS library
        /// </summary>
        public IEnumerable<CdssLibraryDefinition> Libraries { get; }

        /// <summary>
        /// Gets the stop time of the session
        /// </summary>
        public DateTimeOffset Stop { get; private set; }

        /// <summary>
        /// Gets the target of the session
        /// </summary>
        public IdentifiedData Target { get; }

        /// <summary>
        /// Gets the current stack frame
        /// </summary>
        public CdssDebugStackFrame CurrentFrame => this.m_currentFrame;

        /// <summary>
        /// Get the root stack frame (where execution initiated)
        /// </summary>
        public CdssDebugStackFrame EntryFrame => this.m_entryFrame;

        /// <summary>
        /// Enter a debug stack frame for tracking debug data
        /// </summary>
        /// <param name="cdssExecutionStackFrame">The execution frame which is triggering this entry into the stack frame</param>
        /// <returns>The created stack frame</returns>
        public CdssDebugStackFrame EnterFrame(CdssExecutionStackFrame cdssExecutionStackFrame)
        {
            this.m_currentFrame = CdssDebugStackFrame.Create(cdssExecutionStackFrame, this.m_currentFrame);
            if(this.m_entryFrame == null)
            {
                this.m_entryFrame = this.m_currentFrame;
            }
            return this.m_currentFrame;
        }

        /// <summary>
        /// Exit the execution frame for the debugger
        /// </summary>
        public void ExitFrame()
        {
            if(this.m_currentFrame == null )
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.WOULD_RESULT_INVALID_STATE, nameof(ExitFrame)));
            }

            this.m_currentFrame.Exit();
            this.m_currentFrame = this.m_currentFrame.ParentFrame;

        }

        /// <summary>
        /// Get a diagnostic report for the session
        /// </summary>
        public CdssDiagnositcReport GetDiagnosticReport() => new CdssDiagnositcReport(this);

       
    }


}
