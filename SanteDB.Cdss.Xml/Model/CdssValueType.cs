using System;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// CDSS value types
    /// </summary>
    [XmlType(nameof(CdssValueType), Namespace = "http://santedb.org/cdss")]
    public enum CdssValueType
    {
        [XmlEnum("auto")]
        Unspecified = 0,
        /// <summary>
        /// The CDSS value type is <see cref="DateTimeOffset"/>
        /// </summary>
        [XmlEnum("date")]
        Date,
        /// <summary>
        /// The CDSS value type is an <see cref="int"/>
        /// </summary>
        [XmlEnum("integer")]
        Integer,
        /// <summary>
        /// Represents a 64 bit signed integer
        /// </summary>
        [XmlEnum("long")]
        Long,
        /// <summary>
        /// The CDSS value type is a <see cref="string"/>
        /// </summary>
        [XmlEnum("string")]
        String,
        /// <summary>
        /// The CDSS value type is a <see cref="bool"/>
        /// </summary>
        [XmlEnum("bool")]
        Boolean,
        /// <summary>
        /// The CDSS value type is a <see cref="float"/>
        /// </summary>
        [XmlEnum("real")]
        Real

    }
}