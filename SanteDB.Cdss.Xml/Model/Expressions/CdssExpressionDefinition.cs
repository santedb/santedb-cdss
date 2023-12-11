using Newtonsoft.Json;
using SanteDB.Core.BusinessRules;
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
        /// Gets or sets the transpiled metadata
        /// </summary>
        [XmlElement("source"), JsonProperty("source")]
        public CdssTranspileMapMetaData TranspileSourceReference { get; set; }

        /// <summary>
        /// Generate the LINQ expression so it can be computed
        /// </summary>
        /// <param name="cdssContext">The CDSS context</param>
        /// <param name="parameters">The context parameter expressions to pass to the generation</param>
        /// <returns>The generated expression</returns>
        internal abstract Expression GenerateComputableExpression(CdssExecutionContext cdssContext, params ParameterExpression[] parameters);

        /// <summary>
        /// Validate that the expression is appropriately represented for execution
        /// </summary>
        /// <param name="context">The context in which the validation is occurring</param>
        public abstract IEnumerable<DetectedIssue> Validate(CdssExecutionContext context);

        /// <summary>
        /// Represent this as a source code reference string
        /// </summary>
        /// <returns></returns>
        public string ToReferenceString() => $"{this.GetType().Name} {this.TranspileSourceReference?.SourceFileName} @{this.TranspileSourceReference?.StartPosition}";
    }
}
