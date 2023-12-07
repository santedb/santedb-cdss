using SanteDB.Cdss.Xml.Model.Assets;
using SanteDB.Core.i18n;
using SanteDB.Core.Model.Interfaces;
using System;
using System.Collections.Generic;

namespace SanteDB.Cdss.Xml.Diagnostics
{
    /// <summary>
    /// Represents a debug sample where the value at the time of collection is unknown
    /// </summary>
    public class CdssDebugFactSample : CdssDebugSample
    {
        /// <summary>
        /// Private CTOR
        /// </summary>
        private CdssDebugFactSample(string factName, CdssComputableAssetDefinition assetDefinition) : base(factName)
        {
            this.FactDefinition = assetDefinition;
        }

        /// <summary>
        /// Create a new CDSS fact sample
        /// </summary>
        internal static CdssDebugFactSample Create(string factName, CdssComputableAssetDefinition factAsset) => new CdssDebugFactSample(factName, factAsset);

        /// <summary>
        /// Gets the fact definition
        /// </summary>
        public CdssComputableAssetDefinition FactDefinition { get; }
        
        /// <summary>
        /// Gets the result timestamp
        /// </summary>
        public DateTimeOffset ResultTimestamp { get; private set; }

        /// <summary>
        /// Set the fact result
        /// </summary>
        internal void SetFactResult(object value)
        {
            if(base.Value != null)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.WOULD_RESULT_INVALID_STATE, nameof(SetFactResult)));
            }

            this.ResultTimestamp = DateTimeOffset.Now;
            if (value is ICanDeepCopy icdc)
            {
                base.Value = icdc.DeepCopy();
            }
            else
            {
                base.Value = value;
            }
        }
    }
}