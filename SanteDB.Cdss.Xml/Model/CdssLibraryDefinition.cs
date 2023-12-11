using Newtonsoft.Json;
using SanteDB.Core.BusinessRules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

        // Serializer
        private static XmlSerializer s_serializer = new XmlSerializer(typeof(CdssLibraryDefinition));

        /// <summary>
        /// Gets or sets the objects which are included in this definition
        /// </summary>
        [XmlElement("include"), JsonProperty("include")]
        public List<String> Include { get; set; }

        /// <summary>
        /// Gets the rulesets in the CDSS library definition
        /// </summary>
        [XmlElement("logic", typeof(CdssDecisionLogicBlockDefinition)), XmlElement("data", typeof(CdssDatasetDefinition)), JsonProperty("definitions")]
        public List<CdssBaseObjectDefinition> Definitions { get; set; }


        /// <summary>
        /// Loads the <see cref="CdssLibraryDefinition"/> from <paramref name="fromStream"/>
        /// </summary>
        /// <param name="fromStream">The stream from which the CDSS definition should be loaded</param>
        /// <returns>The loaded library definition</returns>
        public static CdssLibraryDefinition Load(Stream fromStream)
        {
            return (CdssLibraryDefinition)s_serializer.Deserialize(fromStream);
        }

        /// <summary>
        /// Save this definition to the specified <paramref name="toStream"/>
        /// </summary>
        /// <param name="toStream">The stream to which this CDSS definition should be saved</param>
        public void Save(Stream toStream)
        {
            s_serializer.Serialize(toStream, this);
        }

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if(this.Definitions?.Any() != true)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.library.empty", "CDSS library must contain at least one logic or data block", Guid.Empty, this.ToReferenceString());
            }
            else
            {
                foreach(var itm in this.Definitions.SelectMany(o=>o.Validate(context)))
                {

                    yield return itm;
                }
            }
        }

    }
}
