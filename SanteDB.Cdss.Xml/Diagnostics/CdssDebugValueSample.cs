using SanteDB.Core.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace SanteDB.Cdss.Xml.Diagnostics
{
    /// <summary>
    /// Represents a single simple value captured at a point in time
    /// </summary>
    public sealed class CdssDebugValueSample : CdssDebugSample
    {

        /// <summary>
        /// Private constructor
        /// </summary>
        private CdssDebugValueSample(string name, object value, bool isWrite) 
        {
            this.Name = name;
            if (value is ICanDeepCopy icdc)
            {
                this.Value = icdc.DeepCopy();
            }
            else
            {
                this.Value = value;
            }
            this.IsWrite = isWrite;
        }


        /// <summary>
        /// Gets the name of the sample
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the value of the sample
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Gets whether this variable reference is a Write (setting the variable) or a read (getting the variable)
        /// </summary>
        public bool IsWrite { get; }

        /// <summary>
        /// Create a new instance of the CDSS debug sample
        /// </summary>
        /// <param name="name">The name of the sample collected</param>
        /// <param name="value">The value of the sample at collection</param>
        /// <param name="isWrite">The value of the sample is a write (setting of the value)</param>
        /// <returns>The collected sample</returns>
        internal static CdssDebugValueSample Create(string name, object value, bool isWrite) => new CdssDebugValueSample(name, value, isWrite);

    }
}
