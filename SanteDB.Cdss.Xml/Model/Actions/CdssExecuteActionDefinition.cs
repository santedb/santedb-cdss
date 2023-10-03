using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Model.Assets;
using SanteDB.Core.i18n;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Actions
{

    /// <summary>
    /// CDSS execute
    /// </summary>
    [XmlType(nameof(CdssExecuteActionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssExecuteActionDefinition : CdssActionDefinition
    {

        /// <summary>
        /// The objects which are to be executed in this repeat loop
        /// </summary>
        [XmlElement("rule", typeof(CdssRuleAssetDefinition)),
            XmlElement("assign", typeof(CdssProperyAssignActionDefinition)),
            XmlElement("propose", typeof(CdssProposeActionDefinition)),
            XmlElement("execute", typeof(CdssExecuteActionDefinition)),
            XmlElement("protocol", typeof(CdssProtocolAssetDefinition)),
            XmlElement("raise", typeof(CdssIssueActionDefinition)),
            JsonProperty("actions")]
        public List<object> Statements { get; set; }

        /// <inheritdoc/>
        internal override void Execute(CdssContext cdssContext)
        {
            foreach (var stmt in this.Statements)
            {
                switch (stmt)
                {
                    case CdssActionDefinition action:
                        action.Execute(cdssContext);
                        break;
                    case CdssComputableAssetDefinition asset:
                        asset.Compute(cdssContext);
                        break;
                    default:
                        throw new InvalidOperationException(String.Format(ErrorMessages.TYPE_NOT_FOUND, stmt.GetType()));
                }
            }
        }
    }
}