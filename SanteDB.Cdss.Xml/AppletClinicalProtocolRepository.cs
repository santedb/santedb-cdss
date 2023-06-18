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
using SanteDB.Cdss.Xml.Model;
using SanteDB.Core.Applets.Services;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Serialization;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml
{

    /// <summary>
    /// Redirect type for clinical protocol installer
    /// </summary>
    [Obsolete("Use AppletClinicalProtocolInstaller", true)]
    public class AppletClinicalProtocolRepository : AppletClinicalProtocolInstaller
    {
        /// <inheritdoc/>
        public AppletClinicalProtocolRepository(IAppletManagerService appletManager, IClinicalProtocolRepositoryService clinicalProtocolRepositoryService) : base(appletManager, clinicalProtocolRepositoryService)
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
        private XmlSerializer m_xsz = XmlModelSerializerFactory.Current.CreateSerializer(typeof(ProtocolDefinition));

        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "Applet Based Clinical Protocol Repository";

        // Tracer
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(AppletClinicalProtocolInstaller));


        /// <summary>
        /// Clinical repository service
        /// </summary>
        public AppletClinicalProtocolInstaller(IAppletManagerService appletManager, IClinicalProtocolRepositoryService clinicalProtocolRepositoryService)
        {
            this.LoadProtocols(appletManager, clinicalProtocolRepositoryService);
        }

        /// <summary>
        /// Load protocols
        /// </summary>
        private void LoadProtocols(IAppletManagerService appletManager, IClinicalProtocolRepositoryService protocolRepositoryService)
        {
            try
            {
                using (AuthenticationContext.EnterSystemContext())
                {
                    // Get protocols from the applet
                    var protocols = appletManager.Applets.SelectMany(o => o.Assets).Where(o => o.Name.StartsWith("protocols/"));

                    foreach (var f in protocols)
                    {
                        var content = f.Content ?? appletManager.Applets.Resolver(f);
                        if (content is String)
                        {
                            using (var rStream = new StringReader(content as String))
                            {
                                protocolRepositoryService.InsertProtocol(
                                    new XmlClinicalProtocol(this.m_xsz.Deserialize(rStream) as ProtocolDefinition)
                                );
                            }
                        }
                        else if (content is byte[])
                        {
                            using (var rStream = new MemoryStream(content as byte[]))
                            {
                                protocolRepositoryService.InsertProtocol(new XmlClinicalProtocol(ProtocolDefinition.Load(rStream)));
                            }
                        }
                        else if (content is XElement)
                        {
                            using (var rStream = (content as XElement).CreateReader())
                            {
                                protocolRepositoryService.InsertProtocol(
                                    new XmlClinicalProtocol(this.m_xsz.Deserialize(rStream) as ProtocolDefinition)
                                    );
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                this.m_tracer.TraceError("Error installing clinical protocols: {0}", e);
            }
        }

    }
}