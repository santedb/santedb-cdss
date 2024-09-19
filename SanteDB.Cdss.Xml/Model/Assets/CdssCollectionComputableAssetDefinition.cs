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
using SanteDB.Cdss.Xml.Model.Actions;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Assets
{
    /// <summary>
    /// Represents a computable collection
    /// </summary>
    [XmlType(nameof(CdssCollectionComputableAssetDefinition), Namespace = "http://santedb.org/cdss")]
    public abstract class CdssCollectionComputableAssetDefinition : CdssComputableAssetDefinition
    {
        /// <summary>
        /// Action definition
        /// </summary>
        [XmlArray("then"),
            XmlArrayItem("propose", typeof(CdssProposeActionDefinition)),
            XmlArrayItem("assign", typeof(CdssPropertyAssignActionDefinition)),
            XmlArrayItem("raise", typeof(CdssIssueActionDefinition)),
            XmlArrayItem("repeat", typeof(CdssRepeatActionDefinition)),
            XmlArrayItem("apply", typeof(CdssRuleReferenceActionDefinition)),
            XmlArrayItem("rule", typeof(CdssRuleAssetDefinition)),
            JsonProperty("then")]
        public List<CdssBaseObjectDefinition> Actions { get; set; }

    }
}
