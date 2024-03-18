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
using DynamicExpresso;
using Newtonsoft.Json;
using SanteDB;
using SanteDB.Core.Applets.ViewModel.Json;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.i18n;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.Roles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// Reperesents a then condition clause
    /// </summary>
    [XmlType(nameof(ProtocolThenClauseCollection), Namespace = "http://santedb.org/cdss")]
    [JsonObject(nameof(ProtocolThenClauseCollection))]
    public class ProtocolThenClauseCollection : DecisionSupportBaseElement
    {
        // JSON Serializer
        private static JsonViewModelSerializer s_serializer = new JsonViewModelSerializer();

        /// <summary>
        /// Actions to be performed
        /// </summary>
        [XmlElement("propose", Type = typeof(ProtocolProposeAction))]
        [XmlElement("ref", Type = typeof(CdssObjectReference))]
        [XmlElement("alert", Type = typeof(DetectedIssue))]
        [JsonProperty("action")]
        public List<Object> Action { get; set; }

        /// <summary>
        /// Evaluate the actions
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Act> Evaluate(CdssContext context)
        {
            Dictionary<String, Object> calculatedScopes = new Dictionary<string, object>()
            {
                { ".", context.Target }
            };

            foreach (var itm in this.Action
                .SelectMany(a=>a is CdssObjectReference cors ? cors.Resolve<ProtocolThenClauseCollection>().Action.OfType<Object>() : new object[] { a })
            )
            {
                switch(itm)
                {
                    case ProtocolProposeAction dataAction:
                        {
                            if (dataAction.Element is String jsonString) // JSON
                            {
                                dataAction.Element = s_serializer.DeSerialize<Act>(jsonString);
                                // Load all concepts for the specified objects
                            }
                            if (dataAction.Element is Act act)
                            {
                                act = act.Clone() as Act;
                                act.Participations = act.Participations?.Select(o => o.Clone() as ActParticipation).ToList();
                                act.Relationships = act.Relationships?.Select(o => o.Clone() as ActRelationship).ToList();
                                act.Protocols = new List<ActProtocol>();// (itm.Element as Act).Protocols);
                                                                        // Now do the actions to the properties as stated
                                foreach (var instr in dataAction.Do)
                                {
                                    instr.Evaluate(act, context, calculatedScopes);
                                }

                                // Assign this patient as the record target
                                act.Key = act.Key ?? Guid.NewGuid();
                                Guid pkey = Guid.NewGuid();

                                act.LoadProperty(o => o.Participations).Add(new ActParticipation(ActParticipationKeys.RecordTarget, context.Target.Key) { ParticipationRole = new Core.Model.DataTypes.Concept() { Key = ActParticipationKeys.RecordTarget, Mnemonic = "RecordTarget" }, Key = pkey });
                                // Add record target to the source for forward rules
                                context.Target.LoadProperty(o => o.Participations).Add(new ActParticipation(ActParticipationKeys.RecordTarget, context.Target) { SourceEntity = act, ParticipationRole = new Core.Model.DataTypes.Concept() { Key = ActParticipationKeys.RecordTarget, Mnemonic = "RecordTarget" }, Key = pkey });
                                act.CreationTime = DateTimeOffset.Now;
                                // The act to the return value
                                yield return act;
                            }
                            else
                            {
                                throw new InvalidOperationException(String.Format(ErrorMessages.ARGUMENT_INCOMPATIBLE_TYPE, typeof(Act), dataAction.Element.GetType()));
                            }
                            break;
                        }
                    case DetectedIssue dte:
                        context.AddIssue(dte); // TODO: Allow templating of issue text
                        break;
                }
                
               
            }
        }
    }

    /// <summary>
    /// Associate data
    /// </summary>
    [XmlType(nameof(PropertyAction), Namespace = "http://santedb.org/cdss")]
    public abstract class PropertyAction : ProtocolProposeAction
    {
        /// <summary>
        /// Action name
        /// </summary>
        public abstract String ActionName { get; }

        /// <summary>
        /// The name of the property
        /// </summary>
        [XmlAttribute("name"), JsonProperty("name")]
        public String Name { get; set; }

        /// <summary>
        /// Evaluate the expression
        /// </summary>
        /// <returns></returns>
        public abstract object Evaluate(Act act, CdssContext context, IDictionary<String, Object> scopes);
    }

    /// <summary>
    /// Add something to a property collection
    /// </summary>
    [XmlType(nameof(PropertyAddAction), Namespace = "http://santedb.org/cdss")]
    public class PropertyAddAction : PropertyAction
    {
        /// <summary>
        /// Evaluate
        /// </summary>
        public override object Evaluate(Act act, CdssContext context, IDictionary<String, Object> scopes)
        {
            var value = act.GetType().GetRuntimeProperty(this.Name) as IList;
            value?.Add(this.Element);
            return value;
        }

        /// <summary>
        /// Add action
        /// </summary>
        public override string ActionName
        {
            get
            {
                return "Add";
            }
        }
    }
}