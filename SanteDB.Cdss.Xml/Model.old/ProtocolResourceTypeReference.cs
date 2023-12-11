using SanteDB.Core.Configuration;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// Represents a reference to a type via the resource name
    /// </summary>
    [XmlType(nameof(ProtocolResourceTypeReference), Namespace = "http://santedb.org/cdss")]
    public class ProtocolResourceTypeReference : ResourceTypeReferenceConfiguration
    {
    }
}