using Newtonsoft.Json;
using SanteDB.Cdss.Xml.XmlLinq;
using SanteDB.Core.Cdss;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
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
