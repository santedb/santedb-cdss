/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 */
using SanteDB.Cdss.Xml.Antlr;
using SanteDB.Cdss.Xml.Model;
using SanteDB.Core.Applets;
using SanteDB.Core.Applets.Services;
using SanteDB.Core.Cdss;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.IO;
using System.Linq;

namespace SanteDB.Cdss.Xml
{

    /// <summary>
    /// Redirect type for clinical protocol installer
    /// </summary>
    [Obsolete("Use AppletClinicalProtocolInstaller", true)]
    public class AppletClinicalProtocolRepository : AppletClinicalProtocolInstaller
    {
        public AppletClinicalProtocolRepository(IAppletManagerService appletManager, ICdssLibraryRepository clinicalProtocolRepositoryService, IRepositoryService<Protocol> protocolRepositoryService, IAppletSolutionManagerService appletSolutionManagerService = null) : base(appletManager, clinicalProtocolRepositoryService, protocolRepositoryService, appletSolutionManagerService)
        {
        }
    }

    /// <summary>
    /// Applet clinical protocol repository
    /// </summary>
    /// <remarks>
    /// <para>This implementation of the <see cref="IClinicalProtocolRepositoryService"/> is responsible for loading 
    /// clinical protocols defined in <see href="https://help.santesuite.org/developers/applets/cdss-protocols">SanteDB's CDSS XML</see> format and 
    /// translating them into <see cref="Protocol"/> instances which can then, in-turn, be linked with instances of <see cref="Act"/>
    /// which are also stored in the CDR.
    /// </para>
    /// </remarks>
    [ServiceProvider("Applet Based Clinical Protocol Installer")]
    public class AppletClinicalProtocolInstaller
    {

        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "Applet Based Clinical Protocol Repository";

        // Tracer
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(AppletClinicalProtocolInstaller));
        private readonly IAppletManagerService m_appletManager;
        private readonly IRepositoryService<Protocol> m_protocolRepositoryService;
        private readonly IAppletSolutionManagerService m_appletSolutionManager;
        private readonly ICdssLibraryRepository m_cdssRepositoryService;


        /// <summary>
        /// Clinical repository service
        /// </summary>
        public AppletClinicalProtocolInstaller(IAppletManagerService appletManager, 
            ICdssLibraryRepository clinicalProtocolRepositoryService, 
            IRepositoryService<Protocol> protocolRepositoryService,
            IAppletSolutionManagerService appletSolutionManagerService = null)
        {
            this.m_appletManager = appletManager;
            this.m_protocolRepositoryService = protocolRepositoryService;
            this.m_appletSolutionManager = appletSolutionManagerService;
            this.m_cdssRepositoryService = clinicalProtocolRepositoryService;
            this.LoadProtocols();
            appletManager.Changed += (o, e) => this.LoadProtocols();
        }

        /// <summary>
        /// Load protocols
        /// </summary>
        private void LoadProtocols()
        {
            try
            {
                using (AuthenticationContext.EnterSystemContext())
                {
                    var solutions = this.m_appletSolutionManager?.Solutions.ToList();
                    if (solutions == null)
                    {
                        this.ProcessApplet(this.m_appletManager.Applets); // no solution manager
                    }
                    else
                    {
                        solutions.Add(new Core.Applets.Model.AppletSolution() { Meta = new Core.Applets.Model.AppletInfo() { Id = String.Empty } });
                        foreach (var s in solutions)
                        {
                            var appletCollection = this.m_appletSolutionManager.GetApplets(s.Meta.Id);
                            this.ProcessApplet(appletCollection);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                this.m_tracer.TraceError("Error installing clinical protocols: {0}", e);
            }
        }

        private void ProcessApplet(ReadonlyAppletCollection appletCollection)
        {
            try
            {

                // Get protocols from the applet
                var protocols = appletCollection.SelectMany(o => o.Assets).Where(o => o.Name.StartsWith("protocols/"));

                foreach (var f in protocols)
                {
                    using (var sourceReader = new MemoryStream(appletCollection.RenderAssetContent(f)))
                    {
                        try
                        {
                            CdssLibraryDefinition library = null;
                            if (f.Name.EndsWith(".cdss"))
                            {
                                library = CdssLibraryTranspiler.Transpile(sourceReader, false);
                            }
                            else
                            {
                                library = CdssLibraryDefinition.Load(sourceReader);
                            }

                            var existing = this.m_cdssRepositoryService.Find(o => o.Id == library.Id).FirstOrDefault();

                            // Is the UUID different then don't install or if the version is older
                            if (existing == null || existing.Uuid == library.Uuid && library.Metadata?.Version.ParseVersion(out _) > existing.Version.ParseVersion(out _))
                            {
                                this.m_tracer.TraceInfo("Installing CDSS rule from applet {0}...", library.Name ?? library.Oid);
                                this.m_cdssRepositoryService.InsertOrUpdate(new XmlProtocolLibrary(library));
                            }

                        }
                        catch (Exception e)
                        {
                            this.m_tracer.TraceWarning("Could not load {0} due to {1}", f.Name, e.ToHumanReadableString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                this.m_tracer.TraceWarning("Cannot process applet {0} for protocols - {1}", appletCollection, e.ToHumanReadableString());
            }
        }
    }
}