using NUnit.Framework;
using SanteDB.Cdss.Xml.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.TestFramework;
using System;
using System.Collections.Generic;
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
            var objectUnderTest = new QuantityObservation()
            {
                TypeConceptKey = Guid.Parse("a261f8cd-69b0-49aa-91f4-e6d3e5c612ed"), // Weight
                UnitOfMeasureKey = Guid.Parse("49974584-7c48-457e-a79c-031cdd62714a"), // lbs_i
                Value = (decimal)8.52,
                Participations = new List<ActParticipation>()
                {
                    new ActParticipation()
                    {
                        ParticipationRoleKey = ActParticipationKeys.RecordTarget,
                        PlayerEntity = new Patient()
                        {
                            DateOfBirth = DateTime.Now.AddDays(-6),
                            GenderConceptKey = AdministrativeGenderConceptKeys.Male
                        }
                    }
                }
            };

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
            var issues = xmlProto.Analyze(objectUnderTest);
        }


    }
}
