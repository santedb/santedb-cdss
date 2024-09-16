/*
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
 */
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
    /// Represents a repetition of any action
    /// </summary>
    [XmlType(nameof(CdssRepeatActionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssRepeatActionDefinition : CdssActionDefinition, IHasCdssActions
    {

        /// <summary>
        /// The number of iterations to repeat
        /// </summary>
        [XmlAttribute("iterations"), JsonProperty("iterations")]
        public int Iterations { get; set; }

        /// <summary>
        /// Iterations are specified
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public bool IterationsSpecified { get; set; }

        /// <summary>
        /// The variable to track iterations
        /// </summary>
        [XmlAttribute("trackBy"), JsonProperty("trackBy")]
        public string IterationVariable { get; set; }

        /// <summary>
        /// Repeat until the fact provided is true
        /// </summary>
        [XmlElement("until"), JsonProperty("until")]
        public CdssWhenDefinition Until { get; set; }

        /// <summary>
        /// The objects which are to be executed in this repeat loop
        /// </summary>
        [XmlElement("execute"),
            JsonProperty("execute")]
        public CdssActionCollectionDefinition Actions { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if (!(this.IterationsSpecified || this.Iterations > 0) && this.Until == null)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.repeat.infinite", "Either @iterations or <until> are required, otherwise repeat action will be infinite", Guid.Empty, this.ToReferenceString());
            }
            if (this.Actions == null)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.execute.statement", "Execute block must carry at least one instruction", Guid.Empty, this.ToReferenceString());
            }
            foreach (var itm in base.Validate(context)
                .Union(this.Until?.Validate(context) ?? new DetectedIssue[0])
                .Union(this.Actions?.Validate(context) ?? new DetectedIssue[0]))
            {
                itm.RefersTo = itm.RefersTo ?? this.ToReferenceString();
                yield return itm;
            }

        }

        /// <inheritdoc/>
        internal override void Execute()
        {
            base.ThrowIfInvalidState();

            using (CdssExecutionStackFrame.EnterChildFrame(this))
            {
                try
                {
                    var iteration = 0;

                    while ((!this.IterationsSpecified) ^ (iteration < this.Iterations))
                    {

                        CdssExecutionStackFrame.Current.Context.SetValue(this.IterationVariable ?? "index", iteration);
                        this.Actions?.Execute();

                        // If there is an UNTIL clause evaluate it
                        var untilResult = this.Until?.Compute();
                        if (untilResult is Boolean b && b || untilResult != null)
                        {
                            break;
                        }
                        iteration++;
                    }

                    CdssExecutionStackFrame.Current.Context.DestroyValue(this.IterationVariable);
                }
                catch (Exception e) when (!(e is CdssEvaluationException))
                {
                    throw new CdssEvaluationException($"Error computing {this.Name ?? this.Id}", e);
                }
            }

        }
    }
}