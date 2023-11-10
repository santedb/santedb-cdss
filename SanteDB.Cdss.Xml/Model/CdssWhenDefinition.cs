using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Model.Assets;
using SanteDB.Cdss.Xml.Model.Expressions;
using System.Collections.Generic;
using System.Linq.Expressions;
using System;
using System.Xml.Serialization;
using SanteDB.Cdss.Xml.Model.Actions;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// Defines a WHEN condition
    /// </summary>
    [XmlType(nameof(CdssWhenDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssWhenDefinition : CdssBaseObjectDefinition
    {


        // The expression which has been calculated
        private Func<object, object, bool> m_compiledExpression;

        /// <summary>
        /// Gets the expression of the fact
        /// </summary>
        [XmlElement("csharp", typeof(CdssCsharpExpressionDefinition)),
         XmlElement("hdsi", typeof(CdssHdsiExpressionDefinition)),
         XmlElement("xml", typeof(CdssXmlLinqExpressionDefinition)),
         XmlElement("all", typeof(CdssAllExpressionDefinition)),
         XmlElement("any", typeof(CdssAnyExpressionDefinition)),
         XmlElement("fact", typeof(CdssFactReferenceExpressionDefinition)),
         JsonProperty("logic")]
        public CdssExpressionDefinition WhenComputation { get; set; }

        /// <summary>
        /// Debug view
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public string DebugView { get; private set; }

        /// <inheritdoc/>
        public bool Compute()
        {
            if (this.m_compiledExpression == null)
            {
                var contextParameter = Expression.Parameter(CdssExecutionStackFrame.Current.Context.GetType(), CdssConstants.ContextVariableName);
                var scopedParameter = Expression.Parameter(CdssExecutionStackFrame.Current.ScopedObject.GetType(), CdssConstants.ScopedObjectVariableName);

                Expression bodyExpression = Expression.Lambda(this.WhenComputation.GenerateComputableExpression(CdssExecutionStackFrame.Current.Context, contextParameter, scopedParameter), contextParameter, scopedParameter);

                // Wrap the expression, compile and set to this value
                // We do this because calling this.m_compiledExpression(context) is faster than 
                // (bool)this.m_compiledExpression.DynamicInvoke(context);
                var contextObjParam = Expression.Parameter(typeof(object));
                var scopeObjParam = Expression.Parameter(typeof(object));
                bodyExpression = Expression.Convert(Expression.Invoke(
                        bodyExpression,
                        Expression.Convert(contextObjParam, contextParameter.Type),
                        Expression.Convert(scopeObjParam, scopedParameter.Type)
                    ), typeof(Object));


                if (typeof(bool) != bodyExpression.Type)
                {
                    bodyExpression = Expression.Convert(bodyExpression, typeof(bool));
                }

                var uncompiledExpression = Expression.Lambda<Func<object, object, bool>>(
                    bodyExpression,
                    contextObjParam,
                    scopeObjParam
                );
                this.DebugView = uncompiledExpression.ToString();
                this.m_compiledExpression = uncompiledExpression.Compile();
            }

            using (CdssExecutionStackFrame.EnterChildFrame(this))
            {
                var result = m_compiledExpression(CdssExecutionStackFrame.Current.Context, CdssExecutionStackFrame.Current.ScopedObject);
                return result;
            }
        }
    }
}