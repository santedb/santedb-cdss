using SanteDB.Cdss.Xml.Antlr;
using SanteDB.Cdss.Xml.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Cdss.Xml.Test
{
    internal static class TestUtils
    {

        internal static CdssLibraryDefinition Load(String logicLibraryName)
        {
            logicLibraryName = $"SanteDB.Cdss.Xml.Test.Protocols.{logicLibraryName}";
            using (var ms = typeof(TestUtils).Assembly.GetManifestResourceStream(logicLibraryName))
            {
                if (logicLibraryName.EndsWith("xml"))
                {
                    return CdssLibraryDefinition.Load(ms);
                }
                else if (logicLibraryName.EndsWith("cdss"))
                {
                    return CdssLibraryTranspiler.Transpile(ms, true, logicLibraryName);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(logicLibraryName));
                }
            }
        }
    }
}
