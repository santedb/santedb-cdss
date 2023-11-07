using Newtonsoft.Json;
using SanteDB.Core.Cdss;
using SanteDB.Core.Model.Query;
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
    /// An HDSI based expression
    /// </summary>
    [XmlType(nameof(CdssHdsiExpressionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssHdsiExpressionDefinition : CdssExpressionDefinition
    {

        /// <summary>
        /// Default CTOR
        /// </summary>
        public CdssHdsiExpressionDefinition()
        {
        }

        /// <summary>
        /// Create a new HDSI expression definition from a string
        /// </summary>
        /// <param name="hdsiExpression">The expression to set</param>
        public CdssHdsiExpressionDefinition(string hdsiExpression)
        {
            this.ExpressionValue = hdsiExpression;
        }

        /// <summary>
        /// Gets or sets where the object should be pulled from
        /// </summary>
        [XmlAttribute("scope"), JsonProperty("scope")]
        public CdssHdsiExpressionScopeType Scope { get; set; }

        /// <summary>
        /// Gets or sets the HDSI syntax expression
        /// </summary>
        [XmlText, JsonProperty("expression")]
        public String ExpressionValue { get; set; }

        /// <inheritdoc/>
        internal override Expression GenerateComputableExpression(CdssExecutionContext cdssContext, params ParameterExpression[] parameters)
        {

            var variableDictionary = new Dictionary<String, Func<Object>>();
            foreach (var varRef in cdssContext.Variables)
            {
                variableDictionary.Add(varRef, () => CdssExecutionStackFrame.Current?.GetValue(varRef));
            }

            Expression bodyExpression = null;
            if (this.ExpressionValue.Contains("="))
            {
                bodyExpression = QueryExpressionParser.BuildLinqExpression(CdssExecutionStackFrame.Current.ScopedObject.GetType(), this.ExpressionValue.ParseQueryString(), "s", variableDictionary, safeNullable: true, forceLoad: true, lazyExpandVariables: true);
            }
            else
            {
                bodyExpression = QueryExpressionParser.BuildPropertySelector(CdssExecutionStackFrame.Current.ScopedObject.GetType(), this.ExpressionValue, true);
            }

            Expression scopedObjectExpression = null;
            switch(this.Scope)
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

    /// <summary>
    /// CDS expression scope
    /// </summary>
    [XmlType(nameof(CdssHdsiExpressionScopeType), Namespace = "http://santedb.org/cdss")]
    public enum CdssHdsiExpressionScopeType
    {
        /// <summary>
        /// Get the value from the current context object
        /// </summary>
        [XmlEnum("context")]
        Context = 0,
        /// <summary>
        /// Get the value from the proposed object
        /// </summary>
        [XmlEnum("scope")]
        ScopedObject = 1
    }
}
