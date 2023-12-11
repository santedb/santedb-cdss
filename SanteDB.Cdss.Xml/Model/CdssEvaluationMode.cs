using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// Evaluation mode of the CDSS logic
    /// </summary>
    [XmlType(nameof(CdssEvaluationMode), Namespace = "http://santedb.org/cdss")]
    public enum CdssEvaluationMode
    {
        /// <summary>
        /// Any of the statements is evaluated to true
        /// </summary>
        [XmlEnum("any")]
        Any,
        /// <summary>
        /// All of the statements are evaluated to true
        /// </summary>
        [XmlEnum("all")]
        All
    }
}