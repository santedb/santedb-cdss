using System;

namespace SanteDB.Cdss.Xml.Diagnostics
{
    /// <summary>
    /// Represents a sample where the debugger has caught an execution exception
    /// </summary>
    public sealed class CdssDebugExceptionSample : CdssDebugSample
    {
        /// <summary>
        /// CTOR
        /// </summary>
        private CdssDebugExceptionSample(Exception exception) 
        {
            this.Exception = exception;
        }

        /// <summary>
        /// Gets the exception that was captured
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Create a new debug exception sample
        /// </summary>
        internal static CdssDebugExceptionSample Create(Exception e) => new CdssDebugExceptionSample(e);

    }
}