using SanteDB.Cdss.Xml.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Cdss.Xml.Exceptions
{
    /// <summary>
    /// Clinical decision support evaluation exception
    /// </summary>
    public class CdssEvaluationException : Exception
    {

        /// <summary>
        /// Gets the protocol which caused this exception
        /// </summary>
        public ProtocolDefinition Protocol { get; }

        /// <summary>
        /// Create a new CDSS evaluation exception
        /// </summary>
        public CdssEvaluationException(String message, ProtocolDefinition protocol, Exception cause) : base(message, cause)
        {
            this.Protocol = protocol;
        }
    }
}
