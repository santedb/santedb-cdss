using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
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
        public CdssFactAssetDefinition When { get; set; }

        /// <summary>
        /// Action definition
        /// </summary>
        [XmlArray("then"), 
            XmlArrayItem("propose", typeof(CdssProposeActionDefinition)),
            XmlArrayItem("assign", typeof(CdssAssignActionDefinition)),
            XmlArrayItem("execute", typeof(CdssExecuteActionDefinition))]
        public List<CdssActionDefinition> Actions { get; set; }

        /// <summary>
        /// Compute the rule and execute any actions in the rule
        /// </summary>
        /// <typeparam name="TContext">The type of context to which the rule applies</typeparam>
        /// <param name="cdssContext">The current CDSS context</param>
        /// <returns>True if the rule was executed, false if it was not executed</returns>
        internal override object Compute<TContext>(CdssContext<TContext> cdssContext)
        {

            var whenResult = this.When.Compute(cdssContext);
            if(whenResult is Boolean whenSuccessful && whenSuccessful)
            {

                foreach(var act in this.Actions)
                {
                    act.Execute(cdssContext);
                }
                return true;
            }
            return false;

        }
    }
}