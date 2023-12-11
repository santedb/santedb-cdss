using SanteDB.Core.Configuration;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// References a type by the name of the resource
    /// </summary>
    [XmlType(nameof(CdssResourceTypeReference), Namespace = "http://santedb.org/cdss")]
    public class CdssResourceTypeReference : ResourceTypeReferenceConfiguration
    {
    }
}