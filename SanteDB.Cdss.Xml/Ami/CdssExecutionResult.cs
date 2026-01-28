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
using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Model.Diagnostics;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Attributes;
using SanteDB.Core.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Ami
{
    /// <summary>
    /// CDSS execution result
    /// </summary>
    [AddDependentSerializers]
    [XmlType(nameof(CdssExecutionResult), Namespace = "http://santedb.org/cdss")]
    public class CdssExecutionResult : IResourceCollection
    {

        /// <summary>
        /// Gets or sets the start time
        /// </summary>
        [XmlElement("started"), JsonProperty("started")]
        public DateTimeOffset StartTime { get; set; }

        /// <summary>
        /// Gets or sets the time that the run was finished
        /// </summary>
        [XmlElement("finished"), JsonProperty("finished")]
        public DateTimeOffset StopTime { get; set; }

        /// <summary>
        /// Gets or sets the target after analysis is complete
        /// </summary>
        [XmlElement("target"), JsonProperty("target")]
        public IdentifiedData ResultingTarget { get; set; }

        /// <summary>
        /// Gets or sets the proposed actions
        /// </summary>
        [XmlElement("propose"), JsonProperty("propose")]
        public List<Act> Proposals { get; set; }

        /// <inheritdoc/>
        [XmlIgnore, JsonIgnore]
        public IEnumerable<IIdentifiedResource> Item => this.Proposals;

        /// <summary>
        /// Gets the detected issues as a part of the execution
        /// </summary>
        [XmlElement("issue"), JsonProperty("issue")]
        public List<DetectedIssue> Issues { get; set; }

        /// <summary>
        /// Gets the total results
        /// </summary>
        public int? TotalResults => this.Proposals.Count;

        /// <summary>
        /// Gets or sets the debug information is present
        /// </summary>
        [XmlElement("debug"), JsonProperty("debug")]
        public CdssDiagnositcReport Debug { get; set; }

        /// <summary>
        /// Add annotation to all objects
        /// </summary>
        public void AddAnnotationToAll(object annotation)
        {
            this.Proposals.ForEach(o => o.AddAnnotation(annotation));
        }
    }
}
