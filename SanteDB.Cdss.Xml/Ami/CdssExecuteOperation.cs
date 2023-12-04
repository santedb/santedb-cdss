using SanteDB.Cdss.Xml.Antlr;
using SanteDB.Cdss.Xml.Model;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Cdss;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Interop;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Parameters;
using SanteDB.Core.Model.Roles;
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
            if(!scoperIsUuid && !parameters.TryGet("definition", out definition))
            {
                throw new ArgumentOutOfRangeException(nameof(scopingKey));
            }

            ICdssLibrary cdssLibrary = null;
            if (scoperIsUuid)
            {
                cdssLibrary = this.m_cdssLibrary.Get(uuid, null);
            }
            else
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(definition))) {
                    var transpiler = CdssLibraryTranspiler.Transpile(ms, true);
                    cdssLibrary = new XmlProtocolLibrary(transpiler);
                }
            }
            if(cdssLibrary != null)
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
            if(!parameters.TryGet("target", out IdentifiedData target))
            {
                throw new MissingMemberException("target");
            }

            // Force the load of participations
            var startTime = DateTimeOffset.Now;
            var results = cdssLibrary.Execute(target);
            return new CdssExecutionResult()
            {
                StartTime = startTime,
                StopTime = DateTimeOffset.Now,
                Issues = results.OfType<DetectedIssue>().ToList(),
                Proposals = results.OfType<IdentifiedData>().ToList(),
                ResultingTarget = target
            };
        }
    }
}
