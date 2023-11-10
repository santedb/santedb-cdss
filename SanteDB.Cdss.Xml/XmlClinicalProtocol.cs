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
using SanteDB.Cdss.Xml.Model.Assets;
using SanteDB.Core.Model;
using SanteDB.Cdss.Xml.Model.Actions;
using SanteDB.Core.i18n;

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

        // Definition
        private CdssProtocolAssetDefinition m_definition;

        /// <inheritdoc/>
        public Guid Uuid => this.m_definition.Uuid;

        /// <inheritdoc/>
        public String Id => this.m_definition.Id;

        /// <inheritdoc/>
        public string Name => this.m_definition?.Name;

        /// <inheritdoc/>
        public string Oid => this.m_definition?.Oid;

        /// <inheritdoc/>
        public string Version => this.m_definition?.Metadata?.Version;

        /// <inheritdoc/>
        public string Documentation => this.m_definition?.Metadata?.Documentation;

        /// <inheritdoc/>
        public IEnumerable<ICdssProtocolScope> Scopes => this.m_definition.Scopes.Select(o => new XmlProtocolAssetGroup(o));

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
        public IEnumerable<Act> ComputeProposals(Patient target, IDictionary<string, object> parameters)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif

            try
            {
                parameters = parameters ?? new Dictionary<String, Object>();

                // Get a clone to make decisions on
                var targetClone = target.DeepCopy() as IdentifiedData;
                
                this.m_tracer.TraceInfo("Calculate ({0}) for {1}...", this.Name, targetClone);

                var context = CdssExecutionContext.CreateContext(targetClone, this.m_scopedLibraries);
                foreach (var itm in parameters)
                {
                    context.SetValue(itm.Key, itm.Value);
                }

                using (CdssExecutionStackFrame.Enter(context))
                {
                    if (!(bool)this.m_protocol.Compute())
                    {
                        yield break; // no computations
                    }
                    else
                    {
                        foreach (var prop in context.Proposals)
                        {
                            yield return prop;
                        }
                    }
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