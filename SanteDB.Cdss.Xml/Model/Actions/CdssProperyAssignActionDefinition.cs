using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Model.Expressions;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Actions
{
    /// <summary>
    /// Represents an action that assigns a property value
    /// </summary>
    [XmlType(nameof(CdssProperyAssignActionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssProperyAssignActionDefinition : CdssActionDefinition
    {

        // The compiled expression
        private Func<object, object, object> m_compiledExpression;

        /// <summary>
        /// View of the compiled expression source
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public String DebugView { get; private set; }

        /// <summary>
        /// Gets or sets the name of the property to assign
        /// </summary>
        [XmlAttribute("path"), JsonProperty("path")]
        public String Path { get; set; }

        /// <summary>
        /// Overwrite the property
        /// </summary>
        [XmlAttribute("overwrite"), JsonProperty("overwrite")]
        public bool OverwriteValue { get; set; }

        /// <summary>
        /// Expressions which this aggregate is made up of
        /// </summary>
        [XmlElement("csharp", typeof(CdssCsharpExpressionDefinition)),
            XmlElement("hdsi", typeof(CdssHdsiExpressionDefinition)),
            XmlElement("xml", typeof(CdssXmlLinqExpressionDefinition)),
            XmlElement("all", typeof(CdssAllExpressionDefinition)),
            XmlElement("any", typeof(CdssAnyExpressionDefinition)),
            XmlElement("fact", typeof(CdssFactReferenceExpressionDefinition)),
            XmlElement("fixed", typeof(String))]
        public Object ContainedExpression { get; set; }

        /// <inheritdoc/>
        internal override void Execute()
        {
            base.ThrowIfInvalidState();

            using (CdssExecutionStackFrame.EnterChildFrame(this))
            {
                switch (this.ContainedExpression)
                {
                    case CdssExpressionDefinition exe:
                        if (this.m_compiledExpression == null)
                        {
                            var contextParameter = Expression.Parameter(CdssExecutionStackFrame.Current.Context.GetType(), CdssConstants.ContextVariableName);
                            var scopeParameter = Expression.Parameter(CdssExecutionStackFrame.Current.ScopedObject.GetType(), CdssConstants.ScopedObjectVariableName);

                            var expressionForValue = exe.GenerateComputableExpression(CdssExecutionStackFrame.Current.Context, contextParameter, scopeParameter);

                            // Convert object parameters
                            var contextObjParameter = Expression.Parameter(typeof(Object));
                            var scopeObjParameter = Expression.Parameter(typeof(Object));
                            expressionForValue = Expression.Invoke(
                                    expressionForValue,
                                    Expression.Convert(contextObjParameter, contextParameter.Type),
                                    Expression.Convert(scopeObjParameter, scopeParameter.Type)
                                );

                            var uncompiledExpression = Expression.Lambda<Func<object, object, object>>(
                                expressionForValue,
                                contextObjParameter,
                                contextObjParameter,
                                scopeObjParameter
                            );
                            this.DebugView = uncompiledExpression.ToString();
                            this.m_compiledExpression = uncompiledExpression.Compile();
                            CdssExecutionStackFrame.Current.ScopedObject.GetOrSetValueAtPath(this.Path, this.m_compiledExpression(CdssExecutionStackFrame.Current.Context, CdssExecutionStackFrame.Current.ScopedObject), this.OverwriteValue);
                        }
                        break;
                    case String str:
                        CdssExecutionStackFrame.Current.ScopedObject.GetOrSetValueAtPath(this.Path, str, this.OverwriteValue);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }
    }
}