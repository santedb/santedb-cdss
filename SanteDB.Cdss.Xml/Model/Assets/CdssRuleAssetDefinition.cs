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
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Cdss.Xml.Model.Actions;
using SanteDB.Core.BusinessRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Assets
{
    /// <summary>
    /// CDSS rule asset definition
    /// </summary>
    [XmlType(nameof(CdssRuleAssetDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssRuleAssetDefinition : CdssComputableAssetDefinition, IHasCdssActions
    {

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


        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if (this.When == null)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Warning, "cdss.rule.whenRecommended", "Rules should carry a WHEN condition unless they are globally applied", Guid.Empty, this.ToReferenceString());
            }
            if (this.Actions == null)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.rule.thenRequired", "Rules must carry a THEN block", Guid.Empty, this.ToReferenceString());
            }
            foreach (var itm in base.Validate(context)
                .Union(this.Actions?.Validate(context) ?? new DetectedIssue[0])
                .Union(this.When?.Validate(context) ?? new DetectedIssue[0]))
            {
                itm.RefersTo = itm.RefersTo ?? this.ToReferenceString();
                yield return itm;
            }
        }

        /// <summary>
        /// Compute the rule and execute any actions in the rule
        /// </summary>
        /// <returns>True if the rule was executed, false if it was not executed</returns>
        public override object Compute()
        {
            base.ThrowIfInvalidState();

            // Has this rule already been executed? 
            using (CdssExecutionStackFrame.EnterChildFrame(this))
            {
                try
                {
                    if (this.When == null || this.When.Compute() is bool b && b)
                    {
                        this.Actions.Execute();
                        return true;
                    }

                    return false;
                }
                catch (Exception e) when (!(e is CdssEvaluationException))
                {
                    throw new CdssEvaluationException($"Error computing {this.Name ?? this.Id}", e);
                }
            }

        }
    }
}