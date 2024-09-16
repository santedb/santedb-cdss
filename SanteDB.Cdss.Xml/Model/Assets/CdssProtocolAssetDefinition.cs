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
using SanteDB.Core.BusinessRules;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Assets
{
    /// <summary>
    /// CDSS clinical protocol definition
    /// </summary>
    /// <remarks>A protocol is essentially a rule, however the protocol signals to 
    /// the interpreter that the protocol is an "entry point" for each rule</remarks>
    [XmlType(nameof(CdssProtocolAssetDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssProtocolAssetDefinition : CdssRuleAssetDefinition
    {

        /// <summary>
        /// Gets or sets the scopes where this protocol should be applied
        /// </summary>
        [XmlArray("scopes"), XmlArrayItem("add"), JsonProperty("scopes")]
        public List<CdssProtocolGroupDefinition> Scopes { get; set; }

        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if (String.IsNullOrEmpty(this.Oid))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.protocol.oidMissing", "CDSS Protocols must carry an OID", Guid.Empty, this.ToReferenceString());
            }
            if (String.IsNullOrEmpty(this.Name))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.protocol.nameMissing", "CDSS Protocols must carry a NAME", Guid.Empty, this.ToReferenceString());
            }
            if (this.Uuid == Guid.Empty)
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.protocol.uuidMissing", "CDSS Protocols must carry a UUID", Guid.Empty, this.ToReferenceString());
            }
            foreach (var itm in base.Validate(context))
            {
                itm.RefersTo = itm.RefersTo ?? this.ToReferenceString();
                yield return itm;
            }
        }
    }
}
