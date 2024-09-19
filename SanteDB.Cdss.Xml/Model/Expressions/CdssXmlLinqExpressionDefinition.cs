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
using SanteDB.Cdss.Xml.XmlLinq;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Cdss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Expressions
{
    /// <summary>
    /// XML based LINQ expression
    /// </summary>
    [XmlType(nameof(CdssXmlLinqExpressionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssXmlLinqExpressionDefinition : CdssExpressionDefinition
    {

        /// <summary>
        /// Default ctor
        /// </summary>
        public CdssXmlLinqExpressionDefinition()
        {

        }

        /// <summary>
        /// Create a new XML LINQ expression from the <paramref name="expression"/>
        /// </summary>
        public CdssXmlLinqExpressionDefinition(Expression expression)
        {
            this.ExpressionDefinition = XmlLambdaExpression.FromExpression(expression);
        }

        /// <summary>
        /// Gets or sets the XML Linq expression
        /// </summary>
        [XmlElement("linq"), JsonProperty("linq")]
        public XmlExpression ExpressionDefinition { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if (this.ExpressionDefinition == null)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.expression.xml.missingExpression", "XML LINQ expression statement required LINQ expression to be present", Guid.Empty, this.ToReferenceString());
            }

        }

        /// <summary>
        /// Gets or sets where the object should be pulled from
        /// </summary>
        [XmlAttribute("scope"), JsonProperty("scope")]
        public CdssHdsiExpressionScopeType Scope { get; set; }

        /// <inheritdoc/>
        internal override Expression GenerateComputableExpression(CdssExecutionContext cdssContext, params ParameterExpression[] parameters)
        {
            this.ExpressionDefinition.InitializeContext(null);

            var bodyExpression = this.ExpressionDefinition.ToExpression();
            Expression scopedObjectExpression = null;
            switch (this.Scope)
            {
                case CdssHdsiExpressionScopeType.Context:
                    scopedObjectExpression = Expression.MakeMemberAccess(parameters.First(o => o.Name == CdssConstants.ContextVariableName), (MemberInfo)cdssContext.GetType().GetProperty(nameof(ICdssExecutionContext.Target)));
                    break;
                default:
                    scopedObjectExpression = parameters.First(o => o.Name == CdssConstants.ScopedObjectVariableName);
                    break;

            }
            return Expression.Invoke(bodyExpression, scopedObjectExpression);
        }
    }
}
