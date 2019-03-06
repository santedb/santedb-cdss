﻿/*
 * Copyright 2015-2019 Mohawk College of Applied Arts and Technology
 * Copyright 2019-2019 SanteSuite Contributors (See NOTICE)
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
 * User: justi
 * Date: 2019-1-12
 */
using SanteDB.Cdss.Xml.Model;
using SanteDB.Core;
using SanteDB.Core.Applets.Services;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml
{
    /// <summary>
    /// Applet clinical protocol repository
    /// </summary>
    [ServiceProvider("Applet Based Clinical Protocol Repository")]
    public class AppletClinicalProtocolRepository : IClinicalProtocolRepositoryService
    {

        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "Applet Based Clinical Protocol Repository";

        // Tracer
        private Tracer m_tracer = Tracer.GetTracer(typeof(AppletClinicalProtocolRepository));

        // Protocols loaded
        private List<SanteDB.Core.Model.Acts.Protocol> m_protocols = new List<SanteDB.Core.Model.Acts.Protocol>();

        /// <summary>
        /// Clinical repository service
        /// </summary>
        public AppletClinicalProtocolRepository()
        {

        }

        /// <summary>
        /// Find the specified protocol
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SanteDB.Core.Model.Acts.Protocol> FindProtocol(Expression<Func<SanteDB.Core.Model.Acts.Protocol, bool>> predicate, int offset, int? count, out int totalResults)
        {
            if (this.m_protocols == null || this.m_protocols.Count == 0)
                this.LoadProtocols();
            var retVal = this.m_protocols.Where(predicate.Compile()).Skip(offset).Take(count ?? 100);
            totalResults = retVal.Count();
            return retVal;
        }

        /// <summary>
        /// Load protocols
        /// </summary>
        private void LoadProtocols()
        {
            try
            {
                // Get protocols from the applet
                var appletManager = ApplicationServiceContext.Current.GetService(typeof(IAppletManagerService)) as IAppletManagerService;
                var protocols = appletManager.Applets.SelectMany(o => o.Assets).Where(o => o.Name.StartsWith("protocols/"));

                foreach (var f in protocols)
                {

                    XmlSerializer xsz = new XmlSerializer(typeof(ProtocolDefinition));
                    var content = f.Content ?? appletManager.Applets.Resolver(f);
                    if (content is String)
                        using (var rStream = new StringReader(content as String))
                            this.m_protocols.Add(
                                new XmlClinicalProtocol(xsz.Deserialize(rStream) as ProtocolDefinition).GetProtocolData()
                            );
                    else if (content is byte[])
                        using (var rStream = new MemoryStream(content as byte[]))
                            this.m_protocols.Add(new XmlClinicalProtocol(ProtocolDefinition.Load(rStream)).GetProtocolData());
                    else if (content is XElement)
                        using (var rStream = (content as XElement).CreateReader())
                            this.m_protocols.Add(
                                new XmlClinicalProtocol(xsz.Deserialize(rStream) as ProtocolDefinition).GetProtocolData()
                                );
                }
            }
            catch (Exception e)
            {
                this.m_tracer.TraceError("Ich bin der roboter: Error loading protocols: {0}", e);
            }
        }

        /// <summary>
        /// Insert the specified protocol
        /// </summary>
        public SanteDB.Core.Model.Acts.Protocol InsertProtocol(SanteDB.Core.Model.Acts.Protocol data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            // Generate key
            if (data.Key == null) data.Key = Guid.NewGuid();
            data.CreationTime = DateTime.Now;

            if (!this.m_protocols.Any(o => o.Key == data.Key))
                this.m_protocols.Add(data);

            return data;
        }

    }
}
