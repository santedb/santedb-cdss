using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Model.Diagnostics;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Ami
{
    /// <summary>
    /// CDSS execution result
    /// </summary>
    [XmlType(nameof(CdssExecutionResult), Namespace = "http://santedb.org/cdss")]
    public class CdssExecutionResult : IResourceCollection
    {

        /// <summary>
        /// Gets or sets the start time
        /// </summary>
        [XmlElement("started"), JsonProperty("started")]
        public DateTimeOffset StartTime { get; set; }

        /// <summary>
        /// Gets or sets the time that the run was finished
        /// </summary>
        [XmlElement("finished"), JsonProperty("finished")]
        public DateTimeOffset StopTime { get; set; }

        /// <summary>
        /// Gets or sets the target after analysis is complete
        /// </summary>
        [XmlElement("target"), JsonProperty("target")]
        public IdentifiedData ResultingTarget { get; set; }

        /// <summary>
        /// Gets or sets the proposed actions
        /// </summary>
        [XmlElement("propose"), JsonProperty("propose")]
        public List<IdentifiedData> Proposals { get; set; }
        
        /// <inheritdoc/>
        [XmlIgnore, JsonIgnore]
        public IEnumerable<IIdentifiedResource> Item => this.Proposals;

        /// <summary>
        /// Gets the detected issues as a part of the execution
        /// </summary>
        [XmlElement("issue"), JsonProperty("issue")]
        public List<DetectedIssue> Issues { get; set; }

        /// <summary>
        /// Gets the total results
        /// </summary>
        public int? TotalResults => this.Proposals.Count;

        /// <summary>
        /// Gets or sets the debug information is present
        /// </summary>
        [XmlElement("debug"), JsonProperty("debug")]
        public CdssDiagnositcReport Debug { get; set; }

        /// <summary>
        /// Add annotation to all objects
        /// </summary>
        public void AddAnnotationToAll(object annotation)
        {
            this.Proposals.ForEach(o => o.AddAnnotation(annotation));
        }
    }
}
