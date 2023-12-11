using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Cdss.Xml.i18n;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SanteDB.Cdss.Xml.Antlr
{
    /// <summary>
    /// CDSS Transpile Error Listener
    /// </summary>
    internal class CdssTranspileErrorListener : BaseErrorListener
    {

        /// <summary>
        /// Errors 
        /// </summary>
        private readonly Stack<CdssTranspilationException.CdssTranspileError> m_errors = new Stack<CdssTranspilationException.CdssTranspileError>();

        public void ThrowIfHasErrors()
        {
            if(this.m_errors.Any())
            {
                throw new CdssTranspilationException(this.m_errors);
            }
        }

        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            this.m_errors.Push(new CdssTranspilationException.CdssTranspileError(offendingSymbol, msg));
        }
    }
}