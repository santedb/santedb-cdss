using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.i18n;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Actions
{

    /// <summary>
    /// Allows for the execution of another rule or fact
    /// </summary>
    [XmlType(nameof(CdssRuleReferenceActionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssRuleReferenceActionDefinition : CdssActionDefinition
    {

        public CdssRuleReferenceActionDefinition()
        {
            
        }

        /// <summary>
        /// Creates a new rule reference action
        /// </summary>
        public CdssRuleReferenceActionDefinition(string ruleName)
        {
            this.RuleName = ruleName;
        }

        /// <summary>
        /// Gets the rule 
        /// </summary>
        [XmlAttribute("ref"), JsonProperty("ref")]
        public string RuleName { get; set; }

        /// <summary>
        /// Execute the specified action
        /// </summary>
        internal override void Execute()
        {
            using (CdssExecutionStackFrame.EnterChildFrame(this))
            {
                try
                {
                    if (CdssExecutionStackFrame.Current.Context.TryGetRuleDefinition(this.RuleName, out var rule))
                    {
                        rule.Compute();
                    }
                    else
                    {
                        throw new CdssEvaluationException(string.Format(ErrorMessages.OBJECT_NOT_FOUND, this.RuleName));
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
            if (string.IsNullOrEmpty(this.RuleName))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.rule.referenceRequired", "Rule reference @ref attribute must be present", Guid.Empty, this.ToReferenceString());
            }
            else if (!context.TryGetRuleDefinition(this.RuleName, out _))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.rule.referenceNotFound", $"Reference to {this.RuleName} not found", Guid.Empty, this.ToReferenceString());
            }
            foreach (var itm in base.Validate(context))
            {
                itm.RefersTo = itm.RefersTo ?? this.ToReferenceString();
                yield return itm;
            }
        }
    }
}