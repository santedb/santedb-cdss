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
using SanteDB.Cdss.Xml.Model.Assets;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Actions
{
    /// <summary>
    /// Inline rule action
    /// </summary>
    /// <remarks>This is a hack since <see cref="CdssRuleAssetDefinition"/> is an asset whereas this is an inline rule </remarks>
    [XmlType(nameof(CdssInlineRuleActionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssInlineRuleActionDefinition : CdssActionDefinition, IHasCdssActions
    {

        /// <summary>
        /// Rule asset
        /// </summary>
        private CdssRuleAssetDefinition m_ruleAsset;

        /// <summary>
        /// Represents the "when" clause for the rule
        /// </summary>
        [XmlElement("when"), JsonProperty("when")]
        public CdssWhenDefinition When { get; set; }

        /// <summary>
        /// Action definition
        /// </summary>
        [XmlElement("then"), JsonProperty("then")]
        public CdssActionCollectionDefinition Actions { get; set; }

        internal override void Execute()
        {
            if (this.m_ruleAsset == null)
            {
                this.m_ruleAsset = new CdssRuleAssetDefinition()
                {
                    Id = this.Id,
                    Name = this.Name,
                    Oid = this.Oid,
                    Uuid = this.Uuid,
                    When = this.When,
                    Actions = this.Actions
                };
            }
            using (CdssExecutionStackFrame.EnterChildFrame(this))
            {
                this.m_ruleAsset.Compute();
            }
        }
    }
}
