/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: fyfej
 * Date: 2023-11-27
 */
using Antlr4.Runtime;
using SanteDB.Cdss.Xml.Exceptions;
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
            if (this.m_errors.Any())
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