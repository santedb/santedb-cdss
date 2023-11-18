using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Model.Actions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Assets
{
    /// <summary>
    /// Represents a computable collection
    /// </summary>
    [XmlType(nameof(CdssCollectionComputableAssetDefinition), Namespace = "http://santedb.org/cdss")]
    public abstract class CdssCollectionComputableAssetDefinition : CdssComputableAssetDefinition
    {
        /// <summary>
        /// Action definition
        /// </summary>
        [XmlArray("then"),
            XmlArrayItem("propose", typeof(CdssProposeActionDefinition)),
            XmlArrayItem("assign", typeof(CdssPropertyAssignActionDefinition)),
            XmlArrayItem("raise", typeof(CdssIssueActionDefinition)),
            XmlArrayItem("repeat", typeof(CdssRepeatActionDefinition)),
            XmlArrayItem("apply", typeof(CdssRuleReferenceActionDefinition)),
            XmlArrayItem("rule", typeof(CdssRuleAssetDefinition)),
            JsonProperty("then")]
        public List<CdssBaseObjectDefinition> Actions { get; set; }

    }
}
