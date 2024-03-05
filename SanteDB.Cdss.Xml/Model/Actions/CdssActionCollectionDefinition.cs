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
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Core.BusinessRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Actions
{

    /// <summary>
    /// Represents a collection of actions
    /// </summary>
    [XmlType(nameof(CdssActionCollectionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssActionCollectionDefinition : CdssBaseObjectDefinition
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public CdssActionCollectionDefinition()
        {
            this.Actions = new List<CdssActionDefinition>();
        }

        /// <summary>
        /// Gets or sets the defined actions
        /// </summary>
        [XmlElement("propose", typeof(CdssProposeActionDefinition)),
            XmlElement("assign", typeof(CdssPropertyAssignActionDefinition)),
            XmlElement("raise", typeof(CdssIssueActionDefinition)),
            XmlElement("repeat", typeof(CdssRepeatActionDefinition)),
            XmlElement("apply", typeof(CdssRuleReferenceActionDefinition)),
            XmlElement("rule", typeof(CdssInlineRuleActionDefinition)),
            JsonProperty("action")]
        public List<CdssActionDefinition> Actions { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if (this.Actions?.Any() != true)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.actionCollection.actionsMissing", "CDSS Action collection must carry at least one action", Guid.Empty, this.ToReferenceString());
            }
            else
            {
                foreach (var itm in this.Actions.SelectMany(o => o.Validate(context)))
                {
                    yield return itm;
                }
            }
        }

        /// <inheritdoc/>
        internal void Execute()
        {
            using (CdssExecutionStackFrame.EnterChildFrame(this))
            {
                try
                {
                    foreach (var stmt in this.Actions)
                    {
                        stmt.Execute();
                    }
                }
                catch (Exception e) when (!(e is CdssEvaluationException))
                {
                    throw new CdssEvaluationException($"Error computing {this.Name ?? this.Id}", e);
                }
            }
        }

    }
}