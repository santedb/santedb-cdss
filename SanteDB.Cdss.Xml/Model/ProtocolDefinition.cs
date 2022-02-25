/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-27
 */
using SanteDB.Core.Applets.ViewModel.Description;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// Protocol definition file
    /// </summary>
    [XmlType(nameof(ProtocolDefinition), Namespace = "http://santedb.org/cdss")]
    [XmlRoot(nameof(ProtocolDefinition), Namespace = "http://santedb.org/cdss")]
    public class ProtocolDefinition : DecisionSupportBaseElement
    {
        private static XmlSerializer s_xsz = XmlModelSerializerFactory.Current.CreateSerializer(typeof(ProtocolDefinition));

        /// <summary>
        /// Includes documentation about the protocol definition
        /// </summary>
        [XmlElement("documentation")]
        public Narrative Documentation { get; set; }

        /// <summary>
        /// Includes a ruleset definition file
        /// </summary>
        [XmlElement("include")]
        public List<string> Include { get; set; }

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
            using (var xr = XmlReader.Create(ms, new XmlReaderSettings()
            {
                IgnoreWhitespace = true
            }))
                return s_xsz.Deserialize(xr) as ProtocolDefinition;
        }
    }
}