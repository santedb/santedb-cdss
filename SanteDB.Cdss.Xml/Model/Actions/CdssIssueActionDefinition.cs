﻿/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: fyfej
 * Date: 2023-11-27
 */
using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Core.BusinessRules;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Actions
{
    /// <summary>
    /// Raise an issue add it to the analysis
    /// </summary>
    [XmlType(nameof(CdssIssueActionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssIssueActionDefinition : CdssActionDefinition
    {

        /// <summary>
        /// The issue to raise
        /// </summary>
        [XmlElement("issue", Namespace = "http://santedb.org/issue"), JsonProperty("issue")]
        public DetectedIssue IssueToRaise { get; set; }

        /// <inheritdoc/>
        internal override void Execute()
        {
            base.ThrowIfInvalidState();

            using (CdssExecutionStackFrame.EnterChildFrame(this))
            {
                try
                {
                    var issue = new DetectedIssue(this.IssueToRaise.Priority, this.IssueToRaise.Id, this.IssueToRaise.Text, this.IssueToRaise.TypeKey, CdssExecutionStackFrame.Current.ScopedObject.ToString());
                    CdssExecutionStackFrame.Current.Context.PushIssue(this.IssueToRaise);
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
            if (this.IssueToRaise == null)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.raise.issue", "Raise action requires a detected issue", Guid.Empty, this.ToReferenceString());
            }
            foreach (var itm in base.Validate(context))
            {
                itm.RefersTo = itm.RefersTo ?? this.ToString();
                yield return itm;
            }
        }
    }
}