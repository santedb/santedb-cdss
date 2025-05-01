/*
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
 * Date: 2024-6-21
 */
using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Diagnostics;
using SanteDB.Core.i18n;
using System;
using System.Linq;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Diagnostics
{
    /// <summary>
    /// Represents a diagnostic sample
    /// </summary>
    [XmlType(nameof(CdssDiagnosticSample), Namespace = "http://santedb.org/cdss")]
    public abstract class CdssDiagnosticSample
    {

        /// <summary>
        /// Serialization ctor
        /// </summary>
        public CdssDiagnosticSample()
        {

        }

        /// <summary>
        /// Creates a new sample object with <paramref name="sample"/> providing the data
        /// </summary>
        protected CdssDiagnosticSample(CdssDebugSample sample)
        {
            this.CollectionTime = sample.CollectionTime.DateTime;
        }

        /// <summary>
        /// Gets or sets the time that the colle
        /// </summary>
        [XmlAttribute("ts"), JsonProperty("ts")]
        public DateTime CollectionTime { get; set; }

        /// <summary>
        /// Determine if collection time is specified
        /// </summary>
        public bool ShouldSerializeCollectionTime() => this.CollectionTime != default(DateTime);

        /// <summary>
        /// Create an appropriate diagnostic sample object for the <paramref name="debugSample"/>
        /// </summary>
        internal static CdssDiagnosticSample Create(CdssDebugSample debugSample)
        {
            switch (debugSample)
            {
                case CdssDebugExceptionSample exc:
                    return new CdssExceptionDiagnosticSample(exc);
                case CdssDebugFactSample fact:
                    return new CdssFactDiagnosticSample(fact);
                case CdssDebugIssueSample iss:
                    return new CdssIssueDiagnosticSample(iss);
                case CdssDebugProposalSample prop:
                    return new CdssProposalDiagnosticSample(prop);
                case CdssDebugPropertyAssignmentSample propAsgn:
                    return new CdssPropertyAssignDiagnosticSample(propAsgn);
                case CdssDebugValueSample val:
                    if (val.IsWrite)
                    {
                        return new CdssValueDiagnosticSample(val);
                    }
                    else
                    {
                        return new CdssValueLookupDiagnosticSample(val);
                    }
                case CdssDebugStackFrame frame:
                    if (!frame.GetSamples().Any())
                    {
                        return null;
                    }
                    return new CdssDiagnosticFrame(frame);
                default:
                    throw new InvalidOperationException(ErrorMessages.MAP_INVALID_TYPE);
            }
        }
    }
}