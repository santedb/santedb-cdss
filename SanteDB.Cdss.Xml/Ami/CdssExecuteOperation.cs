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
using SanteDB.Cdss.Xml.Antlr;
using SanteDB.Cdss.Xml.Diagnostics;
using SanteDB.Core;
using SanteDB.Core.Applets.ViewModel.Json;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Cdss;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Interop;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Parameters;
using SanteDB.Core.Model.Serialization;
using SanteDB.Core.Services;
using SanteDB.Rest.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SanteDB.Cdss.Xml.Ami
{
    /// <summary>
    /// Represents an execution operation for arbitrarily executing a CDSS protocol
    /// </summary>
    public class CdssExecuteOperation : IApiChildOperation
    {
        private readonly ICdssLibraryRepository m_cdssLibrary;
        private Tracer m_tracer = Tracer.GetTracer(typeof(CdssExecuteOperation));

        /// <summary>
        /// DI constructor
        /// </summary>
        /// <param name="cdssLibraryRepository"></param>
        public CdssExecuteOperation(ICdssLibraryRepository cdssLibraryRepository)
        {
            this.m_cdssLibrary = cdssLibraryRepository;
        }
        /// <inheritdoc/>
        public string Name => "execute";

        /// <inheritdoc/>
        public ChildObjectScopeBinding ScopeBinding => ChildObjectScopeBinding.Instance | ChildObjectScopeBinding.Class;

        /// <inheritdoc/>
        public Type[] ParentTypes => new Type[] { typeof(ICdssLibraryRepositoryMetadata) };

        /// <inheritdoc/>
        public object Invoke(Type scopingType, object scopingKey, ParameterCollection parameters)
        {
            var scoperIsUuid = scopingKey is Guid uuid || Guid.TryParse(scopingKey?.ToString(), out uuid);
            var definition = String.Empty;
            if (!parameters.TryGet("definition", out definition) && !scoperIsUuid)
            {
                throw new ArgumentOutOfRangeException(nameof(scopingKey));
            }

            ICdssLibrary cdssLibrary = null;
            if (!String.IsNullOrEmpty(definition))
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(definition)))
                {
                    var transpiler = CdssLibraryTranspiler.Transpile(ms, true);
                    cdssLibrary = new XmlProtocolLibrary(transpiler);
                }
            }
            else
            {
                cdssLibrary = this.m_cdssLibrary.Get(uuid, null);
            }

            if (cdssLibrary != null)
            {
                return this.ExecuteCdssLibrary(cdssLibrary, parameters);
            }
            else
            {
                throw new KeyNotFoundException(scopingKey.ToString());
            }
        }

        /// <summary>
        /// Execute the CDSS library
        /// </summary>
        private CdssExecutionResult ExecuteCdssLibrary(ICdssLibrary cdssLibrary, ParameterCollection parameters)
        {
            String target = String.Empty, targetId = String.Empty, targetType = String.Empty;
            if (!parameters.TryGet("target", out target) &&
                !parameters.TryGet("targetId", out targetId))
            {
                throw new ArgumentNullException("target or targetId");

            }
            else if (!parameters.TryGet("targetType", out targetType))
            {
                throw new ArgumentNullException("targetType");
            }

            var type = new ModelSerializationBinder().BindToType(null, targetType);
            IdentifiedData targetForExecution = null;
            if (!String.IsNullOrEmpty(target))
            {
                using (var jvu = new JsonViewModelSerializer())
                {
                    using (var sr = new StringReader(target))
                    {
                        targetForExecution = jvu.DeSerialize(sr, type) as IdentifiedData;
                        targetForExecution.Key = targetForExecution.Key ?? Guid.NewGuid();
                    }
                }
            }
            else if (Guid.TryParse(targetId, out var targetUuid))
            {
                var repoTyp = typeof(IRepositoryService<>).MakeGenericType(type);
                var repo = ApplicationServiceContext.Current.GetService(repoTyp) as IRepositoryService;
                targetForExecution = repo?.Get(targetUuid);
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }

            // Force the load of participations
            var startTime = DateTimeOffset.Now;
            var results = cdssLibrary.Execute(targetForExecution, parameters.Parameters.Where(o => o.Name != "target" && o.Name != "definition").ToDictionaryIgnoringDuplicates(o => o.Name, o => o.Value));
            var debugData = results.OfType<CdssDebugSessionData>().FirstOrDefault();
            return new CdssExecutionResult()
            {
                StartTime = startTime,
                StopTime = DateTimeOffset.Now,
                Issues = results.OfType<DetectedIssue>().ToList(),
                Proposals = results.OfType<IdentifiedData>().ToList(),
                ResultingTarget = targetForExecution,
                Debug = debugData?.GetDiagnosticReport()
            };
        }
    }
}
