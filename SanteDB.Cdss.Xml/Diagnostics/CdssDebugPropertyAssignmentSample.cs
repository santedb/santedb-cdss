using SanteDB.Core.Model.Interfaces;
using System.Net;

namespace SanteDB.Cdss.Xml.Diagnostics
{
    /// <summary>
    /// Property assignment sample
    /// </summary>
    public sealed class CdssDebugPropertyAssignmentSample  : CdssDebugSample
    {

        /// <summary>
        /// Assign property 
        /// </summary>
        private CdssDebugPropertyAssignmentSample(string propertyPath, object value)
        {
            this.PropertyPath = propertyPath;
            if(value is ICanDeepCopy icdc)
            {
                this.Value = icdc.DeepCopy();
            }
            else
            {
                this.Value = value;
            }
        }

        /// <summary>
        /// Create a property assignment sample
        /// </summary>
        internal static CdssDebugPropertyAssignmentSample Create(string propertyPath, object value) => new CdssDebugPropertyAssignmentSample(propertyPath, value);

        /// <summary>
        /// Gets the property path
        /// </summary>
        public string PropertyPath { get; }

        /// <summary>
        /// Get the value assigned
        /// </summary>
        public object Value { get; }
    }
}