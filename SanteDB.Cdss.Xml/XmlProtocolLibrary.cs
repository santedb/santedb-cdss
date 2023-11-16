using SanteDB.Cdss.Xml.Model;
using SanteDB.Cdss.Xml.Model.Actions;
using SanteDB.Cdss.Xml.Model.Assets;
using SanteDB.Core;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Cdss;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Exceptions;
using SanteDB.Core.i18n;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Roles;
using SharpCompress;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;

namespace SanteDB.Cdss.Xml
{
    /// <summary>
    /// An implementation of the <see cref="ICdssLibrary"/> which uses the XML format
    /// </summary>
    public class XmlProtocolLibrary : ICdssLibrary
    {

        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(XmlProtocolLibrary));

        // definition loaded from XML
        private CdssLibraryDefinition m_library;
        private IList<CdssLibraryDefinition> m_scopedLibraries;

        /// <summary>
        /// Creates a new protocol library
        /// </summary>
        public XmlProtocolLibrary()
        {
        }

        /// <summary>
        /// Create a new protocol library 
        /// </summary>
        public XmlProtocolLibrary(CdssLibraryDefinition library)
        {
            this.m_library = library;
        }

        /// <inheritdoc/>
        public Guid Uuid
        {
            get => this.m_library.Uuid;
            set
            {
                if (this.m_library.Uuid != Guid.Empty ||
                    value != this.m_library.Uuid)
                {
                    throw new InvalidOperationException(ErrorMessages.WOULD_RESULT_INVALID_STATE);
                }
                else
                {
                    this.m_library.Uuid = value;
                }
            }
        }
        /// <inheritdoc/>
        public string Id => this.m_library.Id;

        /// <inheritdoc/>
        public string Name => this.m_library.Name;

        /// <inheritdoc/>
        public string Version => this.m_library.Metadata?.Version;

        /// <inheritdoc/>
        public string Oid => this.m_library.Oid;

        /// <inheritdoc/>
        public string Documentation => this.m_library.Metadata?.Documentation;

        /// <summary>
        /// Get the library definition
        /// </summary>
        public CdssLibraryDefinition Library => this.m_library;

        /// <summary>
        /// Get protocols defined for patients in the library
        /// </summary>
        public IEnumerable<ICdssProtocol> GetProtocols(String forScope)
        {
            this.InitializeLibrary();
            var retVal = this.m_library.Definitions.OfType<CdssDecisionLogicBlockDefinition>()
                    .Where(o => o.Context.Type == typeof(Patient))
                    .SelectMany(o => o.Definitions)
                    .OfType<CdssProtocolAssetDefinition>()
                    .Select(p => new XmlClinicalProtocol(p, this.m_scopedLibraries));
            if (!String.IsNullOrEmpty(forScope))
            {
                retVal = retVal.Where(o => o.Scopes.Any(s => s.Oid == forScope || s.Name == forScope));
            }
            return retVal;
        }

        /// <inheritdoc/>
        public void Load(Stream definitionStream)
        {
            if (definitionStream == null)
            {
                throw new ArgumentNullException(nameof(definitionStream));
            }

            this.m_library = CdssLibraryDefinition.Load(definitionStream);
        }

        /// <inheritdoc/>
        public void Save(Stream definitionStream)
        {
            if (definitionStream == null)
            {
                throw new ArgumentNullException(nameof(definitionStream));
            }

            this.m_library.Save(definitionStream);
        }

        /// <summary>
        /// Initialize the library for use
        /// </summary>
        private void InitializeLibrary()
        {
            if (this.m_scopedLibraries == null)
            {
                var cdssLibraryService = ApplicationServiceContext.Current.GetService<ICdssLibraryRepository>();
                this.m_scopedLibraries = new List<CdssLibraryDefinition>() { this.m_library };
                this.m_scopedLibraries.AddRange(this.m_library.Include?.Select(o => cdssLibraryService?.ResolveReference(o)).OfType<XmlProtocolLibrary>().Select(o=>o.Library) ?? new CdssLibraryDefinition[0]);
                if(this.m_scopedLibraries.Any(o=>o.Status == CdssObjectState.DontUse))
                {
                    throw new InvalidOperationException(String.Format(ErrorMessages.FORBIDDEN_ON_OBJECT_IN_STATE));
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<DetectedIssue> Analyze(IdentifiedData analysisTarget, IDictionary<String, object> parameters = null)
        {

#if DEBUG
            var sw = new Stopwatch();
            sw.Start();
#endif
            try
            {
                // Is the library not active?
                if (this.m_library.Status == CdssObjectState.DontUse)
                {
                    throw new InvalidOperationException(String.Format(ErrorMessages.FORBIDDEN_ON_OBJECT_IN_STATE));
                }


                this.m_tracer.TraceInfo("Starting analysis of {0} using {1}...", analysisTarget, this.Name);

                this.InitializeLibrary();
                var context = CdssExecutionContext.CreateContext((IdentifiedData)analysisTarget, this.m_scopedLibraries);
                using (CdssExecutionStackFrame.Enter(context))
                {
                    parameters?.ForEach(o => context.SetValue(o.Key, o.Value));
                    context.SetValue("mode", "analyze");
                    context.ThrowIfNotValid();

                    var definitions = this.m_library.Definitions
                        .OfType<CdssDecisionLogicBlockDefinition>()
                        .AppliesTo(context)
                        .SelectMany(o => o.Definitions)
                        .OfType<CdssRuleAssetDefinition>()
                        .Select(r => new { result = (bool?)r.Compute(), rule = r.Name })
                        .ToArray();

                    return context.Issues;
                }
            }
            finally
            {
#if DEBUG
                sw.Stop();
                this.m_tracer.TraceInfo("Finished analysis of {0} (in {1} ms)", analysisTarget, sw.ElapsedMilliseconds);
#endif
            }
        }

        /// <inheritdoc/>
        public IEnumerable<object> Execute(IdentifiedData target, IDictionary<String, object> parameters = null)
        {

#if DEBUG
            var sw = new Stopwatch();
            sw.Start();
#endif
            try
            {
                // Is the library not active?
                if (this.m_library.Status == CdssObjectState.DontUse)
                {
                    throw new InvalidOperationException(String.Format(ErrorMessages.FORBIDDEN_ON_OBJECT_IN_STATE));
                }

                this.m_tracer.TraceInfo("Starting analysis of {0} using {1}...", target, this.Name);

                this.InitializeLibrary();
                var context = CdssExecutionContext.CreateContext((IdentifiedData)target, this.m_scopedLibraries);

                using (CdssExecutionStackFrame.Enter(context))
                {
                    parameters?.ForEach(o => context.SetValue(o.Key, o.Value));
                    context.SetValue("mode", "execute");
                    context.ThrowIfNotValid();

                    var definitions = this.m_library.Definitions
                        .OfType<CdssDecisionLogicBlockDefinition>()
                        .AppliesTo(context)
                        .SelectMany(o => o.Definitions)
                        .Select(r => new { result = r.Compute(), rule = r.Name })
                        .ToArray();

                    return context.Proposals.OfType<Object>().Union(context.Issues);
                }

            }
            finally
            {
#if DEBUG
                sw.Stop();
                this.m_tracer.TraceInfo("Finished execution of {0} (in {1} ms)", target, sw.ElapsedMilliseconds);
#endif
            }
        }

    }
}
