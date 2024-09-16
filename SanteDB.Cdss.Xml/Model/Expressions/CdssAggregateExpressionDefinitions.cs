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
using SanteDB.Core.BusinessRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Expressions
{
    /// <summary>
    /// CDSS composite expression definition for ALL/ANY
    /// </summary>
    [XmlType(nameof(CdssAggregateExpressionDefinition), Namespace = "http://santedb.org/cdss")]
    public abstract class CdssAggregateExpressionDefinition : CdssExpressionDefinition
    {
        private readonly ExpressionType m_operator;

        /// <summary>
        /// Expressions which this aggregate is made up of
        /// </summary>
        [XmlElement("csharp", typeof(CdssCsharpExpressionDefinition)),
            XmlElement("hdsi", typeof(CdssHdsiExpressionDefinition)),
            XmlElement("xml", typeof(CdssXmlLinqExpressionDefinition)),
            XmlElement("fact", typeof(CdssFactReferenceExpressionDefinition)),
            XmlElement("all", typeof(CdssAllExpressionDefinition)),
            XmlElement("none", typeof(CdssNoneExpressionDefinition)),
            XmlElement("any", typeof(CdssAnyExpressionDefinition)),
            JsonProperty("expressions")]
        public List<CdssExpressionDefinition> ContainedExpressions { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if (this.ContainedExpressions?.Any() != true)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.expression.aggregate.missingInstructions", "<any> or <all> missing contained expressions", Guid.Empty, this.ToReferenceString());
            }
            foreach (var itm in this.ContainedExpressions?.SelectMany(o => o.Validate(context)))
            {
                itm.RefersTo = itm.RefersTo ?? this.ToReferenceString();
                yield return itm;
            }
        }

        /// <summary>
        /// Constructor setting the operator type
        /// </summary>
        /// <param name="binaryOperator">The binary operator to apply to each item in the aggregate</param>
        protected CdssAggregateExpressionDefinition(ExpressionType binaryOperator)
        {
            this.m_operator = binaryOperator;
            this.ContainedExpressions = new List<CdssExpressionDefinition>();
        }

        /// <summary>
        /// Creates a new CDSS aggreate expression with the specified <paramref name="containedExpressions"/>
        /// </summary>
        protected CdssAggregateExpressionDefinition(ExpressionType binaryOperator, params CdssExpressionDefinition[] containedExpressions) : this(binaryOperator)
        {
            this.ContainedExpressions = containedExpressions.ToList();
        }

        /// <summary>
        /// Allows implementers to modify any of the contained expressions
        /// </summary>
        protected virtual Expression ModifyContainedExpression(Expression computedContainedExpression) => computedContainedExpression;

        /// <inheritdoc/>
        internal override Expression GenerateComputableExpression(CdssExecutionContext cdssContext, params ParameterExpression[] parameters)
        {
            Expression currentBody = null;
            foreach (var itm in this.ContainedExpressions)
            {
                var clause = itm.GenerateComputableExpression(cdssContext, parameters);

                // Is the clause not returning bool? If so then not null is the conversion
                if (clause.Type != typeof(bool))
                {
                    clause = Expression.MakeBinary(ExpressionType.NotEqual, clause, Expression.Constant(null));
                }
                clause = this.ModifyContainedExpression(clause);

                if (currentBody == null)
                {
                    currentBody = clause;
                }
                else
                {
                    currentBody = Expression.MakeBinary(this.m_operator, currentBody, clause);
                }
            }
            return currentBody;
        }
    }

    /// <summary>
    /// Or-else-expression
    /// </summary>
    [XmlType(nameof(CdssAnyExpressionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssAnyExpressionDefinition : CdssAggregateExpressionDefinition
    {
        /// <inheritdoc/>
        public CdssAnyExpressionDefinition() : base(ExpressionType.OrElse)
        {
        }

        /// <inheritdoc/>
        public CdssAnyExpressionDefinition(params CdssExpressionDefinition[] contents) : base(ExpressionType.OrElse, contents)
        {
        }

    }


    /// <summary>
    /// Not any (all of the contents must be false)
    /// </summary>
    [XmlType(nameof(CdssNoneExpressionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssNoneExpressionDefinition : CdssAggregateExpressionDefinition
    {
        /// <inheritdoc/>
        public CdssNoneExpressionDefinition() : base(ExpressionType.AndAlso)
        {
        }

        /// <inheritdoc/>
        public CdssNoneExpressionDefinition(params CdssExpressionDefinition[] contents) : base(ExpressionType.OrElse, contents)
        {
        }

        /// <inheritdoc/>
        protected override Expression ModifyContainedExpression(Expression computedContainedExpression) => Expression.Not(computedContainedExpression);
    }

    /// <summary>
    /// And expression aggregate
    /// </summary>
    [XmlType(nameof(CdssAllExpressionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssAllExpressionDefinition : CdssAggregateExpressionDefinition
    {
        /// <inheritdoc/>
        public CdssAllExpressionDefinition() : base(ExpressionType.AndAlso)
        {
        }

        /// <inheritdoc/>
        public CdssAllExpressionDefinition(params CdssExpressionDefinition[] contents) : base(ExpressionType.AndAlso, contents)
        {
        }
    }
}
