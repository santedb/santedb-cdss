/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Cdss.Xml.Model.Expressions;
using SanteDB.Core.BusinessRules;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// Defines a WHEN condition
    /// </summary>
    [XmlType(nameof(CdssWhenDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssWhenDefinition : CdssBaseObjectDefinition
    {

        /// <summary>
        /// When definition ctor
        /// </summary>
        public CdssWhenDefinition()
        {
            this.LogicBlock = CdssLibraryLoadContext.Current.FindLastLoaded<CdssDecisionLogicBlockDefinition>();
        }

        /// <summary>
        /// Gets the logic block that owns this 
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public CdssDecisionLogicBlockDefinition LogicBlock { get; }

        // The expression which has been calculated
        private Func<object, object, object> m_compiledExpression;

        /// <summary>
        /// Gets the expression of the fact
        /// </summary>
        [XmlElement("csharp", typeof(CdssCsharpExpressionDefinition)),
         XmlElement("hdsi", typeof(CdssHdsiExpressionDefinition)),
         XmlElement("xml", typeof(CdssXmlLinqExpressionDefinition)),
         XmlElement("query", typeof(CdssQueryExpressionDefinition)),
         XmlElement("all", typeof(CdssAllExpressionDefinition)),
         XmlElement("none", typeof(CdssNoneExpressionDefinition)),
         XmlElement("any", typeof(CdssAnyExpressionDefinition)),
         XmlElement("fact", typeof(CdssFactReferenceExpressionDefinition)),
         JsonProperty("logic")]
        public CdssExpressionDefinition WhenComputation { get; set; }

        /// <summary>
        /// Debug view
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public string DebugView { get; private set; }

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if (this.WhenComputation == null)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.when.definitionMissing", "When condition block is missing logic", Guid.Empty, this.ToReferenceString());
            }
            else
            {
                foreach (var itm in this.WhenComputation.Validate(context))
                {
                    itm.RefersTo = itm.RefersTo ?? this.ToString();
                    yield return itm;
                }
            }
        }

        /// <inheritdoc/>
        public bool Compute()
        {
            if (this.m_compiledExpression == null)
            {
                var uncompiledExpression = this.WhenComputation.GenerateComputableExpression(this.LogicBlock?.Context?.Type);

#if DEBUG
                this.DebugView = uncompiledExpression.ToString();
#endif 
                this.m_compiledExpression = uncompiledExpression.Compile();
            }

            using (CdssExecutionStackFrame.EnterChildFrame(this))
            {
                object result = null;
                try
                {
                    result = m_compiledExpression(CdssExecutionStackFrame.Current.Context, CdssExecutionStackFrame.Current.ScopedObject);
                }
                catch (NullReferenceException)
                {
                    CdssExecutionStackFrame.Current.Context.PushIssue(new DetectedIssue(DetectedIssuePriorityType.Warning, "warn.null", $"Fact {this.Name} could not be evaluated", Guid.Empty));
                }

                if (result is bool b)
                {
                    return b;
                }
                else
                {
                    return result != null;
                }
            }
        }
    }
}