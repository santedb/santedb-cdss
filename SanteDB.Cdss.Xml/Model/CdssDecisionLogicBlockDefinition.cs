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
using SanteDB.Cdss.Xml.Model.Assets;
using SanteDB.Core.BusinessRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// Ruleset definition
    /// </summary>
    [XmlType(nameof(CdssDecisionLogicBlockDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssDecisionLogicBlockDefinition : CdssBaseObjectDefinition
    {

        /// <summary>
        /// The types this applies to
        /// </summary>
        [XmlElement("context"), JsonProperty("context")]
        public CdssResourceTypeReference Context { get; set; }

        /// <summary>
        /// Only apply this logic block when the provided conditions are true
        /// </summary>
        [XmlElement("when"), JsonProperty("when")]
        public CdssWhenDefinition When { get; set; }

        /// <summary>
        /// Gets or sets the logic elements that this decision ruleset block defines
        /// </summary>
        [XmlArray("define"),
            XmlArrayItem("fact", typeof(CdssFactAssetDefinition)),
            XmlArrayItem("rule", typeof(CdssRuleAssetDefinition)),
            XmlArrayItem("protocol", typeof(CdssProtocolAssetDefinition)),
            XmlArrayItem("model", typeof(CdssModelAssetDefinition)),
            JsonProperty("define")]
        public List<CdssComputableAssetDefinition> Definitions { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if (this.Context == null)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.logic.contextMissing", "CDSS logic blocks must declare a context (type of data) to which the logic block applies", Guid.Empty, this.ToReferenceString());
            }
            if (this.When == null)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Information, "cdss.logic.globalLogic", $"CDSS logic block will be applied to all instances of {this.Context}", Guid.Empty, this.ToReferenceString());
            }
            if (String.IsNullOrEmpty(this.Name))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.logic.nameRequired", "CDSS logic block should carry a name", Guid.Empty, this.ToReferenceString());
            }
            if (this.Definitions?.Any() != true)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.logic.definitionsMissing", "CDSS logic block should contain at least one definition", Guid.Empty, this.ToReferenceString());
            }
            else
            {
                foreach (var itm in this.Definitions.SelectMany(o => o.Validate(context)).Union(this.When?.Validate(context) ?? new DetectedIssue[0]))
                {
                    itm.RefersTo = itm.RefersTo ?? this.ToReferenceString();
                    yield return itm;
                }
            }
        }

    }
}