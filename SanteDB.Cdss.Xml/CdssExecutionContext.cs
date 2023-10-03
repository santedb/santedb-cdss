using SanteDB.Cdss.Xml.Model;
using SanteDB.Core.i18n;
using SanteDB.Core.Model;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Cdss.Xml
{

    /// <summary>
    /// Used so that our HDSI expression can access a CDSS context variables
    /// </summary>
    internal class CdssExecutionContext : IDisposable
    {
        // Current context on the thread
        [ThreadStatic]
        private static CdssExecutionContext m_currentContext;

        // Context
        private readonly ICdssContext m_context;
        private readonly CdssBaseObjectDefinition m_owner;
        private readonly CdssExecutionContext m_parent;

        /// <summary>
        /// Only allow current call
        /// </summary>
        private CdssExecutionContext(ICdssContext context)
        {
            this.m_context = context;
            this.ScopedObject = context.Target;
        }

        private CdssExecutionContext(CdssExecutionContext parent, CdssBaseObjectDefinition owner)
        {
            this.m_owner = owner;
            this.m_parent = parent;
            this.ScopedObject = parent.ScopedObject;
        }

        /// <summary>
        /// Get the value for <paramref name="variableName"/> from the context
        /// </summary>
        public object GetValue(String variableName) => this.m_context?.GetValue(variableName) ??
            this.m_parent.GetValue(variableName);

        /// <summary>
        /// Get the current wrapper context
        /// </summary>
        public static CdssExecutionContext Current => m_currentContext;

        /// <summary>
        /// Get the parent of this context
        /// </summary>
        public CdssExecutionContext Parent => this.m_parent;

        /// <summary>
        /// Gets or sets the scoped object
        /// </summary>
        public IdentifiedData ScopedObject { get; set; }

        /// <summary>
        /// Get the CDSS definition object which is the owner of this context (null if it is a root context)
        /// </summary>
        public CdssBaseObjectDefinition Owner => this.m_owner;

        /// <summary>
        /// Enter a context
        /// </summary>
        internal static CdssExecutionContext Enter(ICdssContext context)
        {
            if(m_currentContext != null)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.WOULD_RESULT_INVALID_STATE, nameof(Enter)));
            }
            m_currentContext = new CdssExecutionContext(context);
            return m_currentContext;
        }

        /// <summary>
        /// Enter a sub context
        /// </summary>
        /// <param name="owner">The owner of the child context</param>
        /// <returns>The child context</returns>
        internal static CdssExecutionContext EnterChildContext(CdssBaseObjectDefinition owner)
        {
            if(m_currentContext == null)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.WOULD_RESULT_INVALID_STATE, nameof(EnterChildContext)));
            }
            m_currentContext = new CdssExecutionContext(m_currentContext, owner);
            return m_currentContext;
        }

        /// <summary>
        /// Dispose of the context
        /// </summary>
        public void Dispose()
        {
            m_currentContext = m_currentContext?.m_parent;
        }
    }
}


