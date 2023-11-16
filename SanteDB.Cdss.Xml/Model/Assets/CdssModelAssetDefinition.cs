﻿using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Core.Applets.ViewModel.Json;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.i18n;
using SanteDB.Core.Model.Acts;
using System;
using System.Collections.Generic;
using System.Text;
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
            return this.m_parsedModel.DeepCopy() as Act;
        }

        /// <summary>
        /// Validate the model
        /// </summary>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if (String.IsNullOrEmpty(this.ReferencedModel) && this.Model == null)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.model.missing", "Model element must either reference a shared model or must provide a model", Guid.Empty, this.ToString());
            }
        }


    }
}