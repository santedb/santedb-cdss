using Newtonsoft.Json;
using SanteDB.Cdss.Xml.XmlLinq;
using SanteDB.Core.Cdss;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Expressions
{
    /// <summary>
    /// XML based LINQ expression
    /// </summary>
    public class CdssXmlLinqExpressionDefinition : CdssExpressionDefinition
    {

        /// <summary>
        /// Gets or sets the XML Linq expression
        /// </summary>
        [XmlElement("linq"), JsonProperty("linq")]
        public XmlLambdaExpression ExpressionDefinition { get; set; }

        /// <inheritdoc/
        internal override Expression GenerateComputableExpression(CdssContext cdssContext, params ParameterExpression[] parameters)
        {
            this.ExpressionDefinition.InitializeContext(null);
            return Expression.Invoke(this.ExpressionDefinition.ToExpression(), parameters);
        }
    }
}
