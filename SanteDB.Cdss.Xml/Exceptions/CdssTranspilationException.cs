/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using System;
using System.Collections.Generic;
using System.Linq;

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
