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
 * User: fyfej
 * Date: 2023-11-27
 */
using Newtonsoft.Json;
using SanteDB.Core.BusinessRules;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Expressions
{
    /// <summary>
    /// Expression definition
    /// </summary>
    [XmlType(nameof(CdssExpressionDefinition), Namespace = "http://santedb.org/cdss")]
    public abstract class CdssExpressionDefinition
    {

        /// <summary>
        /// Gets or sets the transpiled metadata
        /// </summary>
        [XmlElement("source"), JsonProperty("source")]
        public CdssTranspileMapMetaData TranspileSourceReference { get; set; }

        /// <summary>
        /// Generate the LINQ expression so it can be computed
        /// </summary>
        /// <param name="cdssContext">The CDSS context</param>
        /// <param name="parameters">The context parameter expressions to pass to the generation</param>
        /// <returns>The generated expression</returns>
        internal abstract Expression GenerateComputableExpression(CdssExecutionContext cdssContext, params ParameterExpression[] parameters);

        /// <summary>
        /// Validate that the expression is appropriately represented for execution
        /// </summary>
        /// <param name="context">The context in which the validation is occurring</param>
        public abstract IEnumerable<DetectedIssue> Validate(CdssExecutionContext context);

        /// <summary>
        /// Represent this as a source code reference string
        /// </summary>
        /// <returns></returns>
        public string ToReferenceString() => $"{this.GetType().Name} {this.TranspileSourceReference?.SourceFileName} @{this.TranspileSourceReference?.StartPosition}";
    }
}
