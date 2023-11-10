﻿using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Model.Actions;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Assets
{
    /// <summary>
    /// CDSS rule asset definition
    /// </summary>
    [XmlType(nameof(CdssRuleAssetDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssRuleAssetDefinition : CdssComputableAssetDefinition
    {

        /// <summary>
        /// Represents the "when" clause for the rule
        /// </summary>
        [XmlElement("when"), JsonProperty("when")]
        public CdssWhenDefinition When { get; set; }

        /// <summary>
        /// Action definition
        /// </summary>
        [XmlArray("then"),
            XmlArrayItem("propose", typeof(CdssProposeActionDefinition)),
            XmlArrayItem("assign", typeof(CdssProperyAssignActionDefinition)),
            XmlArrayItem("raise", typeof(CdssIssueActionDefinition)),
            XmlArrayItem("repeat", typeof(CdssRepeatActionDefinition)),
            XmlArrayItem("apply", typeof(CdssExecuteActionDefinition)),
            JsonProperty("then")]
        public List<CdssActionDefinition> Actions { get; set; }

        /// <summary>
        /// Compute the rule and execute any actions in the rule
        /// </summary>
        /// <returns>True if the rule was executed, false if it was not executed</returns>
        public override object Compute()
        {
            base.ThrowIfInvalidState();

            using (CdssExecutionStackFrame.EnterChildFrame(this))
            {
                if (this.When?.Compute() != false)
                {
                    foreach (var act in this.Actions)
                    {
                        act.Execute();
                    }
                    return true;
                }

                return null;
            }

        }
    }
}