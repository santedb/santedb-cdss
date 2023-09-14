using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// State of the CDSS object
    /// </summary>
    [XmlType(nameof(CdssObjectState), Namespace = "http://santedb.org/cdss")]
    public enum CdssObjectState
    {
        /// <summary>
        /// The object has no known state
        /// </summary>
        [XmlEnum("unk")]
        Unknown = 0,
        /// <summary>
        /// The object is intended for trial use
        /// </summary>
        [XmlEnum("trial-use")]
        TrialUse = 1,
        /// <summary>
        /// The object is active
        /// </summary>
        [XmlEnum("active")]
        Active = 2,
        /// <summary>
        /// The object is retired
        /// </summary>
        [XmlEnum("retired")]
        Retired = 3,
        /// <summary>
        /// The object should not be used
        /// </summary>
        [XmlEnum("dont-use")]
        DontUse = 4

    }
}