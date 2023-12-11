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
