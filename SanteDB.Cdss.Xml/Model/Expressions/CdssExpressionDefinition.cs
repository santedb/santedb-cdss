using SanteDB.Core.Cdss;
using SanteDB.Core.Model;
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
        /// <param name="cdssContext">The CDSS context</param>
        /// <param name="parameters">The context parameter expressions to pass to the generation</param>
        /// <returns>The generated expression</returns>
        internal abstract Expression GenerateComputableExpression(CdssExecutionContext cdssContext, params ParameterExpression[] parameters);

    }
}
