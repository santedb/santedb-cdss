﻿/*
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
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Cdss.Xml.Model.Expressions;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Assets
{
    /// <summary>
    /// Represents an instruction to emit/normalize 
    /// </summary>
    [XmlType(nameof(CdssFactNormalizationDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssFactNormalizationDefinition : CdssBaseObjectDefinition
    {

        // The expression which has been calculated
        private Func<object, object, object, object> m_compiledExpression;

        /// <summary>
        /// Represents the "when" clause for the rule
        /// </summary>
        [XmlElement("when"), JsonProperty("when")]
        public CdssWhenDefinition When { get; set; }

        /// <summary>
        /// Debug view of the normalization
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public String DebugView { get; private set; }

        /// <summary>
        /// Expression to emit the data in the proper norm
        /// </summary>
        [XmlElement("csharp", typeof(CdssCsharpExpressionDefinition)),
          XmlElement("hdsi", typeof(CdssHdsiExpressionDefinition)),
          XmlElement("xml", typeof(CdssXmlLinqExpressionDefinition)),
          XmlElement("all", typeof(CdssAllExpressionDefinition)),
            XmlElement("none", typeof(CdssNoneExpressionDefinition)),
          XmlElement("any", typeof(CdssAnyExpressionDefinition)),
          JsonProperty("logic")]
        public CdssExpressionDefinition EmitExpression { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if (this.When == null)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Warning, "cdss.fact.normalize.when", "Normalization instructions should carry a when condition", Guid.Empty, this.ToReferenceString());
            }
            else
            {
                foreach(var itm in this.When.Validate(context))
                {
                    yield return itm;
                }
            }
            if (this.EmitExpression == null)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.fact.normalize.emit", "Normalization instructions must carry a computation", Guid.Empty, this.ToReferenceString());
            }
            else
            {
                foreach(var itm in this.EmitExpression.Validate(context))
                {
                    yield return itm;
                }
            }
            
        }

        /// <summary>
        /// Transform the object, return null if the transfomration is not successful
        /// </summary>
        internal object TransformObject(object retVal)
        {
            using (CdssExecutionStackFrame.EnterChildFrame(this))
            {
                try
                {
                    if (this.When == null || this.When.Compute() is bool b && b)
                    {
                        if (this.m_compiledExpression == null)
                        {
                            var contextParameter = Expression.Parameter(CdssExecutionStackFrame.Current.Context.GetType(), CdssConstants.ContextVariableName);
                            var scopedParameter = Expression.Parameter(typeof(IdentifiedData), CdssConstants.ScopedObjectVariableName);
                            var valueParameter = Expression.Parameter(retVal.GetType(), CdssConstants.ValueVariableName);

                            var expressionForValue = this.EmitExpression.GenerateComputableExpression(CdssExecutionStackFrame.Current.Context, contextParameter, scopedParameter, valueParameter);
                            if (!(expressionForValue is LambdaExpression))
                            {
                                expressionForValue = Expression.Lambda(expressionForValue, contextParameter, scopedParameter, valueParameter);
                            }

                            // Convert object parameters for our FUNC
                            var contextObjParameter = Expression.Parameter(typeof(Object));
                            var valueObjParameter = Expression.Parameter(typeof(Object));
                            var scopeObjParameter = Expression.Parameter(typeof(Object));
                            expressionForValue = Expression.Convert(Expression.Invoke(
                                    expressionForValue,
                                    Expression.Convert(contextObjParameter, contextParameter.Type),
                                    Expression.Convert(scopeObjParameter, scopedParameter.Type),
                                    Expression.Convert(valueObjParameter, valueParameter.Type)
                                ), typeof(Object));

                            var uncompiledExpression = Expression.Lambda<Func<object, object, object, object>>(
                                expressionForValue,
                                contextObjParameter,
                                scopeObjParameter,
                                valueObjParameter
                            );
                            this.DebugView = uncompiledExpression.ToString();
                            this.m_compiledExpression = uncompiledExpression.Compile();
                        }

                        return this.m_compiledExpression(CdssExecutionStackFrame.Current.Context, CdssExecutionStackFrame.Current.ScopedObject, retVal);
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