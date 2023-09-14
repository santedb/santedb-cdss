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
 * Date: 2023-5-19
 */
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Cdss.Xml.Model;
using SanteDB.Core;
using SanteDB.Core.Applets.ViewModel.Description;
using SanteDB.Core.Applets.ViewModel.Null;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Cdss;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SanteDB.Core.Model.Entities;

namespace SanteDB.Cdss.Xml
{
    /// <summary>
    /// Clinicl protocol that is stored/loaded via XML
    /// </summary>
    public class XmlClinicalProtocol : ICdssProtocolAsset
    {

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
            this.m_definition = defn;
        }

        // Definition
        private ProtocolDefinition m_definition;

        /// <inheritdoc/>
        public Guid Uuid => this.m_definition.Uuid;

        /// <inheritdoc/>
        public String Id => this.m_definition.Id;

        /// <inheritdoc/>
        public string Name => this.m_definition?.Name;

        /// <inheritdoc/>
        public string Oid => this.m_definition?.Oid;

        /// <inheritdoc/>
        public string Version => this.m_definition?.Version;

        /// <inheritdoc/>
        public string Documentation => this.m_definition?.Documentation;

        /// <inheritdoc/>
        public CdssAssetClassification Classification => CdssAssetClassification.DecisionSupportProtocol;

        /// <inheritdoc/>
        public IEnumerable<ICdssAssetGroup> Groups => this.m_definition.Groups.Select(o => new XmlProtocolAssetGroup(o));

        /// <summary>
        /// Local index
        /// </summary>
        [ThreadStatic]
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CS0414
        private static Dictionary<String, Object> s_variables = null;
#pragma warning restore CS0414
#pragma warning restore IDE0051 // Remove unused private members

        /// <summary>
        /// Calculate the protocol against a atient
        /// </summary>
        public IEnumerable<Act> ComputeProposals(Entity target, IDictionary<String, Object> parameters, out IEnumerable<DetectedIssue> detectedIssues)
        {
            try
            {
#if DEBUG
                Stopwatch sw = new Stopwatch();
                sw.Start();
#endif
                parameters = parameters ?? new Dictionary<String, Object>();

                // Get a clone to make decisions on
                Entity targetClone = null;
                lock (target)
                {
                    targetClone = target.Clone() as Entity;
                    targetClone.Participations = target.Participations?.ToList();
                }

                this.m_tracer.TraceInfo("Calculate ({0}) for {1}...", this.Name, targetClone);

                var context = new CdssContext<Entity>(targetClone);
                context.Set("index", 0);
                context.Set("parameters", parameters);
                foreach (var itm in parameters)
                {
                    context.Set(itm.Key, itm.Value);
                }

                // Get the rule-sets which apply
                var applicableRulesets = this.m_definition.Rulesets.Where(o => o.AppliesTo.Any(r => r.Type.IsAssignableFrom(target.GetType())));
                List<Act> retVal = new List<Act>();

                foreach (var ruleset in applicableRulesets)
                {
                    this.m_tracer.TraceVerbose("Applying ruleset {0} to {1}", ruleset.Id, targetClone);


                    // Evaluate eligibility
                    if (ruleset.When?.Evaluate(context) == false &&
                        !parameters.ContainsKey("ignoreEntry"))
                    {
                        this.m_tracer.TraceInfo("{0} does not meet criteria for {1}", targetClone, this.Uuid);
                        detectedIssues = new DetectedIssue[]
                        {
                            new DetectedIssue(DetectedIssuePriorityType.Information, "N/A", $"Patient does not meet criteria {this.Uuid} for ruleset {ruleset.Id}", DetectedIssueKeys.OtherIssue)
                        };
                        continue; // skip
                    }

                    // Global variables
                    foreach(var var in ruleset.Variables)
                    {
                        context.Declare(var.Name, var.VariableType);
                        context.Set(var.Name, var.Evaluate())
                    }
                }

                // Rules
                int step = 0;
                foreach (var rule in this.m_definition.Rules)
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

                            // Assign protocol
                            foreach (var itm in acts)
                            {
                                itm.Protocols.Add(new ActProtocol()
                                {
                                    ProtocolKey = this.Uuid,
                                    Protocol = this.ToProtocol(),
                                    Sequence = step,
                                    Version = this.Version
                                });
                                retVal.Add(itm);
                            }
                        }
                        else
                        {
                            this.m_tracer.TraceInfo("{0} does not meet criteria for rule {1}.{2}", targetClone, this.Name, rule.Name ?? rule.Id);
                        }

                        step++;
                    }
                }

                // Now we want to add the stuff to the patient
                lock (target)
                {
                    target.LoadProperty(o => o.Participations).AddRange(retVal.Where(o => o != null).Select(o => new ActParticipation(ActParticipationKeys.RecordTarget, target) { Act = o, ParticipationRole = new Core.Model.DataTypes.Concept() { Key = ActParticipationKeys.RecordTarget, Mnemonic = "RecordTarget" }, Key = Guid.NewGuid() }));
                }
#if DEBUG
                sw.Stop();
                this.m_tracer.TraceVerbose("Protocol {0} took {1} ms", this.Name, sw.ElapsedMilliseconds);
#endif
                detectedIssues = context.DetectedIssues;

                return retVal;
            }
            catch (Exception e)
            {
                throw new CdssEvaluationException($"Error applying protocol {this.m_definition.Id}", this.m_definition, e);
            }
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
                serializer.ViewModel = this.m_definition.Initialize;
                serializer.ViewModel?.Initialize();
            }
            else
            {
                serializer.ViewModel = ViewModelDescription.Merge((parameters["runProtocols"] as IEnumerable<ICdssProtocolAsset>).OfType<XmlClinicalProtocol>().Select(o => o.m_definition.Initialize));
                serializer.ViewModel?.Initialize();
            }

            // serialize - This will load data
            serializer.Serialize(p);

            parameters.Add("xml.initialized", true);
        }

        /// <inheritdoc/>
        public void Load(Stream definitionStream)
        {
            if(definitionStream == null)
            {
                throw new ArgumentNullException(nameof(definitionStream));
            }
            this.m_definition = ProtocolDefinition.Load(definitionStream);
        }

        /// <inheritdoc/>
        public void Save(Stream definitionStream)
        {
            if(definitionStream == null)
            {
                throw new ArgumentNullException(nameof(definitionStream));
            }
            this.m_definition.Save(definitionStream);
        }

        public IEnumerable<DetectedIssue> Analyze(Act collectedData)
        {
            throw new NotImplementedException(); // TODO:
        }
    }
}