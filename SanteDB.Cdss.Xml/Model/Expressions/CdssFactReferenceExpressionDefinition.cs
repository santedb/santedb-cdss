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
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Cdss;
using SanteDB.Core.i18n;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Expressions
{
    /// <summary>
    /// CDSS fact reference 
    /// </summary>
    [XmlType(nameof(CdssFactReferenceExpressionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssFactReferenceExpressionDefinition : CdssExpressionDefinition
    {

        /// <summary>
        /// Serializer CTOR
        /// </summary>
        public CdssFactReferenceExpressionDefinition()
        {

        }

        /// <summary>
        /// Create new reference to <paramref name="factName"/>
        /// </summary>
        public CdssFactReferenceExpressionDefinition(String factName)
        {
            this.FactName = factName;
        }

        /// <summary>
        /// Reference to the fact name
        /// </summary>
        [XmlAttribute("ref"), JsonProperty("ref")]
        public String FactName { get; set; }


        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if (String.IsNullOrEmpty(this.FactName))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.expression.fact.missingReference", "Fact reference expressions require a @ref attribute", Guid.Empty, this.ToReferenceString());
            }
            else if (!context.FactNames.Contains(this.FactName.ToLowerInvariant()))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.expression.fact.notFound", $"Could not find a fact in scope named {this.FactName}", Guid.Empty, this.ToReferenceString());
            }
        }

        /// <summary>
        /// Generate the expression for the lookup of the fact
        /// </summary>
        internal override Expression GenerateComputableExpression(CdssExecutionContext cdssContext, params ParameterExpression[] parameters)
        {
            // We want to create an equivalent expression as : context[factName] call 
            var contextParameter = parameters.First(o => o.Name == CdssConstants.ContextVariableName || typeof(ICdssExecutionContext).IsAssignableFrom(o.Type));

            // Determine the type of fact
            Expression retVal = Expression.Call(contextParameter, typeof(CdssExecutionContext).GetMethod(nameof(CdssExecutionContext.GetFact)), Expression.Constant(this.FactName));
            if (cdssContext.TryGetFactDefinition(this.FactName, out var definition) && definition.ValueTypeSpecified)
            {
                switch (definition.ValueType)
                {
                    case CdssValueType.Boolean:
                        retVal = Expression.Convert(retVal, typeof(bool));
                        break;
                    case CdssValueType.Date:
                        retVal = Expression.Convert(retVal, typeof(DateTime));
                        break;
                    case CdssValueType.Integer:
                        retVal = Expression.Convert(retVal, typeof(int));
                        break;
                    case CdssValueType.Long:
                        retVal = Expression.Convert(retVal, typeof(long));
                        break;
                    case CdssValueType.Real:
                        retVal = Expression.Convert(retVal, typeof(double));
                        break;
                    case CdssValueType.String:
                        retVal = Expression.Convert(retVal, typeof(string));
                        break;
                }
            }
            else if (cdssContext.TryGetFact(this.FactName, out var fact))
            {
                if (fact != null)
                {
                    retVal = Expression.Convert(retVal, fact.GetType());
                }

            }
            else
            {
                retVal = Expression.Constant(cdssContext.GetValue(this.FactName)) ?? throw new CdssEvaluationException(String.Format(ErrorMessages.REFERENCE_NOT_FOUND, this.FactName));
            }
            return retVal;
        }
    }
}
