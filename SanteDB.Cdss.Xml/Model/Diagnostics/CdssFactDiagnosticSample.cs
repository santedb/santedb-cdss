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
using SanteDB.Cdss.Xml.Diagnostics;
using System;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Diagnostics
{
    /// <summary>
    /// Represents a fact calculation sample
    /// </summary>
    [XmlType(nameof(CdssFactDiagnosticSample), Namespace = "http://santedb.org/cdss")]
    public class CdssFactDiagnosticSample : CdssDiagnosticSample
    {

        /// <summary>
        /// Serialization ctor
        /// </summary>
        public CdssFactDiagnosticSample()
        {
        }

        /// <summary>
        /// Create a new fact diagnostic sample from <paramref name="factSample"/>
        /// </summary>
        internal CdssFactDiagnosticSample(CdssDebugFactSample factSample) : base(factSample)
        {
            this.FactName = factSample.FactName;
            this.Value = new CdssDiagnosticSampleValueWrapper(factSample.Value);
            this.FactDefinition = new CdssDiagnosticObjectReference(factSample.FactDefinition);
            this.ComputationTime = factSample.ComputationTime;
        }

        /// <summary>
        /// Exit time for the fact computation
        /// </summary>
        [XmlAttribute("computationMs"), JsonProperty("computationMs")]
        public long ComputationTime { get; set; }

        /// <summary>
        /// Gets or sets the fact name
        /// </summary>
        [XmlElement("name"), JsonProperty("name")]
        public String FactName { get; set; }

        /// <summary>
        /// Gets or sets the fact value
        /// </summary>
        [XmlElement("value"), JsonProperty("value")]
        public CdssDiagnosticSampleValueWrapper Value { get; set; }

        /// <summary>
        /// Gets or sets the definition of the fact that resulted in the sample being collected
        /// </summary>
        [XmlElement("factRef"), JsonProperty("factRef")]
        public CdssDiagnosticObjectReference FactDefinition { get; set; }


    }
}
