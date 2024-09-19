﻿/*
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
using SanteDB.Cdss.Xml.Diagnostics;
using SanteDB.Cdss.Xml.Exceptions;
using System;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Diagnostics
{
    /// <summary>
    /// Represents a collection of an exception
    /// </summary>
    [XmlType(nameof(CdssExceptionDiagnosticSample), Namespace = "http://santedb.org/cdss")]
    public class CdssExceptionDiagnosticSample : CdssDiagnosticSample
    {

        /// <summary>
        /// Serialization ctor
        /// </summary>
        public CdssExceptionDiagnosticSample()
        {
        }

        /// <summary>
        /// Create a new exception sample from <paramref name="exceptionSample"/>
        /// </summary>
        internal CdssExceptionDiagnosticSample(CdssDebugExceptionSample exceptionSample) : base(exceptionSample)
        {
            if (exceptionSample.Exception is CdssEvaluationException cde)
            {
                this.Summary = cde.ToCdssStackTrace();
            }
            else
            {
                this.Summary = exceptionSample.Exception.ToHumanReadableString();
            }
            this.Detail = exceptionSample.Exception.ToString();
        }

        /// <summary>
        /// Gets or sets the summary information
        /// </summary>
        [XmlElement("summary"), JsonProperty("summary")]
        public String Summary { get; set; }

        /// <summary>
        /// Gets or sets the exception detail
        /// </summary>
        [XmlElement("detail"), JsonProperty("detail")]
        public String Detail { get; set; }
    }
}
