using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Core.BusinessRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Actions
{

    /// <summary>
    /// Represents a collection of actions
    /// </summary>
    [XmlType(nameof(CdssActionCollectionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssActionCollectionDefinition : CdssBaseObjectDefinition
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public CdssActionCollectionDefinition()
        {
            this.Actions = new List<CdssActionDefinition>();
        }

        /// <summary>
        /// Gets or sets the defined actions
        /// </summary>
        [XmlElement("propose", typeof(CdssProposeActionDefinition)),
            XmlElement("assign", typeof(CdssPropertyAssignActionDefinition)),
            XmlElement("raise", typeof(CdssIssueActionDefinition)),
            XmlElement("repeat", typeof(CdssRepeatActionDefinition)),
            XmlElement("apply", typeof(CdssRuleReferenceActionDefinition)),
            XmlElement("rule", typeof(CdssInlineRuleActionDefinition)),
            JsonProperty("action")]
        public List<CdssActionDefinition> Actions { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if(this.Actions?.Any() != true)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.actionCollection.actionsMissing", "CDSS Action collection must carry at least one action", Guid.Empty, this.ToReferenceString());
            }
            else
            {
                foreach(var itm in this.Actions.SelectMany(o=>o.Validate(context)))
                {
                    yield return itm;
                }
            }
        }

        /// <inheritdoc/>
        internal void Execute()
        {
            using (CdssExecutionStackFrame.EnterChildFrame(this))
            {
                try
                {
                    foreach (var stmt in this.Actions)
                    {
                        stmt.Execute();
                    }
                }
                catch (Exception e) when (!(e is CdssEvaluationException))
                {
                    throw new CdssEvaluationException($"Error computing {this.Name ?? this.Id}", e);
                }
            }
        }

    }
}