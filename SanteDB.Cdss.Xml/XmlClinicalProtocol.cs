/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-3-10
 */
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Cdss.Xml.Model;
using SanteDB.Core.Applets.ViewModel.Description;
using SanteDB.Core.Applets.ViewModel.Null;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SanteDB.Cdss.Xml
{
    /// <summary>
    /// Clinicl protocol that is stored/loaded via XML
    /// </summary>
    public class XmlClinicalProtocol : IClinicalProtocol
    {
        // Protocol definition
        private Core.Model.Acts.Protocol m_protocolDefinition = null;

        // Tracer
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(XmlClinicalProtocol));

        /// <summary>
        /// Default ctor
        /// </summary>
        public XmlClinicalProtocol()
        {
        }

        /// <summary>
        /// Creates a new protocol from the specified definition
        /// </summary>
        /// <param name="defn"></param>
        public XmlClinicalProtocol(ProtocolDefinition defn)
        {
            this.Definition = defn;
        }

        /// <summary>
        /// Gets or sets the definition of the protocol
        /// </summary>
        public ProtocolDefinition Definition { get; set; }

        /// <summary>
        /// Gets or sets the id of the protocol
        /// </summary>
        public Guid Id
        {
            get
            {
                return this.Definition.Uuid;
            }
        }

        /// <summary>
        /// Gets or sets the name of the protocol
        /// </summary>
        public string Name
        {
            get
            {
                return this.Definition?.Name;
            }
        }

        /// <summary>
        /// Gets the versionof this protocol
        /// </summary>
        public string Version => this.Definition?.Version;

        /// <summary>
        /// Gets the group id
        /// </summary>
        public string GroupId => this.Definition?.GroupId;

        /// <summary>
        /// Local index
        /// </summary>
        [ThreadStatic]
        private static Dictionary<String, Object> s_variables = null;

        /// <summary>
        /// Calculate the protocol against a atient
        /// </summary>
        public IEnumerable<Act> Calculate(Patient triggerPatient, IDictionary<String, Object> parameters)
        {
            try
            {
#if DEBUG
                Stopwatch sw = new Stopwatch();
                sw.Start();
#endif
                if (parameters == null)
                {
                    parameters = new Dictionary<String, Object>();
                }

                // Get a clone to make decisions on
                Patient patient = null;
                lock (triggerPatient)
                {
                    patient = triggerPatient.Clone() as Patient;
                    patient.Participations = triggerPatient.Participations?.ToList();
                }

                this.m_tracer.TraceInfo("Calculate ({0}) for {1}...", this.Name, patient);

                var context = new CdssContext<Patient>(patient);
                context.Set("index", 0);
                context.Set("parameters", parameters);
                foreach (var itm in parameters)
                {
                    context.Set(itm.Key, itm.Value);
                }

                // Evaluate eligibility
                if (this.Definition.When?.Evaluate(context) == false &&
                    !parameters.ContainsKey("ignoreEntry"))
                {
                    this.m_tracer.TraceInfo("{0} does not meet criteria for {1}", patient, this.Id);
                    return new List<Act>();
                }

                List<Act> retVal = new List<Act>();

                // Rules
                int step = 0;
                foreach (var rule in this.Definition.Rules)
                {
                    for (var index = 0; index < rule.Repeat; index++)
                    {

                        context.Set("index", index);
                        foreach (var itm in rule.Variables)
                        {
                            var value = itm.GetValue(null, context, new Dictionary<String, Object>());
                            context.Declare(itm.VariableName, itm.VariableType);
                            context.Set(itm.VariableName, value);
                        }

                        // TODO: Variable initialization
                        if (rule.When.Evaluate(context) && !parameters.ContainsKey("ignoreWhen"))
                        {
                            var acts = rule.Then.Evaluate(context);
                            retVal.AddRange(acts);

                            // Assign protocol
                            foreach (var itm in acts)
                            {
                                itm.Protocols.Add(new ActProtocol()
                                {
                                    ProtocolKey = this.Id,
                                    Protocol = this.GetProtocolData(),
                                    Sequence = step,
                                    Version = this.Version
                                });
                            }
                        }
                        else
                        {
                            this.m_tracer.TraceInfo("{0} does not meet criteria for rule {1}.{2}", patient, this.Name, rule.Name ?? rule.Id);
                        }

                        step++;
                    }
                }

                // Now we want to add the stuff to the patient
                lock (triggerPatient)
                {
                    triggerPatient.LoadProperty(o => o.Participations).AddRange(retVal.Where(o => o != null).Select(o => new ActParticipation(ActParticipationKeys.RecordTarget, triggerPatient) { Act = o, ParticipationRole = new Core.Model.DataTypes.Concept() { Key = ActParticipationKeys.RecordTarget, Mnemonic = "RecordTarget" }, Key = Guid.NewGuid() }));
                }
#if DEBUG
                sw.Stop();
                this.m_tracer.TraceVerbose("Protocol {0} took {1} ms", this.Name, sw.ElapsedMilliseconds);
#endif

                return retVal;
            }
            catch (Exception e)
            {
                throw new CdssEvaluationException($"Error applying protocol {this.Definition.Id}", this.Definition, e);
            }
        }

        /// <summary>
        /// Get protocol data
        /// </summary>
        public Core.Model.Acts.Protocol GetProtocolData()
        {
            if (this.m_protocolDefinition == null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    this.Definition.Save(ms);
                    this.m_protocolDefinition = new Core.Model.Acts.Protocol()
                    {
                        HandlerClass = this.GetType(),
                        Name = this.Name,
                        Definition = ms.ToArray(),
                        Key = this.Id,
                        Oid = this.Definition.Oid
                    };
                }
            }

            return this.m_protocolDefinition;
        }

        /// <summary>
        /// Create the protocol data from the protocol instance
        /// </summary>
        public IClinicalProtocol Load(Core.Model.Acts.Protocol protocolData)
        {
            if (protocolData == null)
            {
                throw new ArgumentNullException(nameof(protocolData));
            }

            using (MemoryStream ms = new MemoryStream(protocolData.Definition))
            {
                this.Definition = ProtocolDefinition.Load(ms);
            }

            var context = new CdssContext<Patient>();
            context.Declare("index", typeof(Int32));

            // Add callback rules
            foreach (var rule in this.Definition.Rules)
            {
                for (var index = 0; index < rule.Repeat; index++)
                {
                    foreach (var itm in rule.Variables)
                    {
                        context.Declare(itm.VariableName, itm.VariableType);
                    }
                }
            }

            this.Definition.When?.Compile<Patient>(context);
            foreach (var wc in this.Definition.Rules)
            {
                wc.When.Compile<Patient>(context);
            }

            return this;
        }

        /// <summary>
        /// Updates an existing plan
        /// </summary>
        public IEnumerable<Act> Update(Patient p, IEnumerable<Act> existingPlan)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initialize the patient
        /// </summary>
        public void Prepare(Patient p, IDictionary<String, Object> parameters)
        {
            if (parameters.ContainsKey("xml.initialized"))
            {
                return;
            }

            // Merge the view models
            NullViewModelSerializer serializer = new NullViewModelSerializer();

            if (parameters["runProtocols"] == null)
            {
                serializer.ViewModel = this.Definition.Initialize;
                serializer.ViewModel?.Initialize();
            }
            else
            {
                serializer.ViewModel = ViewModelDescription.Merge((parameters["runProtocols"] as IEnumerable<IClinicalProtocol>).OfType<XmlClinicalProtocol>().Select(o => o.Definition.Initialize));
                serializer.ViewModel?.Initialize();
            }

            // serialize - This will load data
            serializer.Serialize(p);

            parameters.Add("xml.initialized", true);
        }
    }
}