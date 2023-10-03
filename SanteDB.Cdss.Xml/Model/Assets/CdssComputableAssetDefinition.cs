using Newtonsoft.Json;
using SanteDB.Core.Model;
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
        /// Compute the asset output based on the current context
        /// </summary>
        /// <param name="cdssContext">The context which the value should be computed based on</param>
        /// <returns>The computed value</returns>
        internal abstract object Compute(CdssContext cdssContext);

    }
}