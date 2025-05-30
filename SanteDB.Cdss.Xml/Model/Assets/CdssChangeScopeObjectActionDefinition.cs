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
using SanteDB.Cdss.Xml.Model.Actions;
using SanteDB.Cdss.Xml.Model.Expressions;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Assets
{
    /// <summary>
    /// CDSS change of "ScopedObject" in the CDSS emitting process
    /// </summary>
    [XmlType(nameof(CdssChangeScopeObjectActionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssChangeScopeObjectActionDefinition : CdssActionDefinition
    {

        // The expression which has been calculated
        private Func<object, object, object> m_compiledExpression;

        /// <summary>
        /// Debug view
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public string DebugView { get; private set; }

        /// <summary>
        /// Gets the expression of the fact
        /// </summary>
        [XmlElement("csharp", typeof(CdssCsharpExpressionDefinition)),
         XmlElement("hdsi", typeof(CdssHdsiExpressionDefinition)),
         XmlElement("fact", typeof(CdssFactReferenceExpressionDefinition)),
         XmlElement("query", typeof(CdssQueryExpressionDefinition)),
         JsonProperty("logic")]
        public CdssExpressionDefinition ScopeComputation { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if (this.ScopeComputation == null)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.scopeChange.computationMissing", "Scoped object change requires a computation (one of hdsi, query, fact, or csharp)", Guid.Empty, this.ToReferenceString());
            }
            else
            {
                foreach (var itm in base.Validate(context).Union(this.ScopeComputation.Validate(context)))
                {
                    itm.RefersTo = itm.RefersTo ?? this.ToReferenceString();
                    yield return itm;
                }
            }
        }

        /// <inheritdoc/>
        internal override void Execute()
        {
            if (this.m_compiledExpression == null)
            {

                var uncompiledExpression = this.ScopeComputation.GenerateComputableExpression(this.LogicBlock?.Context?.Type);
                this.DebugView = uncompiledExpression.ToString();
                this.m_compiledExpression = uncompiledExpression.Compile();
            }

            object valueToSet = null;
            using (CdssExecutionStackFrame.EnterChildFrame(this))
            {
                valueToSet = m_compiledExpression(CdssExecutionStackFrame.Current.Context, CdssExecutionStackFrame.Current.ScopedObject);
            }
            CdssExecutionStackFrame.Current.ScopedObject = valueToSet as IdentifiedData;
        }
    }
}