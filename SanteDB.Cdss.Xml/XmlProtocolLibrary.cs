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
 * Date: 2023-11-27
 */
using SanteDB.Cdss.Xml.Model;
using SanteDB.Cdss.Xml.Model.Assets;
using SanteDB.Core;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Cdss;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.i18n;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Claims;
using SanteDB.Core.Services;
using SharpCompress;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;

namespace SanteDB.Cdss.Xml
{
    /// <summary>
    /// An implementation of the <see cref="ICdssLibrary"/> which uses the XML format
    /// </summary>
    public class XmlProtocolLibrary : ICdssLibrary
    {

        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(XmlProtocolLibrary));

        // definition loaded from XML
        private CdssLibraryDefinition m_library;
        private IList<CdssLibraryDefinition> m_scopedLibraries;
        private ICdssLibraryRepositoryMetadata m_storageMetadata;

        /// <summary>
        /// Creates a new protocol library
        /// </summary>
        public XmlProtocolLibrary()
        {
        }

        /// <summary>
        /// Create a new protocol library 
        /// </summary>
        public XmlProtocolLibrary(CdssLibraryDefinition library)
        {
            this.m_library = library;
        }

        /// <inheritdoc/>
        public Guid Uuid
        {
            get => this.m_library.Uuid;
            set
            {
                if (this.m_library.Uuid != Guid.Empty &&
                    value != this.m_library.Uuid)
                {
                    throw new InvalidOperationException(ErrorMessages.WOULD_RESULT_INVALID_STATE);
                }
                else
                {
                    this.m_library.Uuid = value;
                }
                this.m_library.UuidSpecified = true;
            }
        }
        /// <inheritdoc/>
        public string Id => this.m_library.Id;

        /// <inheritdoc/>
        public string Name => this.m_library.Name;

        /// <inheritdoc/>
        public string Version => this.m_library.Metadata?.Version;

        /// <inheritdoc/>
        public string Oid => this.m_library.Oid;

        /// <inheritdoc/>
        public string Documentation => this.m_library.Metadata?.Documentation;

        /// <summary>
        /// Get the library definition
        /// </summary>
        public CdssLibraryDefinition Library => this.m_library;

        /// <summary>
        /// Storage metadata
        /// </summary>
        public ICdssLibraryRepositoryMetadata StorageMetadata
        {
            get => this.m_storageMetadata;
            set
            {
                if (this.m_storageMetadata != null)
                {
                    throw new InvalidOperationException(String.Format(ErrorMessages.WOULD_RESULT_INVALID_STATE, nameof(StorageMetadata)));
                }
                else
                {
                    this.m_storageMetadata = value;
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<Protocol> GetProtocolDefinitions()
        {
            return this.m_library.Definitions.OfType<CdssDecisionLogicBlockDefinition>()
                   .SelectMany(o => o.Definitions)
                   .OfType<CdssProtocolAssetDefinition>()
                   .Where(o => o.Status != CdssObjectState.DontUse)
                   .Select(p => new Protocol()
                   {
                       Key = p.Uuid,
                       Name = p.Name,
                       Oid = p.Oid
                   });
        }

        /// <summary>
        /// Get protocols defined for patients in the library
        /// </summary>
        public IEnumerable<ICdssProtocol> GetProtocols(Patient forPatient, IDictionary<String, Object> parameters, params String[] forScope)
        {

            // If this library is marked as dont use then don't use any protocols
            if (this.m_library.Status == CdssObjectState.DontUse && (!parameters.TryGetValue(CdssParameterNames.DEBUG_MODE, out var dbg) || !XmlConvert.ToBoolean(dbg.ToString())))
            {
                return new ICdssProtocol[0];
            }

            this.InitializeLibrary();
            var context = CdssExecutionContext.CreateContext(forPatient, this.m_scopedLibraries);
            parameters?.ForEach(o => context.SetValue(o.Key, o.Value));
            var retVal = this.m_library.Definitions.OfType<CdssDecisionLogicBlockDefinition>()
                    .AppliesTo(context)
                    .SelectMany(o => o.Definitions)
                    .OfType<CdssProtocolAssetDefinition>()
                    .Where(o => o.Status != CdssObjectState.DontUse)
                    .Select(p => new XmlClinicalProtocol(p, this.m_scopedLibraries));
            forScope = forScope.Where(o=>!String.IsNullOrEmpty(o)).ToArray();
            if (forScope.Any())
            {
                retVal = retVal.Where(o => o.Scopes.Any(s => forScope.Any(f => s.Oid == f || s.Name == f || s.Id == f)));
            }
            return retVal;
        }

        /// <inheritdoc/>
        public void Load(Stream definitionStream)
        {
            if (definitionStream == null)
            {
                throw new ArgumentNullException(nameof(definitionStream));
            }

            this.m_library = CdssLibraryDefinition.Load(definitionStream);
        }

        /// <inheritdoc/>
        public void Save(Stream definitionStream)
        {
            if (definitionStream == null)
            {
                throw new ArgumentNullException(nameof(definitionStream));
            }

            this.m_library.Save(definitionStream);
        }

        /// <summary>
        /// Initialize the library for use
        /// </summary>
        private void InitializeLibrary()
        {
            if (this.m_scopedLibraries == null)
            {
                var cdssLibraryService = ApplicationServiceContext.Current.GetService<ICdssLibraryRepository>();
                this.m_scopedLibraries = new List<CdssLibraryDefinition>() { this.m_library };
                this.m_scopedLibraries.AddRange(this.m_library.Include?.Select(o => cdssLibraryService?.ResolveReference(o)).OfType<XmlProtocolLibrary>().Select(o => o.Library) ?? new CdssLibraryDefinition[0]);

            }
        }

        /// <inheritdoc/>
        public IEnumerable<ICdssResult> Analyze(IdentifiedData analysisTarget, IDictionary<String, object> parameters = null)
        {

#if DEBUG
            var sw = new Stopwatch();
            sw.Start();
#endif
            try
            {
                // Is the library not active?
                object debugParameterValue = null;
                if (this.m_library.Status == CdssObjectState.DontUse ||
                    (this.m_library.Status == CdssObjectState.TrialUse && (parameters?.TryGetValue(CdssParameterNames.DEBUG_MODE, out debugParameterValue) != true || !XmlConvert.ToBoolean(debugParameterValue.ToString()))))
                {
                    return new ICdssResult[0];
                }


                this.m_tracer.TraceInfo("Starting analysis of {0} using {1}...", analysisTarget, this.Name);

                this.InitializeLibrary();

                if (analysisTarget is Entity ent) // HACK: Participations need to be reverse loaded
                {
                    ent.Participations = ent.Participations?.ToList() ?? ent.GetParticipations().ToList();
                }

                var context = CdssExecutionContext.CreateContext((IdentifiedData)analysisTarget, this.m_scopedLibraries);
                using (CdssExecutionStackFrame.Enter(context))
                {
                    parameters?.ForEach(o => context.SetValue(o.Key, o.Value));
                    context.SetValue(CdssParameterNames.EXECUTION_MODE, "analyze");
                    this.InitializeContextVariables(context);
                    
                    context.ThrowIfNotValid();

                    var definitions = this.m_library.Definitions
                        .OfType<CdssDecisionLogicBlockDefinition>()
                        .AppliesTo(context)
                        .SelectMany(o => o.Definitions)
                        .OfType<CdssRuleAssetDefinition>()
                        .Select(r => {
                            try
                            {
                                return r.Compute();
                            }
                            catch(Exception e)
                            {
                                context.PushIssue(new DetectedIssue(DetectedIssuePriorityType.Error, "error.cdss.exception", $"Could not apply {r.Name} - {e.Message}", Guid.Empty));
                                return false;
                            }
                        })
                        .ToArray();

                    return context.Issues.Select(o=>new CdssDetectedIssueResult(o)).OfType<ICdssResult>().Union(context.Proposals.Select(o=> new CdssProposeResult(o, analysisTarget.Key)));
                }
            }
            finally
            {
#if DEBUG
                sw.Stop();
                this.m_tracer.TraceInfo("Finished analysis of {0} (in {1} ms)", analysisTarget, sw.ElapsedMilliseconds);
#endif
            }
        }

        private void InitializeContextVariables(CdssExecutionContext context)
        {
            context.SetValue("currentUserName", AuthenticationContext.Current.Principal.Identity.Name);
            if(AuthenticationContext.Current.Principal is IClaimsPrincipal icp)
            {
                // Retrieve clinician claims
                context.SetValue("currentUserClinicalRole", String.Join(",", icp.FindAll(SanteDBClaimTypes.XspaUserRoleClaim).Select(o => o.Value)));
                context.SetValue("currentUserOrganization", String.Join(",", icp.FindAll(SanteDBClaimTypes.XspaOrganizationNameClaim).Select(o => o.Value)));
                context.SetValue("currentUserFacility", String.Join(",", icp.FindAll(SanteDBClaimTypes.XspaFacilityClaim).Select(o => o.Value)));
            }
        }

        /// <inheritdoc/>
        public IEnumerable<ICdssResult> Execute(IdentifiedData target, IDictionary<String, object> parameters = null)
        {

#if DEBUG
            var sw = new Stopwatch();
            sw.Start();
#endif
            try
            {
                // Is the library not active?
                object debugParameterValue = null, testingParmValue = null;
                _ = parameters?.TryGetValue(CdssParameterNames.DEBUG_MODE, out debugParameterValue);
                _ = debugParameterValue is bool debugMode || Boolean.TryParse(debugParameterValue?.ToString() ?? "false", out debugMode);
                var ignoreStatus = parameters?.TryGetValue(CdssParameterNames.TESTING, out testingParmValue) == true && Boolean.Parse(testingParmValue?.ToString());

                if (!ignoreStatus && this.m_library.Status == CdssObjectState.DontUse)
                {
                    throw new InvalidOperationException(String.Format(ErrorMessages.FORBIDDEN_ON_OBJECT_IN_STATE));
                }
                else if (this.m_library.Status == CdssObjectState.TrialUse && !(debugMode || ignoreStatus))
                {
                    return new ICdssResult[0];
                }

                this.m_tracer.TraceInfo("Starting analysis of {0} using {1}...", target, this.Name);

                this.InitializeLibrary();

                CdssExecutionContext context = null;
                if (debugMode)
                {
                    context = CdssExecutionContext.CreateDebugContext((IdentifiedData)target, this.m_scopedLibraries);
                }
                else
                {
                    context = CdssExecutionContext.CreateContext((IdentifiedData)target, this.m_scopedLibraries);
                }

                if (target is Entity ent) // HACK: Participations need to be reverse loaded
                {
                    ent.Participations = ent.Participations?.ToList() ?? ent.GetParticipations().ToList();
                }

                using (CdssExecutionStackFrame.Enter(context, this.m_library))
                {
                    parameters?.ForEach(o => context.SetValue(o.Key, o.Value));
                    context.SetValue(CdssParameterNames.EXECUTION_MODE, "execute");
                    this.InitializeContextVariables(context);
                    context.ThrowIfNotValid();

                    // If the library has protocols we want to select those for execution - otherwise all rules
                    var toExecute = this.m_library.Definitions.OfType<CdssDecisionLogicBlockDefinition>()
                        .AppliesTo(context)
                        .SelectMany(o => o.Definitions)
                        .OfType<CdssComputableAssetDefinition>();

                    if (toExecute.OfType<CdssProtocolAssetDefinition>().Any())
                    {
                        toExecute = toExecute.OfType<CdssProtocolAssetDefinition>();
                    }

                    var definitions = toExecute
                        .Select(r => new { result = r.Compute(), rule = r.Name })
                        .ToArray();

                    var retVal = context.Proposals.Select(o=>new CdssProposeResult(o)).OfType<ICdssResult>().Union(context.Issues.Select(o=>new CdssDetectedIssueResult(o))).ToList();
                    if (context.DebugSession != null)
                    {
                        retVal.Add(context.DebugSession);
                    }

                    return retVal;
                }

            }
            finally
            {
#if DEBUG
                sw.Stop();
                this.m_tracer.TraceInfo("Finished execution of {0} (in {1} ms)", target, sw.ElapsedMilliseconds);
#endif
            }
        }

    }
}
