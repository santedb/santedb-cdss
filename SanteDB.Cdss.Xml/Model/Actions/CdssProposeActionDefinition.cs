using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Model.Assets;
using SanteDB.Core.Applets.ViewModel.Json;
using SanteDB.Core.Cdss;
using SanteDB.Core.i18n;
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
        JsonProperty("json")]
        public object Model { get; set; }

        /// <summary>
        /// Gets or sets the property to assign
        /// </summary>
        [XmlElement("assign"), JsonProperty("assign")]
        public List<CdssProperyAssignActionDefinition> Assignment { get; set; }

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

                Act model = null;
                switch (this.Model)
                {
                    case String jsonString:
                        model = s_serializer.DeSerialize<Act>(jsonString);
                        break;
                    case Act act:
                        model = act.DeepCopy() as Act;
                        break;
                }

                model.Protocols = new List<ActProtocol>();
                // Get any protocols in the execution context hierarchy
                var ctx = CdssExecutionStackFrame.Current;
                var sequence = 0;
                while (ctx != null)
                {
                    switch(ctx.Owner)
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
                }

                // Set the scoped object for this and call the assign actions
                model.Key = model.Key ?? Guid.NewGuid();
                CdssExecutionStackFrame.Current.ScopedObject = model;
                foreach(var asgn in this.Assignment)
                {
                    asgn.Execute();
                }
            }
        }
    }
}
