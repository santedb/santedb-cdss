using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// Defines a library of CDSS objects 
    /// </summary>
    [XmlType(nameof(CdssLibraryDefinition), Namespace = "http://santedb.org/cdss")]
    [XmlRoot("CdssLibrary", Namespace = "http://santedb.org/cdss")]
    public class CdssLibraryDefinition : CdssBaseObjectDefinition
    {

        /// <summary>
        /// Gets or sets the objects which are included in this definition
        /// </summary>
        [XmlElement("include"), JsonProperty("include")]
        public List<CdssBaseObjectReference> Include { get; set; }

        /// <summary>
        /// Gets the rulesets in the CDSS library definition
        /// </summary>
        [XmlElement("logic", typeof(CdssDecisionLogicBlockDefinition)), XmlElement("data", typeof(CdssDatasetDefinition)), JsonProperty("definitions")]
        public List<CdssBaseObjectDefinition> Definitions { get; set; }

    }
}
