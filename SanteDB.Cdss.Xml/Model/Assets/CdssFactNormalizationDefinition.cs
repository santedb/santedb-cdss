using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Model.Expressions;
using System;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Assets
{
    /// <summary>
    /// Represents an instruction to emit/normalize 
    /// </summary>
    [XmlType(nameof(CdssFactNormalizationDefinition), Namespace = "http://santedb.org/santedb")]
    public class CdssFactNormalizationDefinition
    {

        // The expression which has been calculated
        private Func<object, object, object, object> m_compiledExpression;

        /// <summary>
        /// Represents the "when" clause for the rule
        /// </summary>
        [XmlElement("when"), JsonProperty("when")]
        public CdssFactAssetDefinition When { get; set; }

        /// <summary>
        /// Debug view of the normalization
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public String DebugView { get; private set; }

        /// <summary>
        /// Expression to emit the data in the proper norm
        /// </summary>
        [XmlElement("csharp", typeof(CdssCsharpExpressionDefinition)),
          XmlElement("hdsi", typeof(CdssHdsiExpressionDefinition)),
          XmlElement("xml", typeof(CdssXmlLinqExpressionDefinition)),
          XmlElement("all", typeof(CdssAllExpressionDefinition)),
          XmlElement("any", typeof(CdssAnyExpressionDefinition)),
          XmlElement("fact", typeof(CdssFactAssetDefinition)),
          JsonProperty("logic")]
        public CdssExpressionDefinition EmitExpression { get; set; }

        /// <summary>
        /// Transform the object, return null if the transfomration is not successful
        /// </summary>
        internal object TransformObject(object retVal)
        {
            
            if(this.When.Compute() is bool b && b)
            {
                if(this.m_compiledExpression == null)
                {
                    var contextParameter = Expression.Parameter(CdssExecutionStackFrame.Current.Context.GetType(), CdssConstants.ContextVariableName);
                    var scopedParameter = Expression.Parameter(CdssExecutionStackFrame.Current.ScopedObject.GetType(), CdssConstants.ScopedObjectVariableName);
                    var valueParameter = Expression.Parameter(retVal.GetType(), CdssConstants.ValueVariableName);

                    var expressionForValue = this.EmitExpression.GenerateComputableExpression(CdssExecutionStackFrame.Current.Context, contextParameter, scopedParameter, valueParameter);

                    // Convert object parameters for our FUNC
                    var contextObjParameter = Expression.Parameter(typeof(Object));
                    var valueObjParameter = Expression.Parameter(typeof(Object));
                    var scopeObjParameter = Expression.Parameter(typeof(Object));
                    expressionForValue = Expression.Invoke(
                            expressionForValue,
                            Expression.Convert(contextObjParameter, contextParameter.Type),
                            Expression.Convert(scopeObjParameter, scopedParameter.Type),
                            Expression.Convert(valueObjParameter, valueParameter.Type)
                        );

                    var uncompiledExpression = Expression.Lambda<Func<object, object, object, object>>(
                        expressionForValue,
                        contextObjParameter,
                        scopeObjParameter,
                        valueObjParameter
                    );
                    this.DebugView = uncompiledExpression.ToString();
                    this.m_compiledExpression = uncompiledExpression.Compile();
                }

                return this.m_compiledExpression(CdssExecutionStackFrame.Current.Context, CdssExecutionStackFrame.Current.ScopedObject, retVal);
            }
            return null;
        }
    }
}