using SanteDB.Core.Model.Interfaces;
using System;

namespace SanteDB.Cdss.Xml.Diagnostics
{
    /// <summary>
    /// Represents CDSS sample collected
    /// </summary>
    public class CdssDebugSample
    {

        /// <summary>
        /// Private constructor
        /// </summary>
        protected CdssDebugSample(string name, object value) : this(name)
        {
            if (value is ICanDeepCopy icdc)
            {
                this.Value = icdc.DeepCopy();
            }
            else
            {
                this.Value = value;
            }
        }

        /// <summary>
        /// Create a new CDSS sample registration
        /// </summary>
        protected CdssDebugSample(String name)
        {
            this.Name = name;
            this.CollectionTime = DateTimeOffset.Now;
        }

        /// <summary>
        /// Gets the name of the sample
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the value of the sample
        /// </summary>
        public object Value { get; protected set; }

        /// <summary>
        /// Gets the time the sample was collected
        /// </summary>
        public DateTimeOffset CollectionTime { get; }

        /// <summary>
        /// Create a new instance of the CDSS debug sample
        /// </summary>
        /// <param name="name">The name of the sample collected</param>
        /// <param name="value">The value of the sample at collection</param>
        /// <returns>The collected sample</returns>
        internal static CdssDebugSample Create(string name, object value) => new CdssDebugSample(name, value);
    }
}