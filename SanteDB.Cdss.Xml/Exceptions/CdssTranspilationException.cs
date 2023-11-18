using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Cdss.Xml.Exceptions
{
    /// <summary>
    /// Represents an exception while transpiling a CDSS rule
    /// </summary>
    public class CdssTranspilationException : Exception
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
        /// Creates a new transpilation exception
        /// </summary>
        public CdssTranspilationException(IToken position, String errorMessage) : base(errorMessage)
        {
            this.Line = position.Line;
            this.Column = position.Column;

        }
    }
}
