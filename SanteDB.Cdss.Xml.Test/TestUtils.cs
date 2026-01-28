/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2024-11-25
 */
using SanteDB.Cdss.Xml.Antlr;
using SanteDB.Cdss.Xml.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Cdss.Xml.Test
{
    internal static class TestUtils
    {

        internal static CdssLibraryDefinition Load(String logicLibraryName)
        {
            logicLibraryName = $"SanteDB.Cdss.Xml.Test.Protocols.{logicLibraryName}";
            using (var ms = typeof(TestUtils).Assembly.GetManifestResourceStream(logicLibraryName))
            {
                if (logicLibraryName.EndsWith("xml"))
                {
                    return CdssLibraryDefinition.Load(ms);
                }
                else if (logicLibraryName.EndsWith("cdss"))
                {
                    return CdssLibraryTranspiler.Transpile(ms, true, logicLibraryName);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(logicLibraryName));
                }
            }
        }
    }
}
