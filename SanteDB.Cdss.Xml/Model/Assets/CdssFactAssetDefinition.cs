﻿using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Model.Expressions;
using SanteDB.Cdss.Xml.XmlLinq;
using SanteDB.Core.i18n;
using SanteDB.Core.Model.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml;
using System.Xml.Linq;
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
            XmlElement("xml", typeof(CdssXmlLinqExpressionDefinition)),
            XmlElement("all", typeof(CdssAllExpressionDefinition)),
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
        public override object Compute()
        {
            base.ThrowIfInvalidState();
            
            if (m_compiledExpression == null)
            {
                var contextParameter = Expression.Parameter(CdssExecutionStackFrame.Current.Context.GetType(), CdssConstants.ContextVariableName);
                var scopedParameter = Expression.Parameter(CdssExecutionStackFrame.Current.ScopedObject.GetType(), CdssConstants.ScopedObjectVariableName);
                Expression bodyExpression = Expression.Lambda(FactComputation.GenerateComputableExpression(CdssExecutionStackFrame.Current.Context, contextParameter, scopedParameter), contextParameter, scopedParameter);

                // Wrap the expression, compile and set to this value
                // We do this because calling this.m_compiledExpression(context) is faster than 
                // (bool)this.m_compiledExpression.DynamicInvoke(context);
                var contextObjParam = Expression.Parameter(typeof(object));
                var scopeObjParam = Expression.Parameter(typeof(object));
                bodyExpression = Expression.Convert(Expression.Invoke(
                        bodyExpression, 
                        Expression.Convert(contextObjParam, contextParameter.Type ), 
                        Expression.Convert(scopeObjParam, scopedParameter.Type)
                    ), typeof(Object));

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
                            netType = typeof(DateTimeOffset);
                            break;
                        case CdssValueType.Integer:
                            netType = typeof(int);
                            break;
                        case CdssValueType.Real:
                            netType = typeof(double);
                            break;
                    }

                    if (netType != bodyExpression.Type)
                    {
                        bodyExpression = Expression.Convert(Expression.Convert(bodyExpression, netType), typeof(Object));
                    }
                }

                if (IsNegated)
                {
                    bodyExpression = Expression.Not(bodyExpression);
                }

                var uncompiledExpression = Expression.Lambda<Func<object, object, object>>(
                    bodyExpression,
                    contextObjParam,
                    scopeObjParam
                );
                this.DebugView = uncompiledExpression.ToString();
                this.m_compiledExpression = uncompiledExpression.Compile();
            }

            using (CdssExecutionStackFrame.EnterChildFrame(this))
            {
                var retVal = m_compiledExpression(CdssExecutionStackFrame.Current.Context, CdssExecutionStackFrame.Current.ScopedObject);
                retVal = this.Normalize?.FirstOrDefault(o => o.TransformObject(retVal) != null) ?? retVal;
                
                return retVal;
            }
        }
    }
}