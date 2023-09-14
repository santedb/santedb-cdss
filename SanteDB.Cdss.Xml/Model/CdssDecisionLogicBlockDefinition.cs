using Newtonsoft.Json;
using System.Collections.Generic;
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
        /// Gets or sets the logic elements that this decision ruleset block defines
        /// </summary>
        [XmlArray("define"),
            XmlArrayItem("fact", typeof(CdssFactAssetDefinition)),
            XmlArrayItem("rule", typeof(CdssRuleAssetDefinition)),
            XmlArrayItem("protocol", typeof(CdssProtocolAssetDefinition)),
            XmlArrayItem("normalize", typeof(CdssNormalizeAssetDefinition)),
            JsonProperty("define")]
        public List<CdssComputableAssetDefinition> Definitions { get; set; }

    }
}