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
using SanteDB.Core.Applets.ViewModel.Description;
using SanteDB.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// Represents a ruleset
    /// </summary>
    [XmlType(nameof(ProtocolRulesetDefinition), Namespace = "http://santedb.org/cdss")]
    public class ProtocolRulesetDefinition : DecisionSupportBaseElement
    {

        /// <summary>
        /// Applies to which resources
        /// </summary>
        [XmlElement("appliesTo"), JsonProperty("appliesTo")]
        public List<ProtocolResourceTypeReference> AppliesTo { get; set; }

        /// <summary>
        /// When clause for the entire protocol ruleset
        /// </summary>
        [XmlElement("when"), JsonProperty("when")]
        public ProtocolWhenClauseCollection When { get; set; }

        /// <summary>
        /// Gets or sets the variables for the entire protocol
        /// </summary>
        [XmlElement("variable"), JsonProperty("variable")]
        public List<ProtocolVariableDefinition> Variables { get; set; }

        /// <summary>
        /// Gets or sets the transforms
        /// </summary>
        [XmlElement("transform"), JsonProperty("transform")]
        public List<ProtocolTransformDefinition> Transforms { get; set; }

        /// <summary>
        /// Gets or sets the rules
        /// </summary>
        [XmlElement("rule"), JsonProperty("rule")]
        public List<ProtocolRuleDefinition> Rules { get; set; }

        /// <summary>
        /// View model description
        /// </summary>
        [XmlElement("modelLoad", Namespace = "http://santedb.org/model/view"), JsonProperty("modelLoad")]
        public ViewModelDescription Initialize { get; set; }

    }
}
