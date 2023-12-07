using Newtonsoft.Json;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Roles;
using System;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Diagnostics
{
    /// <summary>
    /// Represents a wrapper for the value collected
    /// </summary>
    [XmlType(nameof(CdssDiagnosticSampleValueWrapper), Namespace = "http://santedb.org/cdss")]
    public class CdssDiagnosticSampleValueWrapper
    {

        /// <summary>
        /// Default serialization ctor
        /// </summary>
        public CdssDiagnosticSampleValueWrapper()
        {
            
        }

        /// <summary>
        /// Creates a new value wrapper using <paramref name="value"/>
        /// </summary>
        internal CdssDiagnosticSampleValueWrapper(object value)
        {
            switch(value)
            {
                case DateTimeOffset dto:
                    this.Value = dto.DateTime;
                    break;
                case float f:
                    this.Value = (double)f;
                    break;
                case decimal d:
                    this.Value = (double)d;
                    break;
                default:
                    this.Value = value;
                    break;
            }
        }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        [XmlElement("int", typeof(int)), 
            XmlElement("guid", typeof(Guid)), 
            XmlElement("string", typeof(String)), 
            XmlElement("long", typeof(long)), 
            XmlElement("bool", typeof(bool)), 
            XmlElement("date", typeof(DateTime)), 
            XmlElement("double", typeof(Double)),
            XmlElement("act", typeof(Act)), 
            XmlElement("substanceAdministration", typeof(SubstanceAdministration)),
            XmlElement("quantityObservation", typeof(QuantityObservation)),
            XmlElement("codedObservation", typeof(CodedObservation)),
            XmlElement("textObservation", typeof(TextObservation)),
            XmlElement("procedure", typeof(Procedure)),
            XmlElement("narrative", typeof(Narrative)),
            XmlElement("encounter", typeof(PatientEncounter)),
            XmlElement("patient", typeof(Patient)),
            XmlElement("provider", typeof(Provider)),
            XmlElement("entity", typeof(Entity)),
            XmlElement("material", typeof(Material)),
            XmlElement("person", typeof(Person)),
            XmlElement("manufacturedMaterial", typeof(ManufacturedMaterial)),
            JsonProperty("value")
            ]
        public object Value { get; set; }

    }
}