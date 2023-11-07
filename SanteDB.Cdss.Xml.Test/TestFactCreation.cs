/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-5-19
 */
using Microsoft.CSharp;
using NUnit.Framework;
using SanteDB.Cdss.Xml.Model;
using SanteDB.Cdss.Xml.Model.Assets;
using SanteDB.Cdss.Xml.Model.Expressions;
using SanteDB.Cdss.Xml.XmlLinq;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Roles;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace SanteDB.Cdss.Xml.Test
{
    [ExcludeFromCodeCoverage]
    [TestFixture(Category = "CDSS")]
    public class TestFactCreation
    {
        /// <summary>
        /// Test patient 
        /// </summary>
        private Patient m_patientUnderTest = new Patient()
        {
            Key = Guid.NewGuid(),
            VersionKey = Guid.NewGuid(),
            VersionSequence = 1,
            CreatedBy = new Core.Model.Security.SecurityProvenance()
            {
                User = new Core.Model.Security.SecurityUser()
                {
                    Key = Guid.NewGuid(),
                    UserName = "bob",
                    SecurityHash = Guid.NewGuid().ToString(),
                    Email = "bob@bob.com",
                    InvalidLoginAttempts = 2,
                    UserClass = ActorTypeKeys.HumanUser
                }
            },
            StatusConceptKey = StatusKeys.Active,
            Names = new List<EntityName>()
                {
                    new EntityName(NameUseKeys.Legal, "Johnson", "William")
                },
            Addresses = new List<EntityAddress>()
                {
                    new EntityAddress(AddressUseKeys.HomeAddress, "123 Main Street West", "Hamilton", "ON", "CA", "L8K5N2")
                },
            Identifiers = new List<EntityIdentifier>()
                {
                    new EntityIdentifier(new IdentityDomain() { Name = "OHIPCARD", DomainName = "OHIPCARD", Oid = "1.2.3.4.5.6" }, "12343120423")
                },
            Telecoms = new List<EntityTelecomAddress>()
                {
                    new EntityTelecomAddress(AddressUseKeys.WorkPlace, "mailto:will@johnson.com")
                },
            Tags = new List<EntityTag>()
                {
                    new EntityTag("hasBirthCertificate", "true")
                },
            Notes = new List<EntityNote>()
                {
                    new EntityNote(Guid.Empty, "William is a test patient")
                    {
                        Author = new Person()
                    }
                },
            GenderConceptKey = Guid.Parse("f4e3a6bb-612e-46b2-9f77-ff844d971198"),
            DateOfBirth = new DateTime(1984, 03, 22),
            MultipleBirthOrder = 2,
            DeceasedDate = new DateTime(2016, 05, 02),
            DeceasedDatePrecision = DatePrecision.Day,
            DateOfBirthPrecision = DatePrecision.Day,
            CreationTime = DateTimeOffset.Now
        };

        /// <summary>
        /// Tests the where clause matches LINQ
        /// </summary>
        [Test]
        public void TestShouldMatchLinq()
        {
            CdssFactAssetDefinition when = new CdssFactAssetDefinition()
            {
                FactComputation = new CdssCsharpExpressionDefinition("!scopedObject.DeceasedDate.HasValue")
            };

            using (CdssExecutionStackFrame.Enter(new CdssExecutionContext<Patient>(this.m_patientUnderTest)))
            {
                var fact = when.Compute();
                Assert.IsInstanceOf<bool>(fact);
                Assert.IsFalse((bool)fact);
            }
        }

        /// <summary>
        /// Tests the where clause extracts via LINQ
        /// </summary>
        [Test]
        public void TestShouldExtractLinq()
        {
            CdssFactAssetDefinition when = new CdssFactAssetDefinition()
            {
                FactComputation = new CdssCsharpExpressionDefinition("scopedObject.DateOfBirth")
            };

            using (CdssExecutionStackFrame.Enter(new CdssExecutionContext<Patient>(this.m_patientUnderTest)))
            {
                var fact = when.Compute();
                Assert.IsInstanceOf<DateTime>(fact);
                Assert.AreEqual(this.m_patientUnderTest.DateOfBirth, fact);
            }
        }

        /// <summary>
        /// Tests the where clause matches LINQ
        /// </summary>
        [Test]
        public void TestShouldMatchSimpleHdsi()
        {
            CdssFactAssetDefinition when = new CdssFactAssetDefinition()
            {
                FactComputation = new CdssHdsiExpressionDefinition("deceasedDate=null")
            };

            using (CdssExecutionStackFrame.Enter(new CdssExecutionContext<Patient>(this.m_patientUnderTest)))
            {
                var fact = when.Compute();
                Assert.IsInstanceOf<bool>(fact);
                Assert.IsFalse((bool)fact);
            }
        }

        /// <summary>
        /// Tests the where clause matches LINQ
        /// </summary>
        [Test]
        public void TestShouldExtractSimpleHdsi()
        {
            CdssFactAssetDefinition when = new CdssFactAssetDefinition()
            {
                FactComputation = new CdssHdsiExpressionDefinition("dateOfBirth")
            };

            using (CdssExecutionStackFrame.Enter(new CdssExecutionContext<Patient>(this.m_patientUnderTest)))
            {
                var fact = when.Compute();
                Assert.IsInstanceOf<DateTime>(fact);
                Assert.AreEqual(this.m_patientUnderTest.DateOfBirth, fact);
            }
        }

        /// <summary>
        /// Tests the where clause matches LINQ
        /// </summary>
        [Test]
        public void TestShouldMatchSimpleXmlLinq()
        {
            Expression<Func<Patient, bool>> filterCondition = (data) => data.DeceasedDate == null;

            CdssFactAssetDefinition when = new CdssFactAssetDefinition()
            {
                FactComputation = new CdssXmlLinqExpressionDefinition(
                    filterCondition
                )
            };
            using (CdssExecutionStackFrame.Enter(new CdssExecutionContext<Patient>(this.m_patientUnderTest)))
            {
                Assert.IsFalse((bool)when.Compute());
            }
        }

        /// <summary>
        /// Tests the where clause matches LINQ
        /// </summary>
        [Test]
        public void TestShouldMatchAllCondition()
        {
            Expression<Func<Patient, bool>> filterCondition = (data) => data.DateOfBirth <= DateTime.Now;

            var factComputation = new CdssAllExpressionDefinition(
                    new CdssXmlLinqExpressionDefinition(filterCondition),
                    new CdssHdsiExpressionDefinition("tag[hasBirthCertificate].value=true"),
                    new CdssCsharpExpressionDefinition($"scopedObject.StatusConceptKey.Value == Guid.Parse(\"{StatusKeys.Active}\")")
                );
            CdssFactAssetDefinition when = new CdssFactAssetDefinition()
            {
                FactComputation = factComputation,
            };

            using (CdssExecutionStackFrame.Enter(new CdssExecutionContext<Patient>(this.m_patientUnderTest)))
            {
                Assert.IsTrue((bool)when.Compute());
            }

            factComputation.ContainedExpressions.Add(new CdssCsharpExpressionDefinition("scopedObject.Tags.Count == 0"));
            when = new CdssFactAssetDefinition() { FactComputation = factComputation };
            using (CdssExecutionStackFrame.Enter(new CdssExecutionContext<Patient>(this.m_patientUnderTest)))
            {
                Assert.IsFalse((bool)when.Compute());
            }
        }

    }
}
