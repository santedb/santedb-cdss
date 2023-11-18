using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using SanteDB.Cdss.Xml.Exceptions;
using System.IO;

namespace SanteDB.Cdss.Xml.Antlr
{
    /// <summary>
    /// CDSS Transpile Error Listener
    /// </summary>
    internal class CdssTranspileErrorListener : BaseErrorListener
    {
        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            throw new CdssTranspilationException(offendingSymbol, msg);
        }
    }
}