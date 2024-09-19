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
using NUnit.Framework;
using SanteDB.Cdss.Xml.Antlr;
using SanteDB.Cdss.Xml.Model;
using System.IO;
using System.Linq;
using System.Text;

namespace SanteDB.Cdss.Xml.Test
{
    [TestFixture]
    public class TestDoesTranspile
    {

        [Test]
        public void TestDoesTranspileValidBCGFile()
        {
            using (var stream = typeof(TestDoesTranspile).Assembly.GetManifestResourceStream("SanteDB.Cdss.Xml.Test.Protocols.BCG.cdss"))
            {
                var transpiledLibrary = CdssLibraryTranspiler.Transpile(stream, true);
                using (var ms = new MemoryStream())
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
                    Assert.AreEqual("org.santedb.cdss.weight.zscores", transpiledLibrary.Id);
                    Assert.AreEqual(9, transpiledLibrary.Definitions.OfType<CdssDecisionLogicBlockDefinition>().First().Definitions.Count());
                    Assert.AreEqual(7, transpiledLibrary.Definitions.OfType<CdssDecisionLogicBlockDefinition>().Last().Definitions.Count());
                    Assert.AreEqual(1, transpiledLibrary.Definitions.OfType<CdssDatasetDefinition>().Count());
                }

                var detranspile = CdssLibraryTranspiler.UnTranspile(transpiledLibrary);
            }
        }

    }
}
