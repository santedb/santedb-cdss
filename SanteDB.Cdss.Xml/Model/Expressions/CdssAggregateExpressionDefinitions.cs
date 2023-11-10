using SanteDB.Cdss.Xml.Model.Assets;
using SanteDB.Core.BusinessRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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
            XmlElement("any", typeof(CdssAnyExpressionDefinition))]
        public List<CdssExpressionDefinition> ContainedExpressions { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if(this.ContainedExpressions?.Any() != true)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.expression.aggregate.missingInstructions", "<any> or <all> missing contained expressions", Guid.Empty);
            }
            foreach(var itm in this.ContainedExpressions?.SelectMany(o=>o.Validate(context)))
            {
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
        }

        /// <summary>
        /// Creates a new CDSS aggreate expression with the specified <paramref name="containedExpressions"/>
        /// </summary>
        protected CdssAggregateExpressionDefinition(ExpressionType binaryOperator, params CdssExpressionDefinition[] containedExpressions) : this(binaryOperator)
        {
            this.ContainedExpressions = containedExpressions.ToList();
        }

        /// <inheritdoc/>
        internal override Expression GenerateComputableExpression(CdssExecutionContext cdssContext, params ParameterExpression[] parameters)
        {
            Expression currentBody = null;
            foreach (var itm in this.ContainedExpressions)
            {
                var clause = Expression.Convert(itm.GenerateComputableExpression(cdssContext, parameters), typeof(bool));
                if(currentBody == null)
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
