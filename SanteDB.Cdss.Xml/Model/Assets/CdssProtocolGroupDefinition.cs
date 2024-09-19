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
using SanteDB.Core.BusinessRules;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Assets
{

    /// <summary>
    /// Represents an individual grouping
    /// </summary>
    [XmlType(nameof(CdssProtocolGroupDefinition), Namespace = "http://santedb.org/cdss")]
    public class CdssProtocolGroupDefinition : CdssBaseObjectDefinition
    {

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            if (string.IsNullOrEmpty(this.Oid) && string.IsNullOrEmpty(this.Name))
            {
                yield return new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.protocol.group.unidentified", "CDSS protocol groups must carry either OID or Name (or both)", Guid.Empty, this.ToReferenceString());
            }
        }
    }
}