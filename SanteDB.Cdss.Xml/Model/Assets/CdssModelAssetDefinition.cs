/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Core.ViewModel.Json;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.i18n;
using SanteDB.Core.Model.Acts;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Assets
{
    /// <summary>
    /// CDSS Model Asset
    /// </summary>
    [XmlType(nameof(CdssModelAssetDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssModelAssetDefinition : CdssComputableAssetDefinition
    {
        // JSON Serializer
        private static JsonViewModelSerializer s_serializer = new JsonViewModelSerializer();

        // Parsed model
        private Act m_parsedModel;

        /// <summary>
        /// Reference model 
        /// </summary>
        [XmlAttribute("ref"), JsonProperty("ref")]
        public String ReferencedModel { get; set; }

        /// <summary>
        /// Gets or sets the model
        /// </summary>
        [XmlElement("json", typeof(String)),
            XmlElement(nameof(Act), typeof(Act), Namespace = "http://santedb.org/model"),
            XmlElement(nameof(TextObservation), typeof(TextObservation), Namespace = "http://santedb.org/model"),
            XmlElement(nameof(SubstanceAdministration), typeof(SubstanceAdministration), Namespace = "http://santedb.org/model"),
            XmlElement(nameof(QuantityObservation), typeof(QuantityObservation), Namespace = "http://santedb.org/model"),
            XmlElement(nameof(CodedObservation), typeof(CodedObservation), Namespace = "http://santedb.org/model"),
            XmlElement(nameof(PatientEncounter), typeof(PatientEncounter), Namespace = "http://santedb.org/model"),
            XmlElement(nameof(Procedure), typeof(Procedure), Namespace = "http://santedb.org/model"),
            XmlElement(nameof(Narrative), typeof(Narrative), Namespace = "http://santedb.org/model"),
            JsonProperty("json")]
        public object Model { get; set; }

        /// <inheritdoc/>
        public override object Compute()
        {

            if (this.m_parsedModel == null)
            {
                if (!String.IsNullOrEmpty(this.ReferencedModel) && CdssExecutionStackFrame.Current.Context.TryGetModel(this.ReferencedModel, out var refModel))
                {
                    this.m_parsedModel = refModel as Act;
                }
                else
                {
                    switch (this.Model)
                    {
                        case String jsonString:
                            this.m_parsedModel = s_serializer.DeSerialize<Act>(jsonString);
                            break;
                        case Act act:
                            this.m_parsedModel = act.DeepCopy() as Act;
                            break;
                        default:
                            throw new CdssEvaluationException(String.Format(ErrorMessages.DEPENDENT_PROPERTY_NULL, nameof(this.Model)));
                    }
                }
            }
            using (CdssExecutionStackFrame.EnterChildFrame(this))
            {
                return this.m_parsedModel.DeepCopy() as Act;
            }
        }

        /// <summary>
        /// Validate the model
        /// </summary>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if (String.IsNullOrEmpty(this.ReferencedModel) && this.Model == null)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.model.missing", "Model element must either reference a shared model or must provide a model", Guid.Empty, this.ToReferenceString());
            }
            else if (this.Model is String jsonStr) // Try parse JSON
            {
                JsonException je = null;
                try
                {
                    JsonConvert.DeserializeObject(jsonStr);
                }
                catch (JsonException e)
                {
                    je = e;
                }
                if (je != null)
                {
                    yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.model.invalid", $"JSON model does not appear to be valid JSON {je.Message}", Guid.Empty, this.ToReferenceString());
                }
            }
        }


    }
}
