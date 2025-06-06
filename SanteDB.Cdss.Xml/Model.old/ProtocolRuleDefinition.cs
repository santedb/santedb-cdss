﻿/*
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
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// Represents a single rule definition
    /// </summary>
    [XmlType(nameof(ProtocolRuleDefinition), Namespace = "http://santedb.org/cdss")]
    [JsonObject(nameof(ProtocolRuleDefinition))]
    public class ProtocolRuleDefinition : DecisionSupportBaseElement
    {
        /// <summary>
        /// Creates a new protocol rule definition
        /// </summary>
        public ProtocolRuleDefinition()
        {
            this.Repeat = 1;
            this.Variables = new List<ProtocolVariableDefinition>();
        }

        /// <summary>
        /// Repeat?
        /// </summary>
        [XmlAttribute("repeat"), JsonProperty("repeat")]
        public int Repeat { get; set; }

        /// <summary>
        /// Variables
        /// </summary>
        [XmlElement("variable"), JsonProperty("variable")]
        public List<ProtocolVariableDefinition> Variables { get; set; }

        /// <summary>
        /// Represents a WHEN condition
        /// </summary>
        [XmlElement("when"), JsonProperty("when")]
        public ProtocolWhenClauseCollection When { get; set; }

        /// <summary>
        /// Represents the THEN conditions
        /// </summary>
        [XmlElement("then"), JsonProperty("then")]
        public ProtocolThenClauseCollection Then { get; set; }
    }
}