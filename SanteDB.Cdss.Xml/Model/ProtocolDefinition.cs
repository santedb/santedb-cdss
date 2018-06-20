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
using SanteDB.Core.Applets.ViewModel.Description;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace SanteDB.Protocol.Xml.Model
{
    /// <summary>
    /// Protocol definition file
    /// </summary>
    [XmlType(nameof(ProtocolDefinition), Namespace = "http://santedb.org/cdss")]
    [XmlRoot(nameof(ProtocolDefinition), Namespace = "http://santedb.org/cdss")]
    public class ProtocolDefinition : DecisionSupportBaseElement
    {

        private static XmlSerializer s_xsz = new XmlSerializer(typeof(ProtocolDefinition));

        /// <summary>
        /// When clause for the entire protocol
        /// </summary>
        [XmlElement("when")]
        public ProtocolWhenClauseCollection When { get; set; }

        /// <summary>
        /// Gets or sets the rules
        /// </summary>
        [XmlElement("rule")]
        public List<ProtocolRuleDefinition> Rules { get; set; }

        /// <summary>
        /// View model description
        /// </summary>
        [XmlElement("loaderModel", Namespace = "http://santedb.org/model/view")]
        public ViewModelDescription Initialize { get; set; }

        /// <summary>
        /// Represents the OID of the protocol
        /// </summary>
        [XmlElement("oid")]
        public string Oid { get; set; }

        /// <summary>
        /// Save the protocol definition to the specified stream
        /// </summary>
        public void Save(Stream ms)
        {
            s_xsz.Serialize(ms, this);
        }

        /// <summary>
        /// Load the protocol from the stream
        /// </summary>
        public static ProtocolDefinition Load(Stream ms)
        {
            return s_xsz.Deserialize(ms) as ProtocolDefinition;
        }
    }
}