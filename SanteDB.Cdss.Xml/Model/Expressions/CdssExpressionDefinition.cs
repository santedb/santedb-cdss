using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Expressions
{
    /// <summary>
    /// Expression definition
    /// </summary>
    [XmlType(nameof(CdssExpressionDefinition), Namespace = "http://santedb.org/cdss")]
    public abstract class CdssExpressionDefinition
    {

        /// <summary>
        /// Generate the LINQ expression so it can be computed
        /// </summary>
        /// <typeparam name="TContext">The context in which the expression is being constructed</typeparam>
        /// <param name="cdssContext">The CDSS context</param>
        /// <param name="contextParameterExpression">The context parameter expression</param>
        /// <returns>The generated expression</returns>
        internal abstract Expression GenerateComputableExpression<TContext>(CdssContext<TContext> cdssContext, ParameterExpression contextParameterExpression);

    }
}
