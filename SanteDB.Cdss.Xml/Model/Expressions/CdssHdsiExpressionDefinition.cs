using Newtonsoft.Json;
using SanteDB.Core.BusinessRules;
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
        /// Negate the HDSI expression
        /// </summary>
        [XmlAttribute("negate"), JsonProperty("negate")]
        public bool IsNegated { get; set; }

        /// <summary>
        /// Gets or sets the HDSI syntax expression
        /// </summary>
        [XmlText, JsonProperty("expression")]
        public String ExpressionValue { get; set; }

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if(String.IsNullOrEmpty(this.ExpressionValue))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.expression.hdsi.missingExpression", "HDSI expression require a property selector or binary expression", Guid.Empty);
            }
            
        }

        /// <inheritdoc/>
        internal override Expression GenerateComputableExpression(CdssExecutionContext cdssContext, params ParameterExpression[] parameters)
        {

            var variableDictionary = new Dictionary<String, Func<Object>>();
            foreach (var varRef in cdssContext.Variables.Union(cdssContext.FactNames ?? new String[0]))
            {
                variableDictionary.Add(varRef, () => CdssExecutionStackFrame.Current?.GetValue(varRef));
            }

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

            LambdaExpression bodyExpression = null;
            if (this.ExpressionValue.Contains("="))
            {
                bodyExpression = QueryExpressionParser.BuildLinqExpression(scopedObjectExpression.Type, this.ExpressionValue.ParseQueryString(), "s", variableDictionary, safeNullable: true, forceLoad: true, lazyExpandVariables: true);
                if(this.IsNegated)
                {
                    bodyExpression = Expression.Lambda(Expression.Not(bodyExpression.Body), bodyExpression.Parameters);
                }
            }
            else
            {
                bodyExpression = QueryExpressionParser.BuildPropertySelector(scopedObjectExpression.Type, this.ExpressionValue, true);
            }

            var retVal = Expression.Invoke(bodyExpression, scopedObjectExpression);
            return retVal;
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
