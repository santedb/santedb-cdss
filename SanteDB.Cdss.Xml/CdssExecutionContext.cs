using System;
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
        private ICdssContext m_context;

        /// <summary>
        /// Only allow current call
        /// </summary>
        private CdssExecutionContext(ICdssContext context)
        {
            this.m_context = context;
        }

        /// <summary>
        /// Get the value for <paramref name="variableName"/> from the context
        /// </summary>
        public object GetValue(String variableName) => this.m_context.GetValue(variableName);

        /// <summary>
        /// Get the current wrapper context
        /// </summary>
        public static CdssExecutionContext Current => m_currentContext;

        /// <summary>
        /// Enter a context
        /// </summary>
        internal static CdssExecutionContext Enter(ICdssContext context)
        {
            m_currentContext = new CdssExecutionContext(context);
            return m_currentContext;
        }

        /// <summary>
        /// Dispose of the context
        /// </summary>
        public void Dispose()
        {
            m_currentContext = null;
        }
    }
}


