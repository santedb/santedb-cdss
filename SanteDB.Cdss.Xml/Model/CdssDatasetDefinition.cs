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
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Http.Compression;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// CDSS dataset definition
    /// </summary>
    [XmlType(nameof(CdssDatasetDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssDatasetDefinition : CdssBaseObjectDefinition
    {

        /// <summary>
        /// Gets or sets the compression scheme name
        /// </summary>
        [XmlAttribute("compress"), JsonProperty("compress")]
        public Core.Http.Description.HttpCompressionAlgorithm CompressionScheme { get; set; }

        /// <summary>
        /// Reference data
        /// </summary>
        [XmlText, JsonProperty("data")]
        public String XmlData { get; set; }

        /// <summary>
        /// Gets or sets the raw data
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public byte[] RawData
        {
            get
            {
                if (string.IsNullOrEmpty(this.XmlData))
                {
                    return null;
                }
                else if (this.CompressionScheme == Core.Http.Description.HttpCompressionAlgorithm.None)
                {
                    return Encoding.UTF8.GetBytes(this.XmlData);
                }
                else
                {
                    using (var ms = new MemoryStream(Convert.FromBase64String(this.XmlData)))
                    {
                        using (var cs = CompressionUtil.GetCompressionScheme(this.CompressionScheme).CreateDecompressionStream(ms))
                        {
                            using (var oms = new MemoryStream())
                            {
                                cs.CopyTo(oms);
                                return oms.ToArray();
                            }
                        }
                    }
                }
            }
            set
            {
                if (value == null)
                {
                    this.XmlData = null;
                }
                else if (this.CompressionScheme == Core.Http.Description.HttpCompressionAlgorithm.None)
                {
                    this.XmlData = Encoding.UTF8.GetString(value);
                }
                else
                {
                    using (var ms = new MemoryStream(value))
                    {
                        using (var oms = new MemoryStream())
                        {
                            using (var cs = CompressionUtil.GetCompressionScheme(this.CompressionScheme).CreateCompressionStream(oms))
                            {
                                ms.CopyTo(cs);
                            }
                            this.XmlData = Convert.ToBase64String(oms.ToArray());

                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if (String.IsNullOrEmpty(this.XmlData))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.dataset.dataMissing", "Reference data provided in a CDSS library must contain CSV data", Guid.Empty, this.ToString());
            }
            if (String.IsNullOrEmpty(this.Id) || string.IsNullOrEmpty(this.Name))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.dataset.unidentified", "Reference data sets provided in CDSS libraries must contain a name", Guid.Empty, this.ToString());
            }
        }
    }
}