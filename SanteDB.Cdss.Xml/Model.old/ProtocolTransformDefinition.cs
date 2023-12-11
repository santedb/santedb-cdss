using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// Transform definitions
    /// </summary>
    [XmlType(nameof(ProtocolVariableDefinition), Namespace = "http://santedb.org/cdss")]
    public class ProtocolTransformDefinition
    {

        /// <summary>
        /// When the conditions are true
        /// </summary>
        [XmlElement("when"), JsonProperty("when")]
        public ProtocolWhenClauseCollection When { get; set; }

        /// <summary>
        /// Protocol then actions
        /// </summary>
        [XmlArray("then"), XmlArrayItem("assign"), JsonProperty("then")]
        public List<PropertyAssignAction> Then { get; set; }
    }
}