/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Cdss.Xml.Antlr;
using SanteDB.Cdss.Xml.Model;
using SanteDB.Core.Cdss;
using SanteDB.Core.Model.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SanteDB.Cdss.Xml.Test
{
    /// <summary>
    /// CDSS library repository
    /// </summary>
    public class TestCdssLibraryPersistenceService : ICdssLibraryRepository
    {
        private readonly List<ICdssLibrary> m_libraries;

        public TestCdssLibraryPersistenceService()
        {
            var asm = typeof(TestCdssLibraryPersistenceService).Assembly;
            this.m_libraries = asm.GetManifestResourceNames()
                .Where(o => o.StartsWith("SanteDB.Cdss.Xml.Test.Protocols"))
                .Select(o =>
                {
                    try
                    {
                        using (var ms = asm.GetManifestResourceStream(o))
                        {
                            if (o.EndsWith("xml"))
                            {
                                return new XmlProtocolLibrary(CdssLibraryDefinition.Load(ms)) as ICdssLibrary;
                            }
                            else if(o.EndsWith("cdss"))
                            {
                                return new XmlProtocolLibrary(CdssLibraryTranspiler.Transpile(ms, true, o));
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        return null;
                    }
                })
                .OfType<ICdssLibrary>()
                .ToList();
        }

        public string ServiceName => "Test CDSS Library";

        public IQueryResultSet<ICdssLibrary> Find(Expression<Func<ICdssLibrary, bool>> filter)
            => this.m_libraries.Where(filter.Compile()).AsResultSet();

        public ICdssLibrary Get(Guid libraryUuid, Guid? version) => this.m_libraries.Find(o => o.Uuid == libraryUuid);

        public ICdssLibrary InsertOrUpdate(ICdssLibrary libraryToInsert)
        {
            throw new NotSupportedException();
        }

        public ICdssLibrary Remove(Guid libraryUuid)
        {
            throw new NotSupportedException();
        }
    }
}
