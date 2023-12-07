using DynamicExpresso;
using NUnit.Framework;
using SanteDB.Cdss.Xml.Diagnostics;
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Cdss.Xml.Model;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.TestFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Cdss.Xml.Test
{
    /// <summary>
    /// Tests that a protocol library can analyse
    /// </summary>
    [TestFixture]
    public class TestProtocolAnalyze
    {

        [OneTimeSetUp]
        public void Initialize()
        {
            // Force load of the DLL
            TestApplicationContext.TestAssembly = typeof(TestProtocolApply).Assembly;
            TestApplicationContext.Initialize(TestContext.CurrentContext.TestDirectory);


        }

        /// <summary>
        /// Tests that a simple wieght can be analyzed
        /// </summary>
        [Test]
        public void TestCanAnalyzeSimpleWeight()
        {
            // NB: 
            var originalObject = new QuantityObservation()
            {
                TypeConceptKey = Guid.Parse("a261f8cd-69b0-49aa-91f4-e6d3e5c612ed"), // Weight
                UnitOfMeasureKey = Guid.Parse("49974584-7c48-457e-a79c-031cdd62714a"), // lbs_i
                Value = (decimal)5.52,
                Participations = new List<ActParticipation>()
                {
                    new ActParticipation()
                    {
                        ParticipationRoleKey = ActParticipationKeys.RecordTarget,
                        PlayerEntity = new Patient()
                        {
                            DateOfBirth = DateTime.Now.AddDays(-21),
                            GenderConceptKey = AdministrativeGenderConceptKeys.Male
                        }
                    }
                }
            };
            var objectUnderTest = originalObject.DeepCopy() as QuantityObservation;


            // Load the weight protocol
            var definition = CdssLibraryDefinition.Load(typeof(TestProtocolApply).Assembly.GetManifestResourceStream("SanteDB.Cdss.Xml.Test.Protocols.Weight.xml"));
            // Ensure the protocol loaded properly
            Assert.IsNotNull(definition.Definitions);
            Assert.AreEqual(3, definition.Definitions.Count);
            Assert.AreEqual(2, definition.Definitions.OfType<CdssDecisionLogicBlockDefinition>().Count());
            Assert.AreEqual(1, definition.Definitions.OfType<CdssDatasetDefinition>().Count());
            Assert.AreEqual(CdssObjectState.Active, definition.Status);
            Assert.AreEqual("3.0", definition.Metadata.Version);

            var xmlProto = new XmlProtocolLibrary(definition);
            // No interpretation
            Assert.IsNull(objectUnderTest.InterpretationConceptKey);

            var issues = xmlProto.Analyze(objectUnderTest);
            Assert.IsNotNull(objectUnderTest.InterpretationConceptKey); // Rule has set the interpretation concept
            Assert.AreEqual(1, issues.Count()); // Rule has detected an issue
            Assert.AreEqual(5.52, objectUnderTest.Value); // Rule has not changed value (only conversions for facts - don't leak into object)
            xmlProto.Analyze(originalObject.DeepCopy() as IdentifiedData);


        }

        [Test]
        public void TestCanExecuteProposal()
        {
            // NB: 
            Patient newborn = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now,
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE" }
            };

            // Load the weight protocol
            var definition = CdssLibraryDefinition.Load(typeof(TestProtocolApply).Assembly.GetManifestResourceStream("SanteDB.Cdss.Xml.Test.Protocols.Weight.xml"));
            var xmlLib = new XmlProtocolLibrary(definition);

            // Run the object in regular mode
            var results = xmlLib.Execute(newborn.DeepCopy() as Patient).ToList();
            Assert.AreEqual(120, results.Count);

            // Run in debug mode
            results = xmlLib.Execute(newborn, new Dictionary<String, Object>() { { "debug", true } }).ToList();
            Assert.AreEqual(121, results.Count);
            Assert.IsTrue(results.OfType<CdssDebugSessionData>().Any());
            var dbg = results.OfType<CdssDebugSessionData>().First();
            using(var ms = new MemoryStream())
            {
                dbg.GetDiagnosticReport().Save(ms);
                var dbgInfo = Encoding.UTF8.GetString(ms.ToArray());
            }
        }
    }
}
