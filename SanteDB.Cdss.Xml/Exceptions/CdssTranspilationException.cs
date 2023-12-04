using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Cdss.Xml.Exceptions
{
    /// <summary>
    /// Represents an exception while transpiling a CDSS rule
    /// </summary>
    public class CdssTranspilationException : Exception
    {

        public class CdssTranspileError
        {

            /// <summary>
            /// Gets the line number
            /// </summary>
            public int Line { get; }

            /// <summary>
            /// Gets the column
            /// </summary>
            public int Column { get; }

            /// <summary>
            /// Gets the message
            /// </summary>
            public string Message { get; }

            /// <summary>
            /// Create a new transpile rror
            /// </summary>
            public CdssTranspileError(IToken position, String errorMessage)
            {
                this.Line = position.Line;
                this.Column = position.Column;
                this.Message = errorMessage;
            }
        }


        /// <summary>
        /// Get the errors
        /// </summary>
        public IEnumerable<CdssTranspileError> Errors { get; }

        /// <summary>
        /// Creates a new transpilation exception
        /// </summary>
        public CdssTranspilationException(IEnumerable<CdssTranspileError> errors) : base($"Error Transpiling Library")
        {
            this.Errors = errors.ToList();    
        }

        /// <summary>
        /// Create a new transpilation exception
        /// </summary>
        public CdssTranspilationException(IToken token, string errorMessage) : this(new CdssTranspileError[] { new CdssTranspileError(token, errorMessage) })
        {
        }
    }
}
