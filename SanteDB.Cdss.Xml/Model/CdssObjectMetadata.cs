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
 */
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// Metadata about an object in the CDSS subsystem
    /// </summary>
    [XmlType(nameof(CdssObjectMetadata), Namespace = "http://santedb.org/cdss")]
    public class CdssObjectMetadata
    {

        /// <summary>
        /// Gets or sets the authors for the object
        /// </summary>
        [XmlArray("authors"), XmlArrayItem("add"), JsonProperty("authors")]
        public List<string> Authors { get; set; }

        /// <summary>
        /// Gets or sets the version code of the CDSS object
        /// </summary>
        [XmlElement("version"), JsonProperty("version")]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the documentation of the object
        /// </summary>
        [XmlElement("documentation"), JsonProperty("documentation")]
        public string Documentation { get; set; }
    }
}