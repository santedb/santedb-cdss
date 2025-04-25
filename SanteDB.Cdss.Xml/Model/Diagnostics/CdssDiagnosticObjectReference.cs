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
using Newtonsoft.Json;
using SanteDB.Core.BusinessRules;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Diagnostics
{
    /// <summary>
    /// Represents a reference to an object
    /// </summary>
    [XmlType(nameof(CdssDiagnosticObjectReference), Namespace = "http://santedb.org/cdss")]
    public class CdssDiagnosticObjectReference : CdssBaseObjectDefinition
    {

        /// <summary>
        /// Serialization ctor
        /// </summary>
        public CdssDiagnosticObjectReference()
        {

        }

        /// <summary>
        /// Creates a new report for an object reference
        /// </summary>
        /// <param name="referencedObject">The object being referenced</param>
        internal CdssDiagnosticObjectReference(CdssBaseObjectDefinition referencedObject)
        {
            this.Id = referencedObject.Id;
            this.Name = referencedObject.Name;
            this.Oid = referencedObject.Oid;
            this.TranspileSourceReference = referencedObject.TranspileSourceReference;
            this.Uuid = referencedObject.Uuid;
            this.UuidSpecified = referencedObject.UuidSpecified;
            this.ReferenceType = referencedObject.GetType().Name;
        }

        /// <summary>
        /// Gets or sets the type that is being referenced
        /// </summary>
        [XmlAttribute("type"), JsonProperty("type")]
        public string ReferenceType { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            yield break;
        }
    }
}
