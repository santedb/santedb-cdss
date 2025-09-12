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
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Cdss.Xml.Model.Expressions;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.i18n;
using SanteDB.Core.Model.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Assets
{
    /// <summary>
    /// Represents an expression 
    /// </summary>
    [XmlType(nameof(CdssFactAssetDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssFactAssetDefinition : CdssComputableAssetDefinition
    {

        // The expression which has been calculated
        private Func<object, object, object> m_compiledExpression;

        /// <summary>
        /// Gets the expression of the fact
        /// </summary>
        [XmlElement("csharp", typeof(CdssCsharpExpressionDefinition)),
            XmlElement("hdsi", typeof(CdssHdsiExpressionDefinition)),
            XmlElement("query", typeof(CdssQueryExpressionDefinition)),
            XmlElement("xml", typeof(CdssXmlLinqExpressionDefinition)),
            XmlElement("all", typeof(CdssAllExpressionDefinition)),
            XmlElement("none", typeof(CdssNoneExpressionDefinition)),
            XmlElement("any", typeof(CdssAnyExpressionDefinition)),
            JsonProperty("logic")]
        public CdssExpressionDefinition FactComputation { get; set; }

        /// <summary>
        /// Normalize the datain the computation
        /// </summary>
        [XmlElement("normalize"), JsonProperty("normalize")]
        public List<CdssFactNormalizationDefinition> Normalize { get; set; }

        /// <summary>
        /// Invert the result
        /// </summary>
        [XmlAttribute("negate"), JsonProperty("negate")]
        public bool IsNegated { get; set; }

        /// <summary>
        /// Gets the type that the value should take on
        /// </summary>
        [XmlAttribute("type"), JsonProperty("type")]
        public CdssValueType ValueType { get; set; }

        /// <summary>
        /// True if <see cref="ValueType"/> has been specified
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public bool ValueTypeSpecified { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if (this.FactComputation == null)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.fact.definition", "Fact assets must have a definition in csharp, hdsi, xml", Guid.Empty, this.ToReferenceString());
            }
            foreach (var itm in base.Validate(context)
                .Union(this.FactComputation?.Validate(context) ?? new DetectedIssue[0])
                .Union(this.Normalize?.SelectMany(o => o.Validate(context)) ?? new DetectedIssue[0]))
            {
                itm.RefersTo = itm.RefersTo ?? this.ToReferenceString();
                yield return itm;
            }
        }

        /// <inheritdoc/>
        public override object Compute()
        {
            base.ThrowIfInvalidState();


            if (this.m_compiledExpression == null)
            {
                var uncompiledExpression = this.FactComputation.GenerateComputableExpression(this.LogicBlock?.Context?.Type);

                if (this.IsNegated)
                {
                    uncompiledExpression = Expression.Lambda<Func<object, object, object>>(Expression.Convert(Expression.Not(Expression.Convert(uncompiledExpression.Body, typeof(bool))), typeof(object)), uncompiledExpression.Parameters);
                }
#if DEBUG
                this.DebugView = uncompiledExpression.ToString();
#endif
                this.m_compiledExpression = uncompiledExpression.Compile();
            }

            using (CdssExecutionStackFrame.EnterChildFrame(this))
            {
                try
                {
                    var retVal = m_compiledExpression(CdssExecutionStackFrame.Current.Context, CdssExecutionStackFrame.Current.ScopedObject);
                    // Convert the value?
                    if (ValueTypeSpecified == true)
                    {
                        var netType = typeof(string);
                        switch (ValueType)
                        {
                            case CdssValueType.Boolean:
                                netType = typeof(bool);
                                break;
                            case CdssValueType.Date:
                                netType = typeof(DateTime);
                                break;
                            case CdssValueType.Integer:
                                netType = typeof(int);
                                break;
                            case CdssValueType.Long:
                                netType = typeof(long);
                                break;
                            case CdssValueType.Real:
                                netType = typeof(double);
                                break;
                        }

                        if (!MapUtil.TryConvert(retVal, netType, out var converted))
                        {
                            if (this.ValueType == CdssValueType.Boolean) // It is boolean but cannot be converted to boolean so we do a null check
                            {
                                retVal = retVal != null;
                            }
                            else
                            {
                                retVal = retVal != CdssConstants.GetDefaultValue(netType);
                            }
                        }
                        retVal = converted;
                    }

                    retVal = this.Normalize?.Select(o => o.TransformObject(retVal)).FirstOrDefault(o => o != null) ?? retVal;

                    return retVal;
                }
                catch (Exception e) when (!(e is CdssEvaluationException))
                {
                    throw new CdssEvaluationException($"Error computing {this.Name ?? this.Id}", e);
                }
            }
        }
    }
}