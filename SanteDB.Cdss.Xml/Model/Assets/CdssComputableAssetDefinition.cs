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
 */
using Newtonsoft.Json;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.i18n;
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
        /// Gets or sets the priority for overridding default
        /// </summary>
        [XmlAttribute("priority"), JsonProperty("priority")]
        public int Priority { get; set; }

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
            if (CdssExecutionStackFrame.Current == null)
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
            if (string.IsNullOrEmpty(this.Id) && string.IsNullOrEmpty(this.Name))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.asset.identification", "CDSS logic asset definitions must carry either a @name or @id attribute", Guid.Empty, this.ToReferenceString());
            }
        }
    }
}