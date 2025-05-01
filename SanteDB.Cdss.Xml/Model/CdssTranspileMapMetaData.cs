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
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// Contains metadata about transpiled source
    /// </summary>
    [XmlType(nameof(CdssTranspileMapMetaData), Namespace = "http://santedb.org/cdss")]
    public class CdssTranspileMapMetaData
    {
        public CdssTranspileMapMetaData()
        {

        }
        /// <summary>
        /// Creates a new transpilation metadata for original source
        /// </summary>
        public CdssTranspileMapMetaData(int startLine, int startColumn, int stopLine, int stopColumn)
        {
            this.StartPosition = $"{startLine}:{startColumn}";
            this.EndPoisition = $"{stopLine}:{stopColumn}";
        }

        /// <summary>
        /// Source file
        /// </summary>
        [XmlAttribute("source"), JsonProperty("source")]
        public string SourceFileName { get; set; }

        /// <summary>
        /// Gets or sets the line
        /// </summary>
        [XmlAttribute("start"), JsonProperty("start")]
        public string StartPosition { get; set; }

        /// <summary>
        /// Gets or sets the column
        /// </summary>
        [XmlAttribute("stop"), JsonProperty("stop")]
        public string EndPoisition { get; set; }

        /// <summary>
        /// Represents the original source code
        /// </summary>
        [XmlElement("src"), JsonProperty("src")]
        public byte[] OriginalSource { get; set; }

    }
}