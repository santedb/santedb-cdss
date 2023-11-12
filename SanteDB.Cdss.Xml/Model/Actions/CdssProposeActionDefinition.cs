﻿using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Cdss.Xml.Model.Assets;
using SanteDB.Core.Applets.ViewModel.Json;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Cdss;
using SanteDB.Core.i18n;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Actions
{
    /// <summary>
    /// Represents an action whereby the specified object is added to the current context's proposals
    /// </summary>
    [XmlType(nameof(CdssProposeActionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssProposeActionDefinition : CdssActionDefinition
    {
        // JSON Serializer
        private static JsonViewModelSerializer s_serializer = new JsonViewModelSerializer();

        // Parsed model
        private Act m_parsedModel;

        /// <summary>
        /// Gets or sets the model 
        /// </summary>
        [XmlElement("json", typeof(String)),
        XmlElement(nameof(Act), typeof(Act), Namespace = "http://santedb.org/model"),
        XmlElement(nameof(TextObservation), typeof(TextObservation), Namespace = "http://santedb.org/model"),
        XmlElement(nameof(SubstanceAdministration), typeof(SubstanceAdministration), Namespace = "http://santedb.org/model"),
        XmlElement(nameof(QuantityObservation), typeof(QuantityObservation), Namespace = "http://santedb.org/model"),
        XmlElement(nameof(CodedObservation), typeof(CodedObservation), Namespace = "http://santedb.org/model"),
        XmlElement(nameof(PatientEncounter), typeof(PatientEncounter), Namespace = "http://santedb.org/model"),
        XmlElement(nameof(Procedure), typeof(Procedure), Namespace = "http://santedb.org/model"),
            XmlElement(nameof(Narrative), typeof(Narrative), Namespace = "http://santedb.org/model"),
        JsonProperty("json")]
        public object Model { get; set; }

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
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.propose.model", "Propose action requires a model", Guid.Empty, this.ToString());
            }
            if (this.Assignment?.Any() != true)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Warning, "cdss.propose.assign", "Propose action should carry dynamic assignments", Guid.Empty, this.ToString());
            }
            foreach (var itm in base.Validate(context).Union(this.Assignment.SelectMany(o => o.Validate(context)) ?? new DetectedIssue[0]))
            {
                itm.RefersTo = itm.RefersTo ?? this.ToString();
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

            using (CdssExecutionStackFrame.EnterChildFrame(this))
            {
                try
                {
                    if (this.m_parsedModel == null)
                    {
                        switch (this.Model)
                        {
                            case String jsonString:
                                this.m_parsedModel = s_serializer.DeSerialize<Act>(jsonString);
                                break;
                            case Act act:
                                this.m_parsedModel = act.DeepCopy() as Act;
                                break;
                        }
                    }
                    Act model = this.m_parsedModel.DeepCopy() as Act;
                    model.Protocols = new List<ActProtocol>();
                    // Get any protocols in the execution context hierarchy
                    var ctx = CdssExecutionStackFrame.Current;
                    var sequence = 0;
                    while (ctx != null)
                    {
                        switch (ctx.Owner)
                        {
                            case CdssRepeatActionDefinition repeat:
                                sequence = (int)ctx.GetValue(repeat.IterationVariable);
                                break;
                            case CdssProtocolAssetDefinition protocol:
                                model.Protocols.Add(new ActProtocol()
                                {
                                    ProtocolKey = protocol.Uuid,
                                    Sequence = sequence,
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
