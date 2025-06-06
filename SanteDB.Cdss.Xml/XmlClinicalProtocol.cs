﻿/*
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
 * Date: 2023-9-15
 */
using SanteDB.Cdss.Xml.Model;
using SanteDB.Cdss.Xml.Model.Assets;
using SanteDB.Core.Cdss;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.i18n;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Roles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace SanteDB.Cdss.Xml
{
    /// <summary>
    /// Clinicl protocol that is stored/loaded via XML
    /// </summary>
    public class XmlClinicalProtocol : ICdssProtocol
    {

        // Tracer
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(XmlClinicalProtocol));
        private readonly CdssProtocolAssetDefinition m_protocol;
        private readonly IEnumerable<CdssLibraryDefinition> m_scopedLibraries;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal XmlClinicalProtocol(CdssProtocolAssetDefinition protocolAssetDefinition, IEnumerable<CdssLibraryDefinition> scopedLibraries)
        {
            this.m_protocol = protocolAssetDefinition;
            this.m_scopedLibraries = scopedLibraries;
        }

        /// <inheritdoc/>
        public Guid Uuid
        {
            get => this.m_protocol.Uuid;
            set
            {
                if (this.m_protocol.Uuid != Guid.Empty ||
                    value != this.m_protocol.Uuid)
                {
                    throw new InvalidOperationException(ErrorMessages.WOULD_RESULT_INVALID_STATE);
                }
                else
                {
                    this.m_protocol.Uuid = value;
                }
            }
        }

        /// <inheritdoc/>
        public String Id => this.m_protocol.Id;

        /// <inheritdoc/>
        public string Name => this.m_protocol?.Name;

        /// <inheritdoc/>
        public string Oid => this.m_protocol?.Oid;

        /// <inheritdoc/>
        public string Version => this.m_protocol?.Metadata?.Version;

        /// <inheritdoc/>
        public string Documentation => this.m_protocol?.Metadata?.Documentation;

        /// <inheritdoc/>
        public IEnumerable<ICdssProtocolScope> Scopes => this.m_protocol.Scopes.Select(o => new XmlProtocolAssetGroup(o));

        ///// <summary>
        ///// Initialize the patient
        ///// </summary>
        //public void Prepare(Patient p, IDictionary<String, Object> parameters)
        //{
        //    if (parameters.ContainsKey("xml.initialized"))
        //    {
        //        return;
        //    }

        //    // Merge the view models
        //    NullViewModelSerializer serializer = new NullViewModelSerializer();

        //    if (parameters["runProtocols"] == null)
        //    {
        //        serializer.ViewModel = this.m_definition.Initialize;
        //        serializer.ViewModel?.Initialize();
        //    }
        //    else
        //    {
        //        serializer.ViewModel = ViewModelDescription.Merge((parameters["runProtocols"] as IEnumerable<ICdssProtocol>).OfType<XmlClinicalProtocol>().Select(o => o.m_definition.Initialize));
        //        serializer.ViewModel?.Initialize();
        //    }

        //    // serialize - This will load data
        //    serializer.Serialize(p);

        //    parameters.Add("xml.initialized", true);
        //}

        /// <inheritdoc/>
        public IEnumerable<Object> ComputeProposals(Patient target, IDictionary<string, object> parameters)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif

            try
            {
                parameters = parameters ?? new Dictionary<String, Object>();

                if (this.m_protocol.Status == CdssObjectState.DontUse && (!parameters.TryGetValue(CdssParameterNames.DEBUG_MODE, out var dbg) || !XmlConvert.ToBoolean(dbg.ToString())))
                {
                    throw new InvalidOperationException(String.Format(ErrorMessages.FORBIDDEN_ON_OBJECT_IN_STATE));
                }

                object debugParameterValue = null;
                _ = parameters?.TryGetValue(CdssParameterNames.DEBUG_MODE, out debugParameterValue);
                _ = debugParameterValue is bool debugMode || Boolean.TryParse(debugParameterValue?.ToString() ?? "false", out debugMode);

                // Get a clone to make decisions on
                var targetClone = target.Clone() as Patient;
                // Safe guard all the properties
                targetClone.Participations = targetClone.Participations?.ToList() ?? target.GetParticipations().ToList();
                targetClone.Participations.ForEach(o => o.Act = o.Act?.Clone() as Act);
                this.m_tracer.TraceInfo("Calculate ({0}) for {1}...", this.Name, targetClone);

                CdssExecutionContext context = null;
                if (debugMode)
                {
                    context = CdssExecutionContext.CreateDebugContext((IdentifiedData)targetClone, this.m_scopedLibraries);
                }
                else
                {
                    context = CdssExecutionContext.CreateContext((IdentifiedData)targetClone, this.m_scopedLibraries);
                }

                using (CdssExecutionStackFrame.Enter(context))
                {

                    foreach (var itm in parameters)
                    {
                        context.SetValue(itm.Key, itm.Value);
                    }

                    if (!(bool)this.m_protocol.Compute())
                    {
                        yield break; // no computations
                    }
                    else
                    {
                        foreach (var prop in context.Proposals.OfType<Object>().Union(context.Issues))
                        {
                            yield return prop;
                        }
                    }
                }

                if(context.DebugSession != null)
                {
                    yield return context.DebugSession;
                }
            }
            finally
            {
#if DEBUG
                sw.Stop();
                this.m_tracer.TraceInfo("Calculate ({0}) for {1} took {2} ms...", this.Name, target, sw.ElapsedMilliseconds);
#endif
            }
        }
    }
}