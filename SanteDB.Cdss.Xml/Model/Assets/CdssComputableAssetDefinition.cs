using Newtonsoft.Json;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.i18n;
using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
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
        /// <returns>The computed value</returns>
        public abstract object Compute();

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if(string.IsNullOrEmpty(this.Id) && string.IsNullOrEmpty(this.Name))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.asset.identification", "CDSS logic asset definitions must carry either a @name or @id attribute", Guid.Empty, this.ToString());
            }
        }
    }
}