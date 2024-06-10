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
 * Date: 2023-11-27
 */
using Newtonsoft.Json;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Model.Acts;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;

namespace SanteDB.Cdss.Xml.Model
{

    /// <summary>
    /// Asset data action base
    /// </summary>
    [XmlType(nameof(ProtocolProposeAction), Namespace = "http://santedb.org/cdss")]
    [JsonObject(nameof(ProtocolProposeAction))]
    public class ProtocolProposeAction
    {
        /// <summary>
        /// ctor
        /// </summary>
        public ProtocolProposeAction()
        {
        }

        /// <summary>
        /// Gets the elements to be performed
        /// </summary>
        [XmlElement(nameof(Act), typeof(Act), Namespace = "http://santedb.org/model")]
        [XmlElement(nameof(TextObservation), typeof(TextObservation), Namespace = "http://santedb.org/model")]
        [XmlElement(nameof(SubstanceAdministration), typeof(SubstanceAdministration), Namespace = "http://santedb.org/model")]
        [XmlElement(nameof(QuantityObservation), typeof(QuantityObservation), Namespace = "http://santedb.org/model")]
        [XmlElement(nameof(CodedObservation), typeof(CodedObservation), Namespace = "http://santedb.org/model")]
        [XmlElement(nameof(PatientEncounter), typeof(PatientEncounter), Namespace = "http://santedb.org/model")]
        [XmlElement(nameof(Procedure), typeof(Procedure), Namespace = "http://santedb.org/model")]
        [XmlElement(nameof(DetectedIssue), typeof(DetectedIssue), Namespace = "http://santedb.org/issue")]
        [XmlElement("jsonModel", typeof(String))]
        [JsonProperty("element")]

        public Object Element { get; set; }

        /// <summary>
        /// Associate the specified data for stuff that cannot be serialized
        /// </summary>
        [XmlElement("assign", typeof(PropertyAssignAction))]
        [XmlElement("add", typeof(PropertyAddAction))]
        [JsonProperty("assign")]
        public List<PropertyAction> Do { get; set; }
    }

}