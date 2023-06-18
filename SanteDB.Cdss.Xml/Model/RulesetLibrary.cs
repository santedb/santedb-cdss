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
using SanteDB.Core.Model.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// Represents a collection of common rules or then conditions which can be included in other contexts
    /// </summary>
    [XmlType(nameof(RulesetLibrary), Namespace = "http://santedb.org/cdss")]
    [XmlRoot(nameof(RulesetLibrary), Namespace = "http://santedb.org/cdss")]
    public class RulesetLibrary : DecisionSupportBaseElement
    {
        private static XmlSerializer s_xsz = XmlModelSerializerFactory.Current.CreateSerializer(typeof(RulesetLibrary));

        /// <summary>
        /// When clause for the entire ruleset
        /// </summary>
        [XmlElement("clause")]
        public List<ProtocolWhenClauseCollection> When { get; set; }

        /// <summary>
        /// Gets or sets the rules
        /// </summary>
        [XmlElement("rule")]
        public List<ProtocolRuleDefinition> Rules { get; set; }

        /// <summary>
        /// Save the rules definition to the specified stream
        /// </summary>
        public void Save(Stream ms)
        {
            s_xsz.Serialize(ms, this);
        }

        /// <summary>
        /// Load the rules from the stream
        /// </summary>
        public static RulesetLibrary Load(Stream ms)
        {
            return s_xsz.Deserialize(ms) as RulesetLibrary;
        }
    }
}