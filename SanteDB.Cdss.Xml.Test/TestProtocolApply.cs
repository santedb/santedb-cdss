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
using Newtonsoft.Json;
using NUnit.Framework;
using SanteDB.Cdss.Xml.Model;
using SanteDB.Core;
using SanteDB.Core.Applets.ViewModel.Json;
using SanteDB.Core.Cdss;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Services;
using SanteDB.Core.TestFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SanteDB.Cdss.Xml.Test
{
    /// <summary>
    /// Tests the application of protocol
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestFixture(Category = "CDSS")]
    public class TestProtocolApply
    {

        private readonly Dictionary<String, Object> m_routineVaccParms = new Dictionary<string, object>()
        {
            {  CdssParameterNames.ENCOUNTER_SCOPE, "org.santedb.ims.pediatric.routineVacc" }
        };

        [OneTimeSetUp]
        public void Initialize()
        {
            // Force load of the DLL
            TestApplicationContext.TestAssembly = typeof(TestProtocolApply).Assembly;
            TestApplicationContext.Initialize(TestContext.CurrentContext.TestDirectory);
        }

        /// <summary>
        /// Test that the care plan schedules OPV0 at the correct time
        /// </summary>
        [Test]
        public void TestShouldScheduleOPV()
        {
            var definition = TestUtils.Load("OPV.cdss");
            var xmlCp = new XmlProtocolLibrary(definition);

            // Patient that is just born = Schedule OPV
            Patient newborn = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now,
                GenderConceptKey = AdministrativeGenderConceptKeys.Female,
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE" }
            };
            // Now apply the protocol
            var acts = xmlCp.GetProtocols(newborn, null, String.Empty).Last().ComputeProposals(newborn, new Dictionary<String, Object>()).ToArray();
            var jsonSerializer = new JsonViewModelSerializer();
            String json = jsonSerializer.Serialize(newborn);

            Assert.AreEqual(4, acts.Count());
        }

        /// <summary>
        /// Test that the care plan schedules OPV0 at the correct time
        /// </summary>
        [Test]
        public void TestShouldScheduleBCG()
        {
            var definition = TestUtils.Load("BCG.cdss");
            var xmlCp = new XmlProtocolLibrary(definition);

            // Patient that is just born = Schedule OPV
            Patient newborn = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now,
                GenderConceptKey = AdministrativeGenderConceptKeys.Female,
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE" }
            };

            // Now apply the protocol
            var acts = xmlCp.GetProtocols(newborn, null, String.Empty).Single().ComputeProposals(newborn, new Dictionary<String, Object>()).ToArray();
            var jsonSerializer = new JsonViewModelSerializer();
            String json = jsonSerializer.Serialize(newborn);
            Assert.AreEqual(1, acts.Count());
        }

        /// <summary>
        /// Test that the care plan schedules OPV0 at the correct time
        /// </summary>
        [Test]
        public void TestShouldRepeatWeight()
        {
            var definition = TestUtils.Load("WeightHeight.cdss");
            var xmlCp = new XmlProtocolLibrary(definition);

            // Patient that is just born = Schedule OPV
            Patient newborn = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now,
                GenderConceptKey = AdministrativeGenderConceptKeys.Female,
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE" }
            };

            // Now apply the protocol
            var acts = xmlCp.GetProtocols(newborn, null, String.Empty).Single().ComputeProposals(newborn, new Dictionary<String, Object>()).ToArray();
            var jsonSerializer = new JsonViewModelSerializer();
            String json = jsonSerializer.Serialize(newborn);
            Assert.AreEqual(120, acts.Count());
            Assert.AreEqual(60, acts.OfType<Act>().Count(o => o.TypeConceptKey == Guid.Parse("850ca898-c656-4ba2-a7c1-ff74e3331396"))); // 60 heights
            Assert.AreEqual(60, acts.OfType<Act>().Count(o => o.TypeConceptKey == Guid.Parse("a261f8cd-69b0-49aa-91f4-e6d3e5c612ed"))); // 60 weights

        }

        /// <summary>
        /// Test that the care plan schedules weight at the correct time
        /// </summary>
        [Test]
        public void TestShouldSkipWeight()
        {
            var definition = TestUtils.Load("WeightHeight.cdss");
            var xmlCp = new XmlProtocolLibrary(definition);

            // Patient that is just born = Schedule OPV
            Patient newborn = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now,
                GenderConceptKey = AdministrativeGenderConceptKeys.Female,
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE" },
                Participations = new List<ActParticipation>()
                {
                    new ActParticipation()
                    {
                        ParticipationRole = new Core.Model.DataTypes.Concept() { Mnemonic = "RecordTarget" },
                        Act = new QuantityObservation()
                        {
                            Value = (decimal)3.2,
                            TypeConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "VitalSign-Weight" },
                            ActTime = DateTime.Now
                        }
                    },
                    new ActParticipation()
                    {
                        ParticipationRole = new Core.Model.DataTypes.Concept() { Mnemonic = "RecordTarget" },
                        Act = new PatientEncounter()
                        {
                            ActTime = DateTime.Now
                        }
                    }
                }
            };

            // Now apply the protocol
            var acts = xmlCp.GetProtocols(newborn, null, String.Empty).Single().ComputeProposals(newborn, new Dictionary<String, Object>()).ToArray();
            var jsonSerializer = new JsonViewModelSerializer();
            String json = jsonSerializer.Serialize(newborn);
            Assert.AreEqual(119, acts.Count());
        }

        /// <summary>
        /// Test that the care plan schedules OPV0 at the correct time
        /// </summary>
        [Test]
        public void TestShouldScheduleMR()
        {
            var definition = TestUtils.Load("MMR.cdss");
            var xmlCp = new XmlProtocolLibrary(definition);

            // Patient that is just born = Schedule OPV
            Patient newborn = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now,
                GenderConceptKey = AdministrativeGenderConceptKeys.Female,
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE" }
            };

            // Now apply the protocol
            var acts = xmlCp.GetProtocols(newborn, null, String.Empty).SelectMany(p => p.ComputeProposals(newborn, new Dictionary<String, Object>())).OfType<Act>().ToArray();
            Console.WriteLine(JsonConvert.SerializeObject(acts));
            Assert.AreEqual(2, acts.Count());

            // The default schedule should have been applied
            Assert.Greater(acts.First().ActTime.Value.DateTime, newborn.DateOfBirth.Value.AddDays(270));
            Assert.Greater(acts.Last().ActTime.Value.DateTime, newborn.DateOfBirth.Value.AddMonths(17));
            Assert.AreEqual("1.3.5.1.4.1.52820.5.3.2.3", acts.First().Protocols.First().Protocol.Oid);

            // Apply the protocol - we should get the accelerated protocol
            newborn.DateOfBirth = DateTime.Now.AddMonths(-18).AddDays(-1);
            acts = xmlCp.GetProtocols(newborn, null, String.Empty).SelectMany(p => p.ComputeProposals(newborn, new Dictionary<String, Object>())).OfType<Act>().ToArray();
            Assert.AreEqual(2, acts.Count());

            // The accelerated schedule should have been applied
            Assert.AreEqual(acts.First().ActTime.Value.DateTime.Date, DateTime.Now.Date);
            Assert.GreaterOrEqual(acts.Last().ActTime.Value.DateTime.Date, DateTime.Now.Date.AddMonths(1));
            Assert.AreEqual("1.3.5.1.4.1.52820.5.3.2.3.1", acts.First().Protocols.First().Protocol.Oid);

        }


        /// <summary>
        /// Test that the care plan schedules OPV0 at the correct time
        /// </summary>
        [Test]
        public void TestShouldSchedulePCV()
        {
            var definition = TestUtils.Load("PCV13.cdss");
            var xmlCp = new XmlProtocolLibrary(definition);

            // Patient that is just born = Schedule OPV
            Patient newborn = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now,
                GenderConceptKey = AdministrativeGenderConceptKeys.Female,
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE" }
            };

            // Now apply the protocol
            var acts = xmlCp.GetProtocols(newborn, null, String.Empty).Single().ComputeProposals(newborn, new Dictionary<String, Object>());
            var jsonSerializer = new JsonViewModelSerializer();
            Console.WriteLine(JsonConvert.SerializeObject(acts));
            
            Assert.AreEqual(3, acts.Count());
        }

        /// <summary>
        /// Test that the care plan schedules OPV0 at the correct time
        /// </summary>
        [Test]
        public void TestShouldScheduleDTP()
        {
            var definition = TestUtils.Load("DTP.cdss");
            var xmlCp = new XmlProtocolLibrary(definition);

            // Patient that is just born = Schedule OPV
            Patient newborn = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now,
                GenderConceptKey = AdministrativeGenderConceptKeys.Female,
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE" }
            };

            // Now apply the protocol
            var acts = xmlCp.GetProtocols(newborn, null, String.Empty).Single().ComputeProposals(newborn, new Dictionary<String, Object>()
            {
                {  CdssParameterNames.DEBUG_MODE, true }
            }).ToArray();
            Console.WriteLine(JsonConvert.SerializeObject(acts));
            Assert.AreEqual(3, acts.OfType<Act>().Count());
        }

        /// <summary>
        /// Test that the care plan schedules OPV0 at the correct time
        /// </summary>
        [Test]
        public void TestShouldScheduleRota()
        {
            var definition = TestUtils.Load("Rota.cdss");
            var xmlCp = new XmlProtocolLibrary(definition);

            // Patient that is just born = Schedule OPV
            Patient newborn = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now,
                GenderConceptKey = AdministrativeGenderConceptKeys.Female,
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE" }
            };

            // Now apply the protocol
            var acts = xmlCp.GetProtocols(newborn, null, String.Empty).Single().ComputeProposals(newborn, new Dictionary<String, Object>());
            Console.WriteLine(JsonConvert.SerializeObject(acts));
            Assert.AreEqual(2, acts.Count());
        }

        /// <summary>
        /// Should schedule all vaccines
        /// </summary>
        [Test]
        public void ShouldHandlePartials()
        {
            var scp = ApplicationServiceContext.Current.GetService<IDecisionSupportService>();
            // Patient that is just born = Schedule OPV
            Patient newborn = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now.AddDays(-10),
                GenderConceptKey = AdministrativeGenderConceptKeys.Female,

                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE", Key = AdministrativeGenderConceptKeys.Female }
            };

            // Now apply the protocol
            var acts = scp.CreateCarePlan(newborn, false, m_routineVaccParms);
            Console.WriteLine(JsonConvert.SerializeObject(acts));
            
            var c = acts.LoadCollection(o => o.Relationships).Select(o => o.TargetAct).GroupBy(o => o.Protocols.First().Protocol.Name).ToDictionary(o => o.Key, o => o.Count());

            Assert.AreEqual(146, acts.LoadCollection(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Select(o => o.LoadProperty(r => r.TargetAct)).Count());
            Assert.IsFalse(acts.LoadCollection(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Select(o => o.LoadProperty(r => r.TargetAct)).Any(o => o.Protocols.Count() > 1));
            Assert.AreEqual(26, acts.LoadCollection(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Where(o => !(o.LoadProperty(r => r.TargetAct) is QuantityObservation)).Count());
            Assert.IsFalse(acts.LoadCollection(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Select(o => o.LoadProperty(r => r.TargetAct)).Any(o => !o.Participations.Any(p => p.ParticipationRoleKey == ActParticipationKeys.RecordTarget)));

            // Male - should not have HPV
            // Patient that is just born = Schedule OPV
            newborn = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now.AddDays(-10),
                GenderConceptKey = AdministrativeGenderConceptKeys.Male,
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "MALE", Key = AdministrativeGenderConceptKeys.Male }
            };

            // Now apply the protocol
            acts = scp.CreateCarePlan(newborn, false, m_routineVaccParms);
            Assert.AreEqual(144, acts.LoadCollection(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Select(o => o.LoadProperty(r => r.TargetAct)).Count());
            Assert.IsFalse(acts.LoadCollection(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Select(o => o.LoadProperty(r => r.TargetAct)).Any(o => o.Protocols.Count() > 1));
            Assert.AreEqual(24, acts.LoadCollection(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Where(o => !(o.LoadProperty(r => r.TargetAct) is QuantityObservation)).Count());
            Assert.IsFalse(acts.LoadCollection(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Select(o => o.LoadProperty(r => r.TargetAct)).Any(o => !o.Participations.Any(p => p.ParticipationRoleKey == ActParticipationKeys.RecordTarget)));


        }

        /// <summary>
        /// Should schedule all vaccines
        /// </summary>
        [Test]
        public void ShouldExcludeAdults()
        {
            var scp = ApplicationServiceContext.Current.GetService<IDecisionSupportService>();
            // Patient that is just born = Schedule OPV
            Patient adult = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now.AddMonths(-240),
                GenderConceptKey = AdministrativeGenderConceptKeys.Female,
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE" }
            };

            // Now apply the protocol
            var acts = scp.CreateCarePlan(adult, false, m_routineVaccParms);
            Console.WriteLine(JsonConvert.SerializeObject(acts));
            Assert.AreEqual(0, acts.LoadCollection(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Select(o => o.LoadProperty(r => r.TargetAct)).Count());
        }

        /// <summary>
        /// Should schedule all vaccines
        /// </summary>
        [Test]
        public void ShouldScheduleAll()
        {
            var scp = ApplicationServiceContext.Current.GetService<IDecisionSupportService>();
            // Patient that is just born = Schedule OPV
            Patient newborn = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now,
                GenderConceptKey = AdministrativeGenderConceptKeys.Female,
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE" }
            };

            // Now apply the protocol
            var acts = scp.CreateCarePlan(newborn, false, m_routineVaccParms);
            var jsonSerializer = new JsonViewModelSerializer();
            var c = acts.LoadCollection(o => o.Relationships).Select(o => o.TargetAct).GroupBy(o => o.Protocols.First().Protocol.Name).ToDictionary(o => o.Key, o => o.Count());
            String json = jsonSerializer.Serialize(newborn);
            Assert.AreEqual(146, acts.LoadCollection(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Select(o => o.LoadProperty(r => r.TargetAct)).Count());
            Assert.IsFalse(acts.LoadCollection(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Select(o => o.LoadProperty(r => r.TargetAct)).Any(o => o.Protocols.Count() > 1));
        }


        /// <summary>
        /// Tests the periodic hull where there is overlap between a late/delayed action and original timing
        /// </summary>
        [Test]
        public void TestShouldGroupPeriodicHull()
        {
            var scp = ApplicationServiceContext.Current.GetService<IDecisionSupportService>();
            // Patient that is just born = Schedule OPV
            Patient patient = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Parse("2024-08-30"),
                GenderConceptKey = AdministrativeGenderConceptKeys.Female,
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE" }
            };

            // Now apply the protocol
            var acts = scp.CreateCarePlan(patient, true, new Dictionary<String, Object>()
            {
                { CdssParameterNames.PERSISTENT_OUTPUT, true },
                { CdssParameterNames.NON_INTERACTIVE, false },
                {CdssParameterNames.EXCLUDE_OBSERVATIONS, true },
                {CdssParameterNames.ENCOUNTER_SCOPE, "org.santedb.ims.pediatric.routineVacc" }
            });


            // Emit the care plan
            foreach(var enc in 
                acts.Relationships
                    .Where(r=>r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent)
                    .Select(r=>r.TargetAct))
            {
                Console.WriteLine("{0} : {1:yyyy-MM-dd} - {2:yyyy-MM-dd}", enc.Type, enc.StartTime, enc.StopTime);
                foreach(var rl in enc.Relationships.Where(r=>r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Select(e=>e.TargetAct))
                {
                    if (rl is SubstanceAdministration adm)
                    {
                        Console.WriteLine("\t{0} #{1} : R:{2:yyyy-MM-dd}, RG: {3:yyyy-MM-dd} - {4:yyyy-MM-dd}", rl.Protocols.First().Protocol.Name, adm.SequenceId, rl.ActTime, rl.StartTime, rl.StopTime);
                    }
                    else
                    {
                        Console.WriteLine("\t{0} : R:{1:yyyy-MM-dd}, RG: {2:yyyy-MM-dd} - {3:yyyy-MM-dd}", rl.TypeConcept.ToDisplay(), rl.ActTime, rl.StartTime, rl.StopTime);
                    }
                }
            }

            Assert.AreEqual(10, acts.Relationships.Count());

        }

        /// <summary>
        /// Should group into appointments
        /// </summary>
        [Test]
        public void ShouldScheduleAppointments()
        {
            var scp = ApplicationServiceContext.Current.GetService<IDecisionSupportService>();
            // Patient that is just born = Schedule OPV
            Patient newborn = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now,
                GenderConceptKey = AdministrativeGenderConceptKeys.Female,
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE" }
            };

            // Now apply the protocol
            var acts = scp.CreateCarePlan(newborn, true);
            var jsonSerializer = new JsonViewModelSerializer();
            string json = jsonSerializer.Serialize(newborn);
            //Assert.GreaterOrEqual(60, acts.LoadCollection(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Select(o => o.LoadProperty(r => r.TargetAct)).Count());
            Assert.GreaterOrEqual(acts.LoadCollection(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Select(o => o.LoadProperty(r => r.TargetAct)).Count(), 60);
            Assert.IsFalse(acts.LoadCollection(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Select(o => o.LoadProperty(r => r.TargetAct)).Any(o => !o.Protocols.IsNullOrEmpty()));
        }

        [Test]
        public void TestShouldNotModifyOriginal()
        {
            var scp = ApplicationServiceContext.Current.GetService<IDecisionSupportService>();

            // Patient that is just born = Schedule OPV
            Patient newborn = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now,
                GenderConceptKey = AdministrativeGenderConceptKeys.Female,
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE" },
                Participations = new List<ActParticipation>()
                {
                    new ActParticipation()
                    {
                        ParticipationRole = new Core.Model.DataTypes.Concept() { Mnemonic = "RecordTarget" },
                        Act = new QuantityObservation()
                        {
                            Value = (decimal)3.2,
                            TypeConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "VitalSign-Weight" },
                            ActTime = DateTime.Now
                        }
                    },
                    new ActParticipation()
                    {
                        ParticipationRole = new Core.Model.DataTypes.Concept() { Mnemonic = "RecordTarget" },
                        Act = new PatientEncounter()
                        {
                            ActTime = DateTime.Now
                        }
                    }
                }
            };

            // Now apply the protocol
            var acts = scp.CreateCarePlan(newborn, true);
            var jsonSerializer = new JsonViewModelSerializer();
            string json = jsonSerializer.Serialize(newborn);
            Assert.AreEqual(2, newborn.Participations.Count);
            //Assert.GreaterOrEqual(60, acts.LoadCollection(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Select(o => o.LoadProperty(r => r.TargetAct)).Count());
            Assert.GreaterOrEqual(acts.LoadCollection(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Select(o => o.LoadProperty(r => r.TargetAct)).Count(), 60);
        }
    }

}