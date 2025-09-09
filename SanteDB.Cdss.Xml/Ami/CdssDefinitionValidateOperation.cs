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
using SanteDB.Cdss.Xml.Antlr;
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Cdss.Xml.Model;
using SanteDB.Core;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Cdss;
using SanteDB.Core.Interop;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Parameters;
using SanteDB.Rest.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Ami
{
    /// <summary>
    /// Cdss validation result
    /// </summary>
    [XmlType(nameof(CdssValidationResult), Namespace = "http://santedb.org/cdss")]
    [XmlRoot(nameof(CdssValidationResult), Namespace = "http://santedb.org/cdss")]
    [JsonObject(nameof(CdssValidationResult))]
    public class CdssValidationResult
    {

        /// <summary>
        /// Detected issues
        /// </summary>
        [XmlElement("issue"), JsonProperty("issue")]
        public List<DetectedIssue> Issues { get; set; }

    }


    /// <summary>
    /// Represents an AMI operation to validate a CDSS document
    /// </summary>
    public class CdssDefinitionValidateOperation : IApiChildOperation
    {
        private readonly ICdssLibraryRepository m_cdssRepository;

        /// <inheritdoc/>
        public CdssDefinitionValidateOperation(ICdssLibraryRepository cdssLibraryRepository)
        {
            this.m_cdssRepository = cdssLibraryRepository;
        }

        /// <inheritdoc/>
        public string Name => "validate";

        /// <inheritdoc/>
        public ChildObjectScopeBinding ScopeBinding => ChildObjectScopeBinding.Class;

        /// <inheritdoc />
        public Type[] ParentTypes => new Type[] { typeof(ICdssLibraryRepositoryMetadata) };

        /// <inheritdoc/>
        public object Invoke(Type scopingType, object scopingKey, ParameterCollection parameters)
        {
            // validate the library
            if (!parameters.TryGet("definition", out string definition))
            {
                throw new ArgumentNullException("definition");
            }

            _ = parameters.TryGet("name", out string fileName);

            List<DetectedIssue> retVal = new List<DetectedIssue>();

            try
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(definition)))
                {
                    var transpiled = CdssLibraryTranspiler.Transpile(ms, true, fileName);

                    var scopedLibraries = transpiled.Include.Select(o => this.m_cdssRepository.ResolveReference(o)).OfType<XmlProtocolLibrary>().Select(o => o.Library).ToList();
                    scopedLibraries.Add(transpiled);

                    // Validate 
                    foreach (var itm in transpiled.Definitions?.OfType<CdssDecisionLogicBlockDefinition>() ?? new CdssDecisionLogicBlockDefinition[0])
                    {
                        if (itm.Context == null)
                        {
                            retVal.Add(new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.missing", "Logic block requires a context", Guid.Empty));
                        }
                        else if (!itm.Context.Type.IsAbstract)
                        {
                            var context = CdssExecutionContext.CreateValidationContext(Activator.CreateInstance(itm.Context.Type) as IdentifiedData, scopedLibraries);
                            retVal.AddRange(itm.Validate(context));
                        }
                    }
                }
            }
            catch (CdssTranspilationException e) // Turn this into a list of detected issues
            {
                retVal.AddRange(e.Errors.Select(o => new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.transpile", o.Message, Guid.Empty, $"@{o.Line}:{o.Column}")));
            }
            catch (TargetInvocationException e) when (e.InnerException is CdssEvaluationException ex)
            {
                retVal.Add(new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.evaluation", ex.Message, Guid.Empty, ex.CdssStack.Owner?.ToReferenceString()));
            }
            catch (CdssEvaluationException e)
            {
                retVal.Add(new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.evaluation", e.Message, Guid.Empty, e.CdssStack.Owner?.ToReferenceString()));
            }
            catch (Exception e)
            {
                retVal.Add(new DetectedIssue(DetectedIssuePriorityType.Error, "cdss.error", e.Message, Guid.Empty, null));

            }
            return new CdssValidationResult() { Issues = retVal };
        }
    }
}
