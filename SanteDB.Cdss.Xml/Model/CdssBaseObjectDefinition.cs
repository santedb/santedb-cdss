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
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// CDSS base object
    /// </summary>
    [XmlType(nameof(CdssBaseObjectDefinition), Namespace = "http://santedb.org/cdss")]
    public abstract class CdssBaseObjectDefinition
    {

        /// <summary>
        /// Register a loaded object
        /// </summary>
        protected CdssBaseObjectDefinition()
        {
            CdssLibraryLoadContext.Current?.RegisterLoaded(this);
        }

        /// <summary>
        /// A unique name which is used to reference the object in the base
        /// </summary>
        [XmlAttribute("id"), JsonProperty("id")]
        public String Id { get; set; }

        /// <summary>
        /// A descriptive name
        /// </summary>
        [XmlAttribute("name"), JsonProperty("name")]
        public String Name { get; set; }

        /// <summary>
        /// The UUID for the CDSS base object
        /// </summary>
        [XmlAttribute("uuid"), JsonProperty("uuid")]
        public Guid Uuid { get; set; }

        /// <summary>
        /// True if UUID is specified
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public bool UuidSpecified { get; set; }

        /// <summary>
        /// Gets or sets the status for the CDSS object
        /// </summary>
        [XmlElement("status"), JsonProperty("status")]
        public CdssObjectState Status { get; set; }

        /// <summary>
        /// True if status has been specified
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public bool StatusSpecified { get; set; }

        /// <summary>
        /// Gets or sets the metadata related to the cdss object
        /// </summary>
        [XmlElement("meta"), JsonProperty("meta")]
        public CdssObjectMetadata Metadata { get; set; }

        /// <summary>
        /// Gets or sets the transpiled metadata
        /// </summary>
        [XmlElement("source"), JsonProperty("source")]
        public CdssTranspileMapMetaData TranspileSourceReference { get; set; }

        /// <summary>
        /// Gets or sets the object identifier
        /// </summary>
        [XmlAttribute("oid"), JsonProperty("oid")]
        public String Oid { get; set; }

        /// <inheritdoc/>
        public override string ToString() => $"{this.GetType().Name} {this.Name} {(!String.IsNullOrEmpty(this.Id) ? $"(#{this.Id})" : "")}";

        /// <summary>
        /// Validate the object definition
        /// </summary>
        public abstract IEnumerable<DetectedIssue> Validate(CdssExecutionContext context);

        /// <summary>
        /// Represent this as a source code reference string
        /// </summary>
        /// <returns></returns>
        public string ToReferenceString() => $"{this.GetType().Name} {this.TranspileSourceReference?.SourceFileName ?? this.Name} @{this.TranspileSourceReference?.StartPosition}";

        /// <summary>
        /// Creat a shallow clone
        /// </summary>
        public CdssBaseObjectDefinition Clone() => (CdssBaseObjectDefinition)this.MemberwiseClone();
    }
}
