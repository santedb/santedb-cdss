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
 */
using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Antlr;
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Cdss.Xml.Model;
using SanteDB.Cdss.Xml.Model.Expressions;
using SanteDB.Core;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Cdss;
using SanteDB.Core.Interop;
using SanteDB.Core.Model;
using SanteDB.Core.Model.Parameters;
using SanteDB.Rest.Common;
using SanteDB.Rest.Common.Operations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using static SanteDB.Cdss.Xml.Ami.CdssSymbolLookupOperation;


namespace SanteDB.Cdss.Xml.Ami
{
    /// <summary>
    /// Cdss validation result
    /// </summary>
    [XmlType(nameof(CdssSymbolLookupResult), Namespace = "http://santedb.org/cdss")]
    [XmlRoot(nameof(CdssSymbolLookupResult), Namespace = "http://santedb.org/cdss")]
    [JsonObject(nameof(CdssSymbolLookupResult))]
    public class CdssSymbolLookupResult
    {

        public CdssSymbolLookupResult()
        {

        }

        public CdssSymbolLookupResult(List<CdssSymbolInfo> symbolInfos)
        {
            this.Symbols = symbolInfos;
        }

        /// <summary>
        /// Detected issues
        /// </summary>
        [XmlElement("symbol"), JsonProperty("symbol")]
        public List<CdssSymbolInfo> Symbols { get; set; }

    }


    /// <summary>
    /// Represents a child operation where the CDSS layer fetches the available symbols which are in scope
    /// </summary>
    public class CdssSymbolLookupOperation : IApiChildOperation
    {
        
        public class CdssSymbolInfo : CdssBaseObjectDefinition
        {

            public CdssSymbolInfo()
            {
                
            }

            public CdssSymbolInfo(ICdssLibrary cdssLibrary)
            {
                this.Id = cdssLibrary.Id;
                this.Name = cdssLibrary.Name;
                this.Oid = cdssLibrary.Oid;
                this.Metadata = new CdssObjectMetadata()
                {
                    Documentation = cdssLibrary.Documentation,
                    Version = cdssLibrary.Version
                };
                this.TypeName = "CdssLibrary";
            }

            public CdssSymbolInfo(CdssBaseObjectDefinition cdssBaseObjectDefinition) : base()
            {
                this.Id = cdssBaseObjectDefinition.Id;
                this.Name = cdssBaseObjectDefinition.Name;
                this.Oid = cdssBaseObjectDefinition.Oid;
                this.Metadata = new CdssObjectMetadata()
                {
                    Documentation = cdssBaseObjectDefinition.Metadata?.Documentation,
                    Version = cdssBaseObjectDefinition.Metadata?.Version
                };
                this.TypeName = cdssBaseObjectDefinition.GetType().GetSerializationName();
            }

            /// <summary>
            /// Gets the type name
            /// </summary>
            [XmlElement("typeName"), JsonProperty("typeName")]
            public String TypeName { get; set; }

            public override IEnumerable<DetectedIssue> Validate(CdssExecutionContext context)
            {
                yield break;
            }
        }

        private readonly ICdssLibraryRepository m_cdssRepository;

        /// <inheritdoc/>
        public CdssSymbolLookupOperation(ICdssLibraryRepository cdssLibraryRepository)
        {
            this.m_cdssRepository = cdssLibraryRepository;
        }

        /// <inheritdoc/>
        public string Name => "symbol";

        /// <inheritdoc/>
        public ChildObjectScopeBinding ScopeBinding => ChildObjectScopeBinding.Class;

        /// <inheritdoc />
        public Type[] ParentTypes => new Type[] { typeof(ICdssLibraryRepositoryMetadata) };

        /// <inheritdoc/>
        public object Invoke(Type scopingType, object scopingKey, ParameterCollection parameters)
        {
            // validate the library
            if (!parameters.TryGet("definition", out string definition))
            {
                throw new ArgumentNullException("definition");
            }

            _ = parameters.TryGet("name", out string fileName);

            List<CdssSymbolInfo> retVal = new List<CdssSymbolInfo>();
            try
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(definition)))
                {
                    var transpiled = CdssLibraryTranspiler.Transpile(ms, true, fileName);


                    var scopedLibraries = transpiled.Include.Select(o => this.m_cdssRepository.ResolveReference(o)).OfType<XmlProtocolLibrary>().Select(o => o.Library).ToList();
                    scopedLibraries.Add(transpiled);

                    retVal.AddRange(scopedLibraries.SelectMany(o => o.Definitions).Union(scopedLibraries.SelectMany(o=>o.Definitions).OfType<CdssDecisionLogicBlockDefinition>().Where(o=>o.Definitions != null).SelectMany(o=>o.Definitions)).Select(o => new CdssSymbolInfo(o)));
                }
            }
            catch (Exception e)
            {

            }
            retVal.AddRange(this.m_cdssRepository.Find(o => true).ToArray().Select(o => new CdssSymbolInfo(o)));
            return new CdssSymbolLookupResult(retVal);
        }
    }
}
