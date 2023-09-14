using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// CDSS for grammar 
    /// </summary>
    [XmlType(nameof(CdssExpressionGrammarType), Namespace = "http://santedb.org/cdss")]
    public enum CdssExpressionGrammarType
    {
        /// <summary>
        /// C# expression
        /// </summary>
        [XmlEnum("csharp")]
        CSharp,
        /// <summary>
        /// HDSI expression
        /// </summary>
        [XmlEnum("hdsi")]
        Hdsi,
        /// <summary>
        /// Linq expression
        /// </summary>
        [XmlEnum("xlinq")]
        XmlLinq,
        /// <summary>
        /// Constant
        /// </summary>
        [XmlEnum("const")]
        Constant
    }
}