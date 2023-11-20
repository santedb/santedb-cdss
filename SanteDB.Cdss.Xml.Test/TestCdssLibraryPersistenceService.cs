using SanteDB.Cdss.Xml.Model;
using SanteDB.Core.Cdss;
using SanteDB.Core.Model.Query;
using SharpCompress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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
                            return new XmlProtocolLibrary(CdssLibraryDefinition.Load(ms)) as ICdssLibrary;
                        }
                    }
                    catch (Exception e)
                    {
                        return null;
                    }
                })
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
