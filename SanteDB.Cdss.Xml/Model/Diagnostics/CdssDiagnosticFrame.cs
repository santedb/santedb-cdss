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
using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Diagnostics
{
    /// <summary>
    /// Represents a CDSS evaluation frame
    /// </summary>
    [XmlType(nameof(CdssDiagnosticFrame), Namespace = "http://santedb.org/cdss")]
    public class CdssDiagnosticFrame : CdssDiagnosticSample
    {

        /// <summary>
        /// Serialization ctor
        /// </summary>
        public CdssDiagnosticFrame()
        {
        }

        internal CdssDiagnosticFrame(CdssDebugStackFrame stackFrame) : base(stackFrame)
        {
            this.Exit = stackFrame.ExitTime.DateTime;
            if (stackFrame.Source != null)
            {
                this.Source = new CdssDiagnosticObjectReference(stackFrame.Source);
            }
            this.Samples = stackFrame.GetSamples().Select(o => CdssDiagnosticSample.Create(o)).OfType<CdssDiagnosticSample>().ToList();
        }

        /// <summary>
        /// Gets or sets teh time that the frame was exited
        /// </summary>
        [XmlAttribute("exitTime"), JsonProperty("exitTime")]
        public DateTime Exit { get; set; }

        /// <summary>
        /// Gets or sets the source
        /// </summary>
        [XmlElement("defn"), JsonProperty("defn")]
        public CdssDiagnosticObjectReference Source { get; set; }

        /// <summary>
        /// Gets or sets the samples collected within the frame
        /// </summary>
        [XmlArray("activities"),
            XmlArrayItem("assign", typeof(CdssPropertyAssignDiagnosticSample)),
            XmlArrayItem("let", typeof(CdssValueDiagnosticSample)),
            XmlArrayItem("get", typeof(CdssValueLookupDiagnosticSample)),
            XmlArrayItem("fact", typeof(CdssFactDiagnosticSample)),
            XmlArrayItem("throw", typeof(CdssExceptionDiagnosticSample)),
            XmlArrayItem("propose", typeof(CdssProposalDiagnosticSample)),
            XmlArrayItem("raise", typeof(CdssIssueDiagnosticSample)),
            XmlArrayItem("compute", typeof(CdssDiagnosticFrame)),
        JsonProperty("activities")]
        public List<CdssDiagnosticSample> Samples { get; set; }

    }
}