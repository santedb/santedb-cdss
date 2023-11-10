using SanteDB.Core.BusinessRules;
using SanteDB.Core.i18n;
using SanteDB.Core.Model;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Actions
{
    /// <summary>
    /// CDSS action definition is a special kind of computable definition specifically intended to execute an action against the current context
    /// </summary>
    [XmlType(nameof(CdssActionDefinition), Namespace = "http://santedb.org/cdss")]
    public abstract class CdssActionDefinition : CdssBaseObjectDefinition
    {

        /// <summary>
        /// Throw an appropriate exception if the CDSS engine is in an invalid state
        /// </summary>
        /// <exception cref="ArgumentNullException">When the context is not provided</exception>
        protected void ThrowIfInvalidState()
        {
            if (CdssExecutionStackFrame.Current == null)
            {
                throw new InvalidOperationException(ErrorMessages.WOULD_RESULT_INVALID_STATE);
            }
        }

        /// <summary>
        /// Execute the action against the current <see cref="CdssExecutionStackFrame"/> frame
        /// </summary>
        internal abstract void Execute();

        /// <inheritdoc/>
        public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
        {
            yield break;
        }
    }
}