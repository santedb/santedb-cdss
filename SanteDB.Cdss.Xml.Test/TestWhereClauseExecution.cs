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
using System;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Model.Constants;
using SanteDB.Core.Model.Entities;
using System.Collections.Generic;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Cdss.Xml.Model;
using System.Linq.Expressions;
using SanteDB.Cdss.Xml.Model.XmlLinq;
using NUnit.Framework;

namespace SanteDB.Cdss.Xml.Test
{
    [TestFixture(Category = "CDSS")]
    public class TestWhereClauseExecution
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
                    UserClass = UserClassKeys.HumanUser
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
                    new EntityIdentifier(new AssigningAuthority() { Name = "OHIPCARD", DomainName = "OHIPCARD", Oid = "1.2.3.4.5.6" }, "12343120423")
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
            ProtocolWhenClauseCollection when = new ProtocolWhenClauseCollection()
            {
                Clause = new List<object>() { "!Target.DeceasedDate.HasValue" }
            };
            Assert.IsFalse(when.Evaluate(new CdssContext<Patient>(this.m_patientUnderTest)));
        }

        /// <summary>
        /// Tests the where clause matches LINQ
        /// </summary>
        [Test]
        public void TestShouldMatchSimpleHdsi()
        {
            ProtocolWhenClauseCollection when = new ProtocolWhenClauseCollection()
            {
                Clause = new List<Object>() {
                    new WhenClauseHdsiExpression() {
                        Expression = "deceasedDate=null"
                    }
                }
            };
            Assert.IsFalse(when.Evaluate(new CdssContext<Patient>(this.m_patientUnderTest)));
        }

        /// <summary>
        /// Tests the where clause matches LINQ
        /// </summary>
        [Test]
        public void TestShouldMatchSimpleXmlLinq()
        {
            Expression<Func<CdssContext<Patient>, bool>> filterCondition = (data) => data.Target.DeceasedDate == null;

            ProtocolWhenClauseCollection when = new ProtocolWhenClauseCollection()
            {
                Clause = new List<Object>() {
                    XmlExpression.FromExpression(filterCondition)
                }
            };
            Assert.IsFalse(when.Evaluate(new CdssContext<Patient>(this.m_patientUnderTest)));
        }

        /// <summary>
        /// Tests the where clause matches LINQ
        /// </summary>
        [Test]
        public void TestShouldMatchAllCondition()
        {
            Expression<Func<CdssContext<Patient>, bool>> filterCondition = (data) => data.Target.DateOfBirth <= DateTime.Now;

            ProtocolWhenClauseCollection when = new ProtocolWhenClauseCollection()
            {
                Operator = BinaryOperatorType.AndAlso,
                Clause = new List<Object>() {
                    XmlExpression.FromExpression(filterCondition),
                    new WhenClauseHdsiExpression() { Expression = "tag[hasBirthCertificate].value=true" },
                    "Target.StatusConceptKey.Value == Guid.Parse(\"" + StatusKeys.Active + "\")"
                }
            };
            Assert.IsTrue(when.Evaluate(new CdssContext<Patient>(this.m_patientUnderTest)));

            when.Clause.Add("Target.Tags.Count == 0");
            when.Compile(new CdssContext<Patient>(this.m_patientUnderTest));
            Assert.IsFalse(when.Evaluate(new CdssContext<Patient>(this.m_patientUnderTest)));
        }

    }
}
