using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// Load context used for setting parents and allowing assets to understand where in the Xml TREE the object exists
    /// </summary>
    internal class CdssLibraryLoadContext : IDisposable
    {

        // Load context
        [ThreadStatic]
        private static CdssLibraryLoadContext m_currentLoadContext = null;

        /// <summary>
        /// Current stack of objects loaded in the order in which they were loaded
        /// </summary>
        private readonly LinkedList<CdssBaseObjectDefinition> m_loadedObjects = new LinkedList<CdssBaseObjectDefinition>();

        /// <summary>
        /// Load context
        /// </summary>
        private CdssLibraryLoadContext()
        {
        }

        /// <summary>
        /// Get the current load context
        /// </summary>
        public static CdssLibraryLoadContext Current
        {
            get
            {
                if(m_currentLoadContext == null)
                {
                    m_currentLoadContext = new CdssLibraryLoadContext();
                }
                return m_currentLoadContext;
            }
        }

        /// <summary>
        /// Register a loaded object
        /// </summary>
        public void RegisterLoaded(CdssBaseObjectDefinition objectLoaded)
        {
            this.m_loadedObjects.AddLast(objectLoaded);
        }

        /// <summary>
        /// Find the last loaded <typeparamref name="TLastNode"/>
        /// </summary>
        public TLastNode FindLastLoaded<TLastNode>() where TLastNode : CdssBaseObjectDefinition 
        {
            var node = this.m_loadedObjects.Last;
            while (node != null)
            {
                if(node.Value is TLastNode tl)
                {
                    return tl;
                }
                node = node.Previous;
            }
            return null;
        }


        /// <summary>
        /// Dispose of the load context
        /// </summary>
        public void Dispose()
        {
            m_currentLoadContext = null;
            this.m_loadedObjects.Clear();
        }
    }
}
