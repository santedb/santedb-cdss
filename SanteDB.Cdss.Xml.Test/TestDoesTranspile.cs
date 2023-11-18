using NUnit.Framework;
using SanteDB.Cdss.Xml.Antlr;
using SanteDB.Cdss.Xml.Model;
using SanteDB.Core.Cdss;
using SanteDB.Core.Model.Roles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Cdss.Xml.Test
{
    [TestFixture]
    public class TestDoesTranspile
    {

        [Test]
        public void TestDoesTranspileValidBCGFile()
        {
            using(var stream = typeof(TestDoesTranspile).Assembly.GetManifestResourceStream("SanteDB.Cdss.Xml.Test.Protocols.BCG.cdss"))
            {
                var transpiledLibrary = CdssLibraryTranspiler.Transpile(stream, true);
                using(var ms = new MemoryStream())
                {
                    transpiledLibrary.Save(ms);
                    var libXml = Encoding.UTF8.GetString(ms.ToArray());
                    Assert.AreEqual(1, transpiledLibrary.Definitions.Count());
                    Assert.AreEqual("org.santedb.cdss.vaccine.bcg", transpiledLibrary.Id);
                    Assert.AreEqual(4, transpiledLibrary.Definitions.OfType<CdssDecisionLogicBlockDefinition>().First().Definitions.Count());
                }
            }
        }

        [Test]
        public void TestDoesTranspileValidWeightFile()
        {
            using (var stream = typeof(TestDoesTranspile).Assembly.GetManifestResourceStream("SanteDB.Cdss.Xml.Test.Protocols.Weight.cdss"))
            {
                var transpiledLibrary = CdssLibraryTranspiler.Transpile(stream, true);
                using (var ms = new MemoryStream())
                {
                    transpiledLibrary.Save(ms);
                    var libXml = Encoding.UTF8.GetString(ms.ToArray());
                    Assert.AreEqual(3, transpiledLibrary.Definitions.Count());
                    Assert.AreEqual("org.santedb.cdss.growth", transpiledLibrary.Id);
                    Assert.AreEqual(9, transpiledLibrary.Definitions.OfType<CdssDecisionLogicBlockDefinition>().First().Definitions.Count());
                    Assert.AreEqual(7, transpiledLibrary.Definitions.OfType<CdssDecisionLogicBlockDefinition>().Last().Definitions.Count());
                    Assert.AreEqual(1, transpiledLibrary.Definitions.OfType<CdssDatasetDefinition>().Count());
                }
            }
        }

    }
}
