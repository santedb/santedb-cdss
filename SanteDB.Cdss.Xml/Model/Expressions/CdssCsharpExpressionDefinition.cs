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
using DynamicExpresso;
using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Expressions
{

    /// <summary>
    /// C# based expression
    /// </summary>
    [XmlType(nameof(CdssCsharpExpressionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssCsharpExpressionDefinition : CdssExpressionDefinition
    {

        /// <summary>
        /// Defualt ctor
        /// </summary>
        public CdssCsharpExpressionDefinition()
        {

        }

        /// <summary>
        /// Create a new CDSS C# expression based on <paramref name="expression"/>
        /// </summary>
        public CdssCsharpExpressionDefinition(String expression)
        {
            this.ExpressionValue = expression;
        }


        /// <summary>
        /// Expression value
        /// </summary>
        [XmlText, JsonProperty("expression")]
        public String ExpressionValue { get; set; }

        /// <inheritdoc/>
        internal override Expression GenerateComputableExpression(CdssExecutionContext cdssContext, params ParameterExpression[] parameters)
        {
            try
            {

                return cdssContext.GetExpressionInterpreter().Parse(this.ExpressionValue, parameters.Select(o => new Parameter(o)).ToArray()).Expression;
            }
            catch (Exception e)
            {
                throw new CdssEvaluationException($"{e.Message} - \"{this.ExpressionValue}\"", e);
            }
        }

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if (String.IsNullOrEmpty(this.ExpressionValue))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.expression.csharp.missingLogic", "C# expression logic missing", Guid.Empty);
            }
            else
            {
                var identifiers = context.GetExpressionInterpreter()
                  .SetVariable("context", null, typeof(CdssExecutionContext))
                  .SetVariable("value", null, typeof(object))
                  .SetVariable("scopedObject", null, typeof(IdentifiedData))
                  .DetectIdentifiers(this.ExpressionValue);

                if (identifiers.UnknownIdentifiers.Any())
                {
                    yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.expression.csharp.unknownId", $"Unknown identifiers {String.Join(",", identifiers.UnknownIdentifiers.Select(o => o.ToString()))} in C# expression {this.ExpressionValue}", Guid.Empty, this.ToReferenceString());
                }
                else if (this.ExpressionValue.Count(o => o == '[') != this.ExpressionValue.Count(o => o == ']'))
                {
                    yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.expression.csharp.bracketMismatch", $"Missing indexer close/open bracket in {this.ExpressionValue}", Guid.Empty, this.ToReferenceString());
                }
                else if (this.ExpressionValue.Count(o => o == '(') != this.ExpressionValue.Count(o => o == ')'))
                {
                    yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.expression.csharp.bracketMismatch", $"Missing parentheses close/open in {this.ExpressionValue}", Guid.Empty, this.ToReferenceString());
                }
            }
        }
    }
}
