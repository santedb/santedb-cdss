using SanteDB.Cdss.Xml.Model;
using SanteDB.Core.Cdss;
using SanteDB.Core.i18n;
using SanteDB.Core.Model;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;

namespace SanteDB.Cdss.Xml
{

    /// <summary>
    /// Used so that our HDSI expression can access a CDSS context variables
    /// </summary>
    public class CdssExecutionStackFrame : IDisposable
    {
        // Current context on the thread
        [ThreadStatic]
        private static CdssExecutionStackFrame m_currentContext;

        // Context
        private readonly CdssExecutionContext m_context;
        private readonly CdssBaseObjectDefinition m_owner;
        private readonly CdssExecutionStackFrame m_parent;
        private IdentifiedData m_scopedObject = null;

        /// <summary>
        /// Only allow current call
        /// </summary>
        private CdssExecutionStackFrame(ICdssExecutionContext context)
        {
            if (!(context is CdssExecutionContext ctx))
            { 
                throw new ArgumentOutOfRangeException(nameof(context), String.Format(ErrorMessages.ARGUMENT_INCOMPATIBLE_TYPE, typeof(CdssExecutionContext), context.GetType()));
            }

            this.m_context = ctx;
            this.m_scopedObject = context.Target;
        }

        private CdssExecutionStackFrame(CdssExecutionStackFrame parent, CdssBaseObjectDefinition owner) : this(parent.Context)
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
        public static CdssExecutionStackFrame Current => m_currentContext;

        /// <summary>
        /// Get the parent of this context
        /// </summary>
        internal CdssExecutionStackFrame Parent => this.m_parent;

        /// <summary>
        /// Gets or sets the scoped object
        /// </summary>
        public IdentifiedData ScopedObject
        {
            get => this.m_scopedObject;
            set
            {
                if (this.m_context.DebugSession != null &&
                    this.m_context is ICdssExecutionContext cec 
                    && cec.Target != value 
                    && value != this.Parent?.ScopedObject 
                    && value != this.ScopedObject)
                {
                    this.m_context.DebugSession.CurrentFrame.AddSample("scopedObject", value);
                }
                this.m_scopedObject = value;
            }
        }

        /// <summary>
        /// Get the CDSS definition object which is the owner of this context (null if it is a root context)
        /// </summary>
        public CdssBaseObjectDefinition Owner => this.m_owner;

        /// <summary>
        /// Gets the context in which this is executing
        /// </summary>
        public CdssExecutionContext Context => this.m_context;

        /// <summary>
        /// Enter a context
        /// </summary>
        public static CdssExecutionStackFrame Enter(ICdssExecutionContext context)
        {
            if (m_currentContext != null)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.WOULD_RESULT_INVALID_STATE, nameof(Enter)));
            }

            m_currentContext = new CdssExecutionStackFrame(context);
            (context as CdssExecutionContext)?.DebugSession?.EnterFrame(m_currentContext);

            return m_currentContext;
        }

        /// <summary>
        /// Enter a sub context
        /// </summary>
        /// <param name="owner">The owner of the child context</param>
        /// <returns>The child context</returns>
        internal static CdssExecutionStackFrame EnterChildFrame(CdssBaseObjectDefinition owner)
        {
            if (m_currentContext == null)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.WOULD_RESULT_INVALID_STATE, nameof(EnterChildFrame)));
            }
            m_currentContext = new CdssExecutionStackFrame(m_currentContext, owner);
            m_currentContext.Context.DebugSession?.EnterFrame(m_currentContext);
            return m_currentContext;
        }

        /// <summary>
        /// Dispose of the context
        /// </summary>
        public void Dispose()
        {
            m_currentContext = m_currentContext?.m_parent;
            this.m_context.DebugSession?.ExitFrame();
        }
    }
}


