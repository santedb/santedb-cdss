using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Expressions
{
    /// <summary>
    /// CDSS composite expression definition for ALL/ANY
    /// </summary>
    public abstract class CdssAggregateExpressionDefinition : CdssExpressionDefinition
    {
        private readonly ExpressionType m_operator;

        /// <summary>
        /// Expressions which this aggregate is made up of
        /// </summary>
        [XmlElement("csharp", typeof(CdssCsharpExpressionDefinition)),
            XmlElement("hdsi", typeof(CdssHdsiExpressionDefinition)),
            XmlElement("xml", typeof(CdssXmlLinqExpressionDefinition)),
            XmlElement("all", typeof(CdssAllExpressionDefinition)),
            XmlElement("any", typeof(CdssAnyExpressionDefinition))]
        public List<CdssExpressionDefinition> ContainedExpressions { get; set; }

        /// <summary>
        /// Constructor setting the operator type
        /// </summary>
        /// <param name="binaryOperator">The binary operator to apply to each item in the aggregate</param>
        protected CdssAggregateExpressionDefinition(ExpressionType binaryOperator)
        {
            this.m_operator = binaryOperator;
        }

        /// <inheritdoc/>
        internal override Expression GenerateComputableExpression(CdssContext cdssContext, params ParameterExpression[] parameters)
        {
            Expression currentBody = null;
            foreach (var itm in this.ContainedExpressions)
            {
                var clause = itm.GenerateComputableExpression(cdssContext, parameters);
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
    public class CdssAnyExpressionDefinition : CdssAggregateExpressionDefinition
    {
        /// <inheritdoc/>
        public CdssAnyExpressionDefinition() : base(ExpressionType.OrElse)
        {
        }
    }

    /// <summary>
    /// And expression aggregate
    /// </summary>
    public class CdssAllExpressionDefinition : CdssAggregateExpressionDefinition
    {
        /// <inheritdoc/>
        public CdssAllExpressionDefinition() : base(ExpressionType.AndAlso)
        {
            
        }
    }
}
