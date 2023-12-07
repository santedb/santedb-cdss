using System;

namespace SanteDB.Cdss.Xml.Diagnostics
{
    /// <summary>
    /// Represents a sample where the debugger has caught an execution exception
    /// </summary>
    public class CdssDebugExceptionSample : CdssDebugSample
    {
        /// <summary>
        /// CTOR
        /// </summary>
        private CdssDebugExceptionSample(Exception exception) : base("__EXCEPTION", exception)
        {
        }

        /// <summary>
        /// Create a new debug exception sample
        /// </summary>
        internal static CdssDebugExceptionSample Create(Exception e) => new CdssDebugExceptionSample(e);

    }
}