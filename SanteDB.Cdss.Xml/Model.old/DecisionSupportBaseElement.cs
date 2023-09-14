/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-5-19
 */
using Newtonsoft.Json;
using System;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// Represents a base protocol element
    /// </summary>
    [XmlType(nameof(DecisionSupportBaseElement), Namespace = "http://santedb.org/cdss")]
    [JsonObject(nameof(DecisionSupportBaseElement))]
    public abstract class DecisionSupportBaseElement
    {

        /// <summary>
        /// Gets or sets the identifier of the object
        /// </summary>
        [XmlAttribute("uuid"), JsonProperty("uuid")]
        public Guid Uuid { get; set; }

        /// <summary>
        /// Gets or sets the OID for the object
        /// </summary>
        [XmlAttribute("oid"), JsonProperty("oid")]
        public String Oid { get; set; }

        /// <summary>
        /// Identifies the object within the protocol
        /// </summary>
        [XmlAttribute("id"), JsonProperty("id")]
        public String Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the object
        /// </summary>
        [XmlAttribute("name"), JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the version of the object
        /// </summary>
        [XmlAttribute("version"), JsonProperty("version")]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the documentation for the element
        /// </summary>
        [XmlElement("annotation"), JsonProperty("annotation")]
        public String Documentation { get; set; }

    }
}