/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-12-8
 */
using SanteDB.Core.Model.Interfaces;

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
