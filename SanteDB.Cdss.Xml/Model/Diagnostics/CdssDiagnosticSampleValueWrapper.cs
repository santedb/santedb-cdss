/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: fyfej
 * Date: 2024-6-21
 */
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
            switch (value)
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