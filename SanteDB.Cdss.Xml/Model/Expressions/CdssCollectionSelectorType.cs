using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Expressions
{
    /// <summary>
    /// Collector type
    /// </summary>
    [XmlType(nameof(CdssCollectionSelectorType), Namespace = "http://santedb.org/cdss")]
    public enum CdssCollectionSelectorType
    {
        [XmlEnum("first")]
        First,
        [XmlEnum("last")]
        Last,
        [XmlEnum("single")]
        Single,
        [XmlEnum("avg")]
        Average,
        [XmlEnum("sum")]
        Sum

    }
}