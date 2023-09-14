using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Model.Expressions;
using SanteDB.Cdss.Xml.XmlLinq;
using SanteDB.Core.i18n;
using SanteDB.Core.Model.Query;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// Represents an expression 
    /// </summary>
    [XmlType(nameof(CdssFactAssetDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssFactAssetDefinition : CdssComputableAssetDefinition
    {

        // The expression which has been calculated
        private Func<Object, Object> m_compiledExpression;

        /// <summary>
        /// Gets the expression of the fact
        /// </summary>
        [XmlElement("csharp", typeof(CdssCsharpExpressionDefinition)),
            XmlElement("hdsi", typeof(CdssHdsiExpressionDefinition)),
            XmlElement("xml", typeof(CdssXmlLinqExpressionDefinition)),
            XmlElement("all", typeof(CdssAllExpressionDefinition)),
            XmlElement("any", typeof(CdssAnyExpressionDefinition))]
        public CdssExpressionDefinition FactComputation { get; set; }

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
        internal override object Compute<TContext>(CdssContext<TContext> cdssContext)
        {
            if(this.m_compiledExpression == null)
            {
                var contextParameter = Expression.Parameter(typeof(CdssContext<TContext>));
                Expression bodyExpression = Expression.Lambda<Func<CdssContext<TContext>, Object>>(this.FactComputation.GenerateComputableExpression(cdssContext, contextParameter), contextParameter);
               
                // Wrap the expression, compile and set to this value
                // We do this because calling this.m_compiledExpression(context) is faster than 
                // (bool)this.m_compiledExpression.DynamicInvoke(context);
                var objParam = Expression.Parameter(typeof(Object));
                bodyExpression = Expression.Invoke(
                        bodyExpression, Expression.Convert(objParam, typeof(CdssContext<TContext>))
                    );

                // Convert the value?
                if (this.ValueTypeSpecified == true)
                {
                    var netType = typeof(String);
                    switch(this.ValueType)
                    {
                        case CdssValueType.Boolean:
                            netType = typeof(Boolean);
                            break;
                        case CdssValueType.Date:
                            netType = typeof(DateTimeOffset);
                            break;
                        case CdssValueType.Integer:
                            netType = typeof(Int32);
                            break;
                        case CdssValueType.Real:
                            netType = typeof(double);
                            break;
                    }

                    if(netType != bodyExpression.Type)
                    {
                        bodyExpression = Expression.Convert(bodyExpression, netType);
                    }
                }
                
                if(this.IsNegated)
                {
                    bodyExpression = Expression.Not(bodyExpression);
                }

                var uncompiledExpression = Expression.Lambda<Func<Object, Object>>(
                    bodyExpression,
                    objParam
                );
                this.DebugView = bodyExpression.ToString();
                this.m_compiledExpression = uncompiledExpression.Compile();
            }

            using (CdssExecutionContext.Enter(cdssContext))
            {
                return this.m_compiledExpression(cdssContext);
            }
        }
    }
}