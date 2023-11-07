using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Assets
{
    /// <summary>
    /// CDSS clinical protocol definition
    /// </summary>
    /// <remarks>A protocol is essentially a rule, however the protocol signals to 
    /// the interpreter that the protocol is an "entry point" for each rule</remarks>
    [XmlType(nameof(CdssProtocolAssetDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssProtocolAssetDefinition : CdssRuleAssetDefinition
    {

        /// <summary>
        /// Gets or sets the scopes where this protocol should be applied
        /// </summary>
        [XmlArray("scopes"), XmlArrayItem("type"), JsonProperty("scopes")]
        public List<CdssProtocolGroupDefinition> Scopes { get; set; }

    }
}
