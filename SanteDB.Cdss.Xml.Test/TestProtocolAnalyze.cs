﻿/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using NUnit.Framework;
using SanteDB.Cdss.Xml.Antlr;
using SanteDB.Cdss.Xml.Diagnostics;
using SanteDB.Cdss.Xml.Model;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.TestFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
                StatusConceptKey = StatusKeys.Completed,
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
            var definition = TestUtils.Load("WeightHeight.cdss");
            // Ensure the protocol loaded properly
            Assert.IsNotNull(definition.Definitions);
            Assert.AreEqual(3, definition.Definitions.Count);
            Assert.AreEqual(2, definition.Definitions.OfType<CdssDecisionLogicBlockDefinition>().Count());
            Assert.AreEqual(1, definition.Definitions.OfType<CdssDatasetDefinition>().Count());
            Assert.AreEqual(CdssObjectState.Active, definition.Status);
            Assert.AreEqual("3.20", definition.Metadata.Version);

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
                GenderConceptKey = AdministrativeGenderConceptKeys.Female,
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE" }
            };

            // Load the weight protocol
            var definition = TestUtils.Load("WeightHeight.cdss");
            var xmlLib = new XmlProtocolLibrary(definition);

            // Run the object in regular mode
            var results = xmlLib.Execute(newborn.DeepCopy() as Patient).ToList();
            Assert.AreEqual(120, results.Count);

            // Run in debug mode
            results = xmlLib.Execute(newborn, new Dictionary<String, Object>() { { "debug", true } }).ToList();
            Assert.AreEqual(121, results.Count);
            Assert.IsTrue(results.OfType<CdssDebugSessionData>().Any());
            var dbg = results.OfType<CdssDebugSessionData>().First();
            using (var ms = new MemoryStream())
            {
                dbg.GetDiagnosticReport().Save(ms);
                var dbgInfo = Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        [Test]
        public void TestCanDebugWithSourceReferences()
        {
            // NB: 
            Patient newborn = new Patient()
            {
                Key = Guid.NewGuid(),
                DateOfBirth = DateTime.Now,
                GenderConceptKey = AdministrativeGenderConceptKeys.Female,
                GenderConcept = new Core.Model.DataTypes.Concept() { Mnemonic = "FEMALE" }
            };

            // Load the weight protocol
            var definition = TestUtils.Load("BCG.cdss");
            var xmlLib = new XmlProtocolLibrary(definition);

            // Run in debug mode
            var results = xmlLib.Execute(newborn, new Dictionary<String, Object>() { { "debug", true } }).ToList();
            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.OfType<CdssDebugSessionData>().Any());
            var dbg = results.OfType<CdssDebugSessionData>().First();
            using (var ms = new MemoryStream())
            {
                dbg.GetDiagnosticReport().Save(ms);
                var dbgInfo = Encoding.UTF8.GetString(ms.ToArray());
            }
        }
    }
}
