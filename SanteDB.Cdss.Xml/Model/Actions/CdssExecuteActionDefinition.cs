using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Cdss.Xml.Model.Assets;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.i18n;
using System;
using System.Collections.Generic;
using System.Linq;
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
        [XmlElement("rule", typeof(CdssRuleReferenceActionDefinition)),
            XmlElement("assign", typeof(CdssPropertyAssignActionDefinition)),
            XmlElement("propose", typeof(CdssProposeActionDefinition)),
            XmlElement("raise", typeof(CdssIssueActionDefinition)),
            JsonProperty("actions")]
        public List<CdssActionDefinition> Statements { get; set; }

        /// <inheritdoc/>
        internal override void Execute()
        {
            base.ThrowIfInvalidState();
            using (CdssExecutionStackFrame.EnterChildFrame(this))
            {
                try
                {
                    foreach (var stmt in this.Statements)
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

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if (this.Statements?.Any() != true)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.execute.statement", "Execute block must carry at least one instruction", Guid.Empty, this.ToString());
            }
            foreach (var itm in base.Validate(context).Union(this.Statements?.SelectMany(o=>o.Validate(context)) ?? new DetectedIssue[0]))
            {
                itm.RefersTo = itm.RefersTo ?? this.ToString();
                yield return itm;
            }
        }
    }
}