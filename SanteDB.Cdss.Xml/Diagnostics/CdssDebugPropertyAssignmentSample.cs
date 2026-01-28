/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.Model.Interfaces;

namespace SanteDB.Cdss.Xml.Diagnostics
{
    /// <summary>
    /// Property assignment sample
    /// </summary>
    public sealed class CdssDebugPropertyAssignmentSample : CdssDebugSample
    {

        /// <summary>
        /// Assign property 
        /// </summary>
        private CdssDebugPropertyAssignmentSample(string propertyPath, object value)
        {
            this.PropertyPath = propertyPath;
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