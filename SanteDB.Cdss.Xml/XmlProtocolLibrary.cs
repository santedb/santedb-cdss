using SanteDB.Cdss.Xml.Model;
using SanteDB.Core.Cdss;
using System;
using System.Collections.Generic;
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

        // definition loaded from XML
        private RulesetLibrary m_definition;

        /// <summary>
        /// Creates a new protocol library
        /// </summary>
        public XmlProtocolLibrary()
        {
        }

        /// <summary>
        /// Create a new protocol library 
        /// </summary>
        public XmlProtocolLibrary(RulesetLibrary library)
        {
            this.m_definition = library;
        }

        /// <inheritdoc/>
        public Guid Uuid => this.m_definition.Uuid;

        /// <inheritdoc/>
        public string Id => this.m_definition.Id;

        /// <inheritdoc/>
        public string Name => this.m_definition.Name;

        /// <inheritdoc/>
        public string Version => this.m_definition.Version;

        /// <inheritdoc/>
        public string Oid => this.m_definition.Oid;

        /// <inheritdoc/>
        public string Documentation => this.m_definition.Documentation;

        /// <inheritdoc/>
        public IEnumerable<ICdssAssetGroup> Groups => this.m_definition.Groups.Select(o => new XmlProtocolAssetGroup(o));

        /// <inheritdoc/>
        public CdssAssetClassification Classification => CdssAssetClassification.DecisionSupportLibrary;

        /// <inheritdoc/>
        public void Load(Stream definitionStream)
        {
            if(definitionStream == null)
            {
                throw new ArgumentNullException(nameof(definitionStream));
            }

            this.m_definition = RulesetLibrary.Load(definitionStream);
        }

        /// <inheritdoc/>
        public TResolved ResolveElement<TResolved>(string elementName) =>
            this.m_definition.Rules.Where(o => o.Id == elementName).OfType<TResolved>()
                .Union(this.m_definition.Actions.Where(o => o.Id == elementName).OfType<TResolved>())
                .Union(this.m_definition.When.Where(o => o.Id == elementName).OfType<TResolved>()).SingleOrDefault();

        /// <inheritdoc/>
        public void Save(Stream definitionStream)
        {
            if(definitionStream == null)
            {
                throw new ArgumentNullException(nameof(definitionStream));
            }

            this.m_definition.Save(definitionStream);
        }
    }
}
