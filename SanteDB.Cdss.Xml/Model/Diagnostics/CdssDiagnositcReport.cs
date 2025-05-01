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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Diagnostics
{
    /// <summary>
    /// Represents a serialization of a <see cref="CdssDebugSessionData"/> object
    /// </summary>
    [XmlType(nameof(CdssDiagnositcReport), Namespace = "http://santedb.org/cdss")]
    public class CdssDiagnositcReport
    {
        // Serializer
        private static XmlSerializer s_xsz = new XmlSerializer(typeof(CdssDiagnositcReport));

        public CdssDiagnositcReport()
        {
        }

        /// <summary>
        /// Creates a diagnostic report from the <paramref name="sessionData"/>
        /// </summary>
        /// <param name="sessionData">The session data upon which the report should be based</param>
        internal CdssDiagnositcReport(CdssDebugSessionData sessionData)
        {
            this.Start = sessionData.Start.DateTime;
            this.End = sessionData.Stop.DateTime;
            this.Libraries = sessionData.Libraries.Select(o => o.Id ?? o.Name).ToList();
            this.Target = sessionData.Target.ToString();
            this.EntryFrame = new CdssDiagnosticFrame(sessionData.EntryFrame);
        }

        /// <summary>
        /// Gets or sets the start time of the diagnostic session
        /// </summary>
        [XmlAttribute("start"), JsonProperty("start")]
        public DateTime Start { get; set; }

        /// <summary>
        /// Gets or sets the end time
        /// </summary>
        [XmlAttribute("end"), JsonProperty("end")]
        public DateTime End { get; set; }

        /// <summary>
        /// Gets or sets the target information
        /// </summary>
        [XmlElement("target"), JsonProperty("target")]
        public String Target { get; set; }

        /// <summary>
        /// Gets or sets the libraries used
        /// </summary>
        [XmlArray("references"), XmlArrayItem("ref"), JsonProperty("references")]
        public List<String> Libraries { get; set; }

        /// <summary>
        /// Gets or sets the entry frame
        /// </summary>
        [XmlElement("frame"), JsonProperty("frame")]
        public CdssDiagnosticFrame EntryFrame { get; set; }

        /// <summary>
        /// Save this object to <paramref name="ms"/>
        /// </summary>
        public void Save(MemoryStream ms)
        {
            s_xsz.Serialize(ms, this);
        }
    }
}
