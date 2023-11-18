using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Model.Assets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Actions
{
    /// <summary>
    /// Inline rule action
    /// </summary>
    /// <remarks>This is a hack since <see cref="CdssRuleAssetDefinition"/> is an asset whereas this is an inline rule </remarks>
    [XmlType(nameof(CdssInlineRuleActionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssInlineRuleActionDefinition : CdssActionDefinition, IHasCdssActions
    {

        /// <summary>
        /// Rule asset
        /// </summary>
        private CdssRuleAssetDefinition m_ruleAsset;

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

        internal override void Execute()
        {
            if(this.m_ruleAsset == null)
            {
                this.m_ruleAsset = new CdssRuleAssetDefinition()
                {
                    Id = this.Id,
                    Name = this.Name,
                    Oid = this.Oid,
                    Uuid = this.Uuid,
                    When = this.When,
                    Actions = this.Actions
                };
            }
            this.m_ruleAsset.Compute();
        }
    }
}
