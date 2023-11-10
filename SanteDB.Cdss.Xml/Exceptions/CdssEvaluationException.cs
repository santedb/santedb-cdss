/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-5-19
 */
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
        public CdssExecutionStackFrame CdssStack { get; }

        /// <summary>
        /// Create a new CDSS evaluation exception
        /// </summary>
        public CdssEvaluationException(String message, Exception cause) : base(message, cause)
        {
            this.CdssStack = CdssExecutionStackFrame.Current;
        }

        /// <summary>
        /// Create new evaluation exception with just a message
        /// </summary>
        public CdssEvaluationException(String message) : this(message, null)
        {
        }
        
        /// <inheritdoc/>
        public override string ToString()
        {
            var sb = new StringBuilder("CDSS:");
            sb.AppendFormat("{0}\r\n", this.Message);
            sb.Append("CDSS Stack:\r\n");
            var ctx = this.CdssStack;
            while (ctx != null)
            {
                sb.AppendFormat("\t{0}\r\n", ctx.Owner?.ToString() ?? this.CdssStack.Context.ToString());
                ctx = ctx.Parent;
            } 
            sb.AppendFormat("at: \r\n", this.StackTrace);
            return sb.ToString();
        }
    }
}
