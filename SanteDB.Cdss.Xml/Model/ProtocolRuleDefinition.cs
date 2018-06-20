﻿/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
 *
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
 * Date: 2017-9-1
 */
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Protocol.Xml.Model
{
    /// <summary>
    /// Represents a single rule definition
    /// </summary>
    [XmlType(nameof(ProtocolRuleDefinition), Namespace = "http://santedb.org/cdss")]
    public class ProtocolRuleDefinition : DecisionSupportBaseElement
    {

        public ProtocolRuleDefinition()
        {
            this.Repeat = 1;
            this.Variables = new List<ProtocolVariableDefinition>();
        }

        /// <summary>
        /// Repeat?
        /// </summary>
        [XmlAttribute("repeat")]
        public int Repeat { get; set; }

        /// <summary>
        /// Variables
        /// </summary>
        [XmlElement("variable")]
        public List<ProtocolVariableDefinition> Variables { get; set; }

        /// <summary>
        /// Represents a WHEN condition
        /// </summary>
        [XmlElement("when")]
        public ProtocolWhenClauseCollection When { get; set; }

        /// <summary>
        /// Represents the THEN conditions
        /// </summary>
        [XmlElement("then")]
        public ProtocolThenClauseCollection Then { get; set; }
    }
}