using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Expressions
{
    /// <summary>
    /// CDSS fact reference 
    /// </summary>
    [XmlType(nameof(CdssFactReferenceExpressionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssFactReferenceExpressionDefinition : CdssExpressionDefinition
    {

        /// <summary>
        /// Reference to the fact name
        /// </summary>
        [XmlAttribute("ref"), JsonProperty("ref")]
        public String FactName { get; set; }

        /// <summary>
        /// Generate the expression for the lookup of the fact
        /// </summary>
        internal override Expression GenerateComputableExpression(CdssExecutionContext cdssContext, params ParameterExpression[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}
