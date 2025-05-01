/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: fyfej
 * Date: 2024-6-21
 */
using SanteDB.Cdss.Xml.Model.Assets;
using SanteDB.Core.Model.Interfaces;

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
        private CdssDebugFactSample(string factName, CdssComputableAssetDefinition assetDefinition, object value, long computationTime)
        {
            this.FactName = factName;
            this.FactDefinition = assetDefinition;
            this.ComputationTime = computationTime;
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
        internal static CdssDebugFactSample Create(string factName, CdssComputableAssetDefinition factAsset, object value, long computationTime) => new CdssDebugFactSample(factName, factAsset, value, computationTime);

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

        /// <summary>
        /// Gets the computation time
        /// </summary>
        public long ComputationTime { get; }
    }
}