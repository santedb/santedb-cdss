using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Model.Assets;
using SanteDB.Core.i18n;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Actions
{
    /// <summary>
    /// Represents a repetition of any action
    /// </summary>
    [XmlType(nameof(CdssRepeatActionDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssRepeatActionDefinition : CdssExecuteActionDefinition
    {

        /// <summary>
        /// The number of iterations to repeat
        /// </summary>
        [XmlAttribute("iterations"), JsonProperty("iterations")]
        public int Iterations { get; set; }

        /// <summary>
        /// Iterations are specified
        /// </summary>
        [XmlIgnore, JsonIgnore]
        public bool IterationsSpecified { get; set; }

        /// <summary>
        /// The variable to track iterations
        /// </summary>
        [XmlAttribute("trackBy"), JsonProperty("trackBy")]
        public string IterationVariable { get; set; }

        /// <summary>
        /// Repeat until the fact provided is true
        /// </summary>
        [XmlElement("until"), JsonProperty("until")]
        public CdssFactAssetDefinition Until { get; set; }

        /// <inheritdoc/>
        internal override void Execute()
        {
            base.ThrowIfInvalidState();

            using(CdssExecutionStackFrame.EnterChildFrame(this))
            {
                var iteration = 0;
                
                while(this.IterationsSpecified && iteration <= this.Iterations || true) {

                    iteration++;
                    base.Execute();

                    // If there is an UNTIL clause evaluate it
                    var untilResult = this.Until?.Compute();
                    if(untilResult is Boolean b && b || untilResult != null)
                    {
                        break;
                    }
                }
            }
        }
    }
}