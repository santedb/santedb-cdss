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
using SanteDB.Cdss.Xml.Diagnostics;
using SanteDB.Core.BusinessRules;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Diagnostics
{
    /// <summary>
    /// Represents a diagnostic sample for issues
    /// </summary>
    [XmlType(nameof(CdssIssueDiagnosticSample), Namespace = "http://santedb.org/cdss")]
    public class CdssIssueDiagnosticSample : CdssDiagnosticSample
    {

        /// <summary>
        /// Serialization ctor
        /// </summary>
        public CdssIssueDiagnosticSample()
        {
        }

        /// <summary>
        /// Creates a new diagnostic sample from <paramref name="issueSample"/>
        /// </summary>
        internal CdssIssueDiagnosticSample(CdssDebugIssueSample issueSample) : base(issueSample)
        {
            this.Issue = issueSample.Issue;
        }

        /// <summary>
        /// Gets or sets the detected issue
        /// </summary>
        [XmlElement("issue"), JsonProperty("issue")]
        public DetectedIssue Issue { get; set; }
    }
}
