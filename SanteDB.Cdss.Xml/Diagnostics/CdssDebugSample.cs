using SanteDB.Core.Model.Interfaces;
using System;

namespace SanteDB.Cdss.Xml.Diagnostics
{
    /// <summary>
    /// Represents CDSS sample collected
    /// </summary>
    public abstract class CdssDebugSample
    {

        /// <summary>
        /// Create a new CDSS sample registration
        /// </summary>
        protected CdssDebugSample()
        {
            this.CollectionTime = DateTimeOffset.Now;
        }

        /// <summary>
        /// Gets the time the sample was collected
        /// </summary>
        public DateTimeOffset CollectionTime { get; }

    }
}