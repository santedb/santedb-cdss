﻿/*
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
using SanteDB.Cdss.Xml.Ami;
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Cdss.Xml.Model.Assets;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Cdss;
using SanteDB.Core.i18n;
using SanteDB.Core.Model.Acts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Actions
{
    /// <summary>
    /// Represents an action whereby the specified object is added to the current context's proposals
    /// </summary>
    [XmlType(nameof(CdssProposeActionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssProposeActionDefinition : CdssActionDefinition
    {

        /// <summary>
        /// Gets or sets the model 
        /// </summary>
        [XmlElement("model"), JsonProperty("model")]
        public CdssModelAssetDefinition Model { get; set; }

        /// <summary>
        /// Gets or sets the property to assign
        /// </summary>
        [XmlElement("assign"), JsonProperty("assign")]
        public List<CdssPropertyAssignActionDefinition> Assignment { get; set; }


        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if (this.Model == null)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.propose.model", "Propose action requires a model", Guid.Empty, this.ToReferenceString());
            }
            if (this.Assignment?.Any() != true)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Warning, "cdss.propose.assign", "Propose action should carry dynamic assignments", Guid.Empty, this.ToReferenceString());
            }
            foreach (var itm in base.Validate(context).Union(this.Assignment.SelectMany(o => o.Validate(context)) ?? new DetectedIssue[0]).Union(this.Model?.Validate(context) ?? new DetectedIssue[0]))
            {
                itm.RefersTo = itm.RefersTo ?? this.ToReferenceString();
                yield return itm;
            }
        }

        /// <inheritdoc/>
        internal override void Execute()
        {
            base.ThrowIfInvalidState();

            if (this.Model == null)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.DEPENDENT_PROPERTY_NULL, nameof(Model)));
            }
            else if(Boolean.TryParse(CdssExecutionStackFrame.Current.GetValue(CdssParameterNames.EXCLUDE_PROPOSALS)?.ToString(), out var exclude) && exclude)
            {
                return;
            }

            using (CdssExecutionStackFrame.EnterChildFrame(this))
            {
                try
                {
                    var model = this.Model.Compute() as Act;
                    model.Protocols = new List<ActProtocol>();
                    // Get any protocols in the execution context hierarchy
                    var ctx = CdssExecutionStackFrame.Current;
                    int? sequence = null;
                    while (ctx != null)
                    {
                        switch (ctx.Owner)
                        {
                            case CdssRepeatActionDefinition repeat:
                                sequence = (int)ctx.GetValue(repeat.IterationVariable);
                                break;

                            case CdssProtocolAssetDefinition protocol:

                                if (!sequence.HasValue) // sequence has not been set by a repeat - so we want to set it via the position of the rule or action in the protocol
                                {
                                    var protocolKey = $"{protocol.Name} Sequence";
                                    ctx.Context.Declare(protocolKey, 0);
                                    sequence = (int?)ctx.Context[protocolKey];
                                    sequence = sequence.GetValueOrDefault() + 1;
                                    ctx.Context[protocolKey] = sequence;
                                }

                                model.Protocols.Add(new ActProtocol()
                                {
                                    ProtocolKey = protocol.Uuid,
                                    Sequence = sequence.Value,
                                    Protocol = new Protocol()
                                    {
                                        Oid = protocol.Oid,
                                        Key = protocol.Uuid,
                                        Name = protocol.Name
                                    },
                                    Version = protocol.Metadata?.Version ?? "1.0"
                                });
                                break;
                        }
                        ctx = ctx.Parent;
                    }

                    // Set the scoped object for this and call the assign actions
                    model.Key = model.Key ?? Guid.NewGuid();
                    CdssExecutionStackFrame.Current.ScopedObject = model;
                    foreach (var asgn in this.Assignment)
                    {
                        asgn.Execute();
                    }
                    CdssExecutionStackFrame.Current.Context.PushProposal(model);
                }
                catch (Exception e) when (!(e is CdssEvaluationException))
                {
                    throw new CdssEvaluationException($"Error computing {this.Name ?? this.Id}", e);
                }
            }
        }
    }
}
