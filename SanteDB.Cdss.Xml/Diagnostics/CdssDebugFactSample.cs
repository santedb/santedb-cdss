using SanteDB.Cdss.Xml.Model.Assets;
using SanteDB.Core.i18n;
using SanteDB.Core.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SanteDB.Cdss.Xml.Diagnostics
{
    /// <summary>
    /// Represents a debug sample where the value at the time of collection is unknown
    /// </summary>
    public sealed class CdssDebugFactSample : CdssDebugSample
    {
        /// <summary>
        /// Private CTOR
        /// </summary>
        private CdssDebugFactSample(string factName, CdssComputableAssetDefinition assetDefinition, object value) 
        {
            this.FactName = factName;
            this.FactDefinition = assetDefinition;
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
        /// Create a new CDSS fact sample
        /// </summary>
        internal static CdssDebugFactSample Create(string factName, CdssComputableAssetDefinition factAsset, object value) => new CdssDebugFactSample(factName, factAsset, value);

        /// <summary>
        /// Gets the name of the sample
        /// </summary>
        public string FactName { get; }

        /// <summary>
        /// Gets the fact definition
        /// </summary>
        public CdssComputableAssetDefinition FactDefinition { get; }
        
        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public object Value { get; }

    }
}