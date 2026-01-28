/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2024-6-21
 */
using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Cdss.Xml.Model.Expressions;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.i18n;
using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Actions
{
    /// <summary>
    /// Represents an action that assigns a property value
    /// </summary>
    [XmlType(nameof(CdssPropertyAssignActionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssPropertyAssignActionDefinition : CdssActionDefinition
    {

        // The compiled expression
        private Func<object, object, object> m_compiledExpression;

        /// <summary>
        /// View of the compiled expression source
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public String DebugView { get; private set; }

        /// <summary>
        /// Gets or sets the name of the property to assign
        /// </summary>
        [XmlAttribute("path"), JsonProperty("path")]
        public String Path { get; set; }

        /// <summary>
        /// Overwrite the property
        /// </summary>
        [XmlAttribute("overwrite"), JsonProperty("overwrite")]
        public bool OverwriteValue { get; set; }

        /// <summary>
        /// Expressions which this aggregate is made up of
        /// </summary>
        [XmlElement("csharp", typeof(CdssCsharpExpressionDefinition)),
            XmlElement("hdsi", typeof(CdssHdsiExpressionDefinition)),
            XmlElement("query", typeof(CdssQueryExpressionDefinition)),
            XmlElement("fact", typeof(CdssFactReferenceExpressionDefinition)),
            XmlElement("fixed", typeof(String)), JsonProperty("expression")]
        public Object ContainedExpression { get; set; }

        /// <summary>
        /// Gets or sets the target fact where this assignment should occur
        /// </summary>
        [XmlAttribute("target"), JsonProperty("target")]
        public string TargetFact { get; set; }


        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if (this.ContainedExpression == null)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.assign.property", "Assign action requires a setter expression", Guid.Empty, this.ToReferenceString());
            }
            if (String.IsNullOrEmpty(this.Path))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.assign.path", "Assign action requires a path expression", Guid.Empty, this.ToReferenceString());
            }
            foreach (var itm in base.Validate(context).Union((this.ContainedExpression as CdssExpressionDefinition)?.Validate(context) ?? new DetectedIssue[0]))
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
                    var targetObject = CdssExecutionStackFrame.Current.ScopedObject;
                    if(!String.IsNullOrEmpty(this.TargetFact))
                    {
                        if(!CdssExecutionStackFrame.Current.Context.TryGetFact(this.TargetFact, out var factValueRaw))
                        {
                            throw new KeyNotFoundException(String.Format(ErrorMessages.OBJECT_NOT_FOUND, this.TargetFact));
                        }
                        targetObject = (IdentifiedData)factValueRaw;
                    }

                    switch (this.ContainedExpression)
                    {
                        case CdssExpressionDefinition exe:
                            if (this.m_compiledExpression == null)
                            {
                                var uncompiledExpression = exe.GenerateComputableExpression(this.LogicBlock?.Context?.Type);
#if DEBUG
                                this.DebugView = uncompiledExpression.ToString();
#endif
                                this.m_compiledExpression = uncompiledExpression.Compile();
                            }
                            var value = this.m_compiledExpression(CdssExecutionStackFrame.Current.Context, CdssExecutionStackFrame.Current.ScopedObject);
                            CdssExecutionStackFrame.Current.Context.DebugSession?.CurrentFrame.AddAssignment(this.Path, value);
                            targetObject.GetOrSetValueAtPath(this.Path, value, this.OverwriteValue);
                            break;
                        case String str:
                            targetObject.GetOrSetValueAtPath(this.Path, str, this.OverwriteValue);
                            break;
                        default:
                            throw new InvalidOperationException();
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