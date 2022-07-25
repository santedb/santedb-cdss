/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-5
 */

using NUnit.Framework;
using SanteDB.Cdss.Xml.Model;
using SanteDB.Core;
using SanteDB.Core.Model;
using SanteDB.Core.Applets.ViewModel.Json;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Protocol;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using SanteDB.Core.Model.Query;

namespace SanteDB.Cdss.Xml.Test
{
    /// <summary>
    /// Tests the application of protocol
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestFixture(Category = "CDSS")]
    public class TestProtocolApply : IServiceProvider, IApplicationServiceContext
    {
        public bool IsRunning => true;

        public OperatingSystemID OperatingSystem => OperatingSystemID.Win32;

        public SanteDBHostType HostType => SanteDBHostType.Other;

        public DateTime StartTime => DateTime.Now;

        public event EventHandler Starting;

        public event EventHandler Started;

        public event EventHandler Stopping;

        public event EventHandler Stopped;

        /// <summary>
        /// Test that the care plan schedules OPV0 at the correct time
        /// </summary>
        [Test]
        public void TestShouldScheduleOPV()
        {
            ProtocolDefinition definition = ProtocolDefinition.Load(typeof(TestProtocolApply).Assembly.GetManifestResourceStream("SanteDB.Cdss.Xml.Test.Protocols.OralPolioVaccine.xml"));
            XmlClinicalProtocol xmlCp = new XmlClinicalProtocol(definition);

            // Patient that is just born = Schedule OPV
            Patient newborn = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now,
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE" }
            };
            // Now apply the protocol
            var acts = xmlCp.Calculate(newborn, new Dictionary<String, Object>());
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
            ProtocolDefinition definition = ProtocolDefinition.Load(typeof(TestProtocolApply).Assembly.GetManifestResourceStream("SanteDB.Cdss.Xml.Test.Protocols.BcgVaccine.xml"));
            XmlClinicalProtocol xmlCp = new XmlClinicalProtocol(definition);

            // Patient that is just born = Schedule OPV
            Patient newborn = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now,
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE" }
            };

            // Now apply the protocol
            var acts = xmlCp.Calculate(newborn, null);
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
            ProtocolDefinition definition = ProtocolDefinition.Load(typeof(TestProtocolApply).Assembly.GetManifestResourceStream("SanteDB.Cdss.Xml.Test.Protocols.Weight.xml"));
            XmlClinicalProtocol xmlCp = new XmlClinicalProtocol(definition);

            // Patient that is just born = Schedule OPV
            Patient newborn = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now,
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE" }
            };

            // Now apply the protocol
            var acts = xmlCp.Calculate(newborn, null);
            var jsonSerializer = new JsonViewModelSerializer();
            String json = jsonSerializer.Serialize(newborn);
            Assert.AreEqual(60, acts.Count());
        }

        /// <summary>
        /// Test that the care plan schedules weight at the correct time
        /// </summary>
        [Test]
        public void TestShouldSkipWeight()
        {
            ProtocolDefinition definition = ProtocolDefinition.Load(typeof(TestProtocolApply).Assembly.GetManifestResourceStream("SanteDB.Cdss.Xml.Test.Protocols.Weight.xml"));
            XmlClinicalProtocol xmlCp = new XmlClinicalProtocol(definition);

            // Patient that is just born = Schedule OPV
            Patient newborn = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now,
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
            var acts = xmlCp.Calculate(newborn, null);
            var jsonSerializer = new JsonViewModelSerializer();
            String json = jsonSerializer.Serialize(newborn);
            Assert.AreEqual(59, acts.Count());
        }

        /// <summary>
        /// Test that the care plan schedules OPV0 at the correct time
        /// </summary>
        [Test]
        public void TestShouldScheduleMR()
        {
            ProtocolDefinition definition = ProtocolDefinition.Load(typeof(TestProtocolApply).Assembly.GetManifestResourceStream("SanteDB.Cdss.Xml.Test.Protocols.MeaslesRubellaVaccine.xml"));
            XmlClinicalProtocol xmlCp = new XmlClinicalProtocol(definition);

            // Patient that is just born = Schedule OPV
            Patient newborn = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now,
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE" }
            };

            // Now apply the protocol
            var acts = xmlCp.Calculate(newborn, null);
            var jsonSerializer = new JsonViewModelSerializer();
            String json = jsonSerializer.Serialize(newborn);
            Assert.AreEqual(2, acts.Count());
        }

        /// <summary>
        /// Test that the care plan schedules OPV0 at the correct time
        /// </summary>
        [Test]
        public void TestShouldSchedulePCV()
        {
            ProtocolDefinition definition = ProtocolDefinition.Load(typeof(TestProtocolApply).Assembly.GetManifestResourceStream("SanteDB.Cdss.Xml.Test.Protocols.PCV13Vaccine.xml"));
            XmlClinicalProtocol xmlCp = new XmlClinicalProtocol(definition);

            // Patient that is just born = Schedule OPV
            Patient newborn = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now,
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE" }
            };

            // Now apply the protocol
            var acts = xmlCp.Calculate(newborn, null);
            var jsonSerializer = new JsonViewModelSerializer();
            String json = jsonSerializer.Serialize(newborn);
            Assert.AreEqual(3, acts.Count());
        }

        /// <summary>
        /// Test that the care plan schedules OPV0 at the correct time
        /// </summary>
        [Test]
        public void TestShouldScheduleDTP()
        {
            ProtocolDefinition definition = ProtocolDefinition.Load(typeof(TestProtocolApply).Assembly.GetManifestResourceStream("SanteDB.Cdss.Xml.Test.Protocols.DTP-HepB-HibTrivalent.xml"));
            XmlClinicalProtocol xmlCp = new XmlClinicalProtocol(definition);

            // Patient that is just born = Schedule OPV
            Patient newborn = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now,
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE" }
            };

            // Now apply the protocol
            var acts = xmlCp.Calculate(newborn, null);
            var jsonSerializer = new JsonViewModelSerializer();
            String json = jsonSerializer.Serialize(newborn);
            Assert.AreEqual(3, acts.Count());
        }

        /// <summary>
        /// Test that the care plan schedules OPV0 at the correct time
        /// </summary>
        [Test]
        public void TestShouldScheduleRota()
        {
            ProtocolDefinition definition = ProtocolDefinition.Load(typeof(TestProtocolApply).Assembly.GetManifestResourceStream("SanteDB.Cdss.Xml.Test.Protocols.RotaVaccine.xml"));
            XmlClinicalProtocol xmlCp = new XmlClinicalProtocol(definition);

            // Patient that is just born = Schedule OPV
            Patient newborn = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now,
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE" }
            };

            // Now apply the protocol
            var acts = xmlCp.Calculate(newborn, null);
            var jsonSerializer = new JsonViewModelSerializer();
            String json = jsonSerializer.Serialize(newborn);
            Assert.AreEqual(2, acts.Count());
        }

        /// <summary>
        /// Should schedule all vaccines
        /// </summary>
        [Test]
        public void ShouldHandlePartials()
        {
            SimpleCarePlanService scp = new SimpleCarePlanService(new DummyProtocolRepository());
            ApplicationServiceContext.Current = this;
            // Patient that is just born = Schedule OPV
            Patient newborn = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now,
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE" }
            };

            // Now apply the protocol
            var acts = scp.CreateCarePlan(newborn);
            var jsonSerializer = new JsonViewModelSerializer();
            String json = jsonSerializer.Serialize(newborn);
            Assert.AreEqual(83, acts.LoadCollection(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Select(o => o.LoadProperty(r => r.TargetAct)).Count());
            Assert.IsFalse(acts.LoadCollection(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Select(o => o.LoadProperty(r => r.TargetAct)).Any(o => o.Protocols.Count() > 1));
            acts = scp.CreateCarePlan(newborn);
            //Assert.AreEqual(60, acts.LoadCollection(o=>o.Relationships).Where(r=>r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Select(o=>o.LoadProperty(r=>r.TargetAct)).Count());
            Assert.AreEqual(23, acts.LoadCollection(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Where(o => !(o.LoadProperty(r => r.TargetAct) is QuantityObservation)).Count());
            acts = scp.CreateCarePlan(newborn);
            //Assert.AreEqual(60, acts.LoadCollection(o=>o.Relationships).Where(r=>r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Select(o=>o.LoadProperty(r=>r.TargetAct)).Count());
            Assert.AreEqual(83, acts.LoadCollection(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Select(o => o.LoadProperty(r => r.TargetAct)).Count());
            Assert.IsFalse(acts.LoadCollection(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Select(o => o.LoadProperty(r => r.TargetAct)).Any(o => !o.Participations.Any(p => p.ParticipationRoleKey == ActParticipationKeys.RecordTarget)));
        }

        /// <summary>
        /// Should schedule all vaccines
        /// </summary>
        [Test]
        public void ShouldExcludeAdults()
        {
            SimpleCarePlanService scp = new SimpleCarePlanService(new DummyProtocolRepository());
            ApplicationServiceContext.Current = this;
            // Patient that is just born = Schedule OPV
            Patient adult = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now.AddMonths(-240),
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE" }
            };

            // Now apply the protocol
            var acts = scp.CreateCarePlan(adult);
            var jsonSerializer = new JsonViewModelSerializer();
            String json = jsonSerializer.Serialize(adult);
            Assert.AreEqual(0, acts.LoadCollection(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Select(o => o.LoadProperty(r => r.TargetAct)).Count());
        }

        /// <summary>
        /// Should schedule all vaccines
        /// </summary>
        [Test]
        public void ShouldScheduleAll()
        {
            SimpleCarePlanService scp = new SimpleCarePlanService(new DummyProtocolRepository());
            ApplicationServiceContext.Current = this;
            // Patient that is just born = Schedule OPV
            Patient newborn = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now,
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE" }
            };

            // Now apply the protocol
            var acts = scp.CreateCarePlan(newborn);
            var jsonSerializer = new JsonViewModelSerializer();
            String json = jsonSerializer.Serialize(newborn);
            Assert.AreEqual(83, acts.LoadCollection(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Select(o => o.LoadProperty(r => r.TargetAct)).Count());
            Assert.IsFalse(acts.LoadCollection(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Select(o => o.LoadProperty(r => r.TargetAct)).Any(o => o.Protocols.Count() > 1));
        }

        /// <summary>
        /// Should group into appointments
        /// </summary>
        [Test]
        public void ShouldScheduleAppointments()
        {
            SimpleCarePlanService scp = new SimpleCarePlanService(new DummyProtocolRepository());
            ApplicationServiceContext.Current = this;
            // Patient that is just born = Schedule OPV
            Patient newborn = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now,
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE" }
            };

            // Now apply the protocol
            var acts = scp.CreateCarePlan(newborn, true);
            var jsonSerializer = new JsonViewModelSerializer();
            string json = jsonSerializer.Serialize(newborn);
            Assert.AreEqual(60, acts.LoadCollection(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Select(o => o.LoadProperty(r => r.TargetAct)).Count());
            Assert.IsFalse(acts.LoadCollection(o => o.Relationships).Where(r => r.RelationshipTypeKey == ActRelationshipTypeKeys.HasComponent).Select(o => o.LoadProperty(r => r.TargetAct)).Any(o => o.Protocols.Count() > 1));
        }

        /// <summary>
        /// Get service
        /// </summary>
        public object GetService(Type serviceType)
        {
            return new DummyProtocolRepository();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Dummy clinical repository
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class DummyProtocolRepository : IClinicalProtocolRepositoryService
    {
        public String ServiceName => "Fake Repository";

        [Obsolete("Use FindProtocol()", true)]
        public IEnumerable<Core.Model.Acts.Protocol> FindProtocol(Expression<Func<Core.Model.Acts.Protocol, bool>> predicate, int offset, int? count, out int totalResults)
        {
            List<Core.Model.Acts.Protocol> retVal = new List<Core.Model.Acts.Protocol>();

            foreach (var i in typeof(DummyProtocolRepository).Assembly.GetManifestResourceNames())
                if (i.EndsWith(".xml"))
                {
                    ProtocolDefinition definition = ProtocolDefinition.Load(typeof(TestProtocolApply).Assembly.GetManifestResourceStream(i));
                    retVal.Add(new XmlClinicalProtocol(definition).GetProtocolData());
                }
            totalResults = retVal.Count;
            return retVal;
        }

        public IQueryResultSet<IClinicalProtocol> FindProtocol(String name = null, String oid = null)
        {
            return new MemoryQueryResultSet<IClinicalProtocol>(typeof(DummyProtocolRepository).Assembly.GetManifestResourceNames().Select(i =>
            {
                if (i.EndsWith(".xml"))
                {
                    ProtocolDefinition definition = ProtocolDefinition.Load(typeof(TestProtocolApply).Assembly.GetManifestResourceStream(i));
                    return new XmlClinicalProtocol(definition);
                }
                else
                    return null;
            })).Where(predicate);
        }

        public IClinicalProtocol GetProtocol(Guid protocolUuid) => this.FindProtocol(o => o.Id == protocolUuid).First();

        public Core.Model.Acts.Protocol InsertProtocol(Core.Model.Acts.Protocol data)
        {
            throw new NotImplementedException();
        }

        public IClinicalProtocol InsertProtocol(IClinicalProtocol data)
        {
            throw new NotImplementedException();
        }
    }
}