using Newtonsoft.Json;
using SanteDB.Core.i18n;
using SanteDB.Core.Model;
using System;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Assets
{
    /// <summary>
    /// CDSS computable asset definition
    /// </summary>
    [XmlType(nameof(CdssComputableAssetDefinition), Namespace = "http://santedb.org/cdss")]
    public abstract class CdssComputableAssetDefinition : CdssBaseObjectDefinition
    {

        /// <summary>
        /// Gets the debug view
        /// </summary>
        [XmlIgnore, JsonProperty("debug")]
        public virtual string DebugView { get; protected set; }

        /// <summary>
        /// Throw an appropriate exception if the CDSS engine is in an invalid state
        /// </summary>
        /// <exception cref="ArgumentNullException">When the context is not provided</exception>
        protected void ThrowIfInvalidState()
        {
            if(CdssExecutionStackFrame.Current == null)
            {
                throw new InvalidOperationException(ErrorMessages.WOULD_RESULT_INVALID_STATE);
            }
        }

        /// <summary>
        /// Compute the asset output based on the current context
        /// </summary>
        /// <param name="cdssContext">The context which the value should be computed based on</param>
        /// <returns>The computed value</returns>
        public abstract object Compute();

    }
}