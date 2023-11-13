using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Cdss.Xml.Model.Actions;
using SanteDB.Core.BusinessRules;
using System;
using System.Collections.Generic;
using System.Linq;
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
            XmlArrayItem("assign", typeof(CdssPropertyAssignActionDefinition)),
            XmlArrayItem("raise", typeof(CdssIssueActionDefinition)),
            XmlArrayItem("repeat", typeof(CdssRepeatActionDefinition)),
            XmlArrayItem("apply", typeof(CdssRuleReferenceActionDefinition)),
            JsonProperty("then")]
        public List<CdssActionDefinition> Actions { get; set; }


        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if (this.When == null)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Warning, "cdss.rule.whenRecommended", "Rules should carry a WHEN condition unless they are globally applied", Guid.Empty, this.ToString());
            }
            if (this.Actions?.Any() != true)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.rule.thenRequired", "Rules must carry a THEN block", Guid.Empty, this.ToString());
            }
            foreach(var itm in base.Validate(context)
                .Union(this.Actions?.SelectMany(o=>o.Validate(context) ?? new DetectedIssue[0]))
                .Union(this.When?.Validate(context) ?? new DetectedIssue[0]))
            {
                itm.RefersTo = itm.RefersTo ?? this.ToString();
                yield return itm;
            }
        }

        /// <summary>
        /// Compute the rule and execute any actions in the rule
        /// </summary>
        /// <returns>True if the rule was executed, false if it was not executed</returns>
        public override object Compute()
        {
            base.ThrowIfInvalidState();

            using (CdssExecutionStackFrame.EnterChildFrame(this))
            {
                try
                {
                    if (this.When == null || this.When.Compute() is bool b && b)
                    {
                        foreach (var act in this.Actions)
                        {
                            act.Execute();
                        }
                        return true;
                    }

                    return null;
                }
                catch (Exception e) when (!(e is CdssEvaluationException))
                {
                    throw new CdssEvaluationException($"Error computing {this.Name ?? this.Id}", e);
                }
            }

        }
    }
}