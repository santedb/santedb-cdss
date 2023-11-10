﻿using Newtonsoft.Json;
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
            using(CdssExecutionStackFrame.EnterChildFrame(this))
            {
                if(CdssExecutionStackFrame.Current.Context.TryGetRule(this.RuleName, out var rule))
                {
                    rule.Compute();
                }
                else
                {
                    throw new CdssEvaluationException(string.Format(ErrorMessages.OBJECT_NOT_FOUND, this.RuleName));
                }
            }
        }

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if(string.IsNullOrEmpty(this.RuleName))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.rule.referenceRequired", "Rule reference @ref attribute must be present", Guid.Empty, this.ToString());
            }
            else if(!context.TryGetRule(this.RuleName, out _))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.rule.referenceNotFound", $"Reference to {this.RuleName} not found", Guid.Empty, this.ToString());
            }
            foreach(var itm in base.Validate(context))
            {
                yield return itm;
            }
        }
    }
}