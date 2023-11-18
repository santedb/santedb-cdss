﻿using Antlr4.Runtime;
using SanteDB.Cdss.Xml.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace SanteDB.Cdss.Xml.Antlr
{
    /// <summary>
    /// An class which transpiles string data from the CDSS text file format into an executable <see cref="CdssLibraryDefinition"/>
    /// </summary>
    public static class CdssLibraryTranspiler
    {
        /// <summary>
        /// Transpiles the text format CDSS from <paramref name="inputStream"/> into a <see cref="CdssLibraryDefinition"/>
        /// </summary>
        /// <param name="inputStream">The input stream containing the plain text CDSS definition</param>
        /// <param name="includeSourceMap">True if the output should include the source map</param>
        /// <returns>The transpiled CDSS library</returns>
        public static CdssLibraryDefinition Transpile(Stream inputStream, bool includeSourceMap)
        {
            var antlrStream = new AntlrInputStream(inputStream);
            var lexer = new CdssLibraryLexer(antlrStream);
            var processedTokens = new CommonTokenStream(lexer);
            var parser = new CdssLibraryParser(processedTokens);
            parser.RemoveErrorListeners();
            lexer.RemoveErrorListeners();
            var errorListener = new CdssTranspileErrorListener();
            parser.AddErrorListener(errorListener);
            var result = parser.library();
            
            // Process
            var visitor = new CdssLibraryVisitor(includeSourceMap, (inputStream as FileStream)?.Name ?? ":memory:");
            return visitor.VisitLibrary(result);
        }

    }
}
