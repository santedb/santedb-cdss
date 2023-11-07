using SanteDB.Cdss.Xml.Model;
using SanteDB.Cdss.Xml.Model.Actions;
using SanteDB.Cdss.Xml.Model.Assets;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Cdss;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        public Guid Uuid => this.m_library.Uuid;

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
        public IEnumerable<ICdssProtocol> GetProtocols(Type forContext) =>
            this.m_library
            .Definitions
            .OfType<CdssDecisionLogicBlockDefinition>()
            .Where(o => o.Context.Type == forContext)
            .SelectMany(o => o.Definitions.OfType<CdssProtocolAssetDefinition>())
            .Select(o => new XmlClinicalProtocol(o));

        /// <inheritdoc/>
        public void Save(Stream definitionStream)
        {
            if (definitionStream == null)
            {
                throw new ArgumentNullException(nameof(definitionStream));
            }

            this.m_library.Save(definitionStream);
        }

        /// <inheritdoc/>
        public IEnumerable<DetectedIssue> Analyze(IdentifiedData analysisTarget)
        {

#if DEBUG
            var sw = new Stopwatch();
            sw.Start();
#endif
            try
            {
                this.m_tracer.TraceInfo("Starting analysis of {0} using {1}...", analysisTarget, this.Name);

                var context = CdssExecutionContext.CreateContext((IdentifiedData)analysisTarget.DeepCopy());
                using (CdssExecutionStackFrame.Enter(context))
                {
                    var definitions = this.m_library.Definitions.OfType<CdssDecisionLogicBlockDefinition>()
                        .Where(o => o.Context.Type == analysisTarget.GetType())
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
        public IEnumerable<object> Execute(IdentifiedData target)
        {

#if DEBUG
            var sw = new Stopwatch();
            sw.Start();
#endif
            try
            {
                this.m_tracer.TraceInfo("Starting analysis of {0} using {1}...", target, this.Name);

                var executionContext = CdssExecutionContext.CreateContext((IdentifiedData)target.DeepCopy());

                using (CdssExecutionStackFrame.Enter(executionContext))
                {
                    var definitions = this.m_library.Definitions.OfType<CdssDecisionLogicBlockDefinition>()
                        .Where(o => o.Context.Type == target.GetType())
                        .SelectMany(o => o.Definitions)
                        .Select(r => new { result = (bool?)r.Compute(), rule = r.Name })
                        .ToArray();

                    return executionContext.Proposals.OfType<Object>().Union(executionContext.Issues);
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
