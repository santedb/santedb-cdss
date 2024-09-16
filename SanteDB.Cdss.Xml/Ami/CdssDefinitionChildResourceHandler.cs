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
using SanteDB.Cdss.Xml.Model;
using SanteDB.Core;
using SanteDB.Core.Cdss;
using SanteDB.Core.Interop;
using SanteDB.Core.Model.Query;
using SanteDB.Rest.Common;
using System;
using System.Collections.Specialized;

namespace SanteDB.Cdss.Xml.Ami
{
    /// <summary>
    /// Represents a child resource on the CDSS library definition used for resolving a definition
    /// </summary>
    public class CdssDefinitionChildResourceHandler : IApiChildResourceHandler
    {
        private readonly ICdssLibraryRepository m_cdssLibraryRepository;

        public CdssDefinitionChildResourceHandler(ICdssLibraryRepository cdssLibraryRepository)
        {
            this.m_cdssLibraryRepository = cdssLibraryRepository;
        }

        /// <inheritdoc/>
        public string Name => "_definition";

        /// <inheritdoc/>
        public Type PropertyType => typeof(CdssBaseObjectDefinition);

        /// <inheritdoc/>
        public ResourceCapabilityType Capabilities => ResourceCapabilityType.Get | ResourceCapabilityType.Search;

        /// <inheritdoc/>
        public ChildObjectScopeBinding ScopeBinding => ChildObjectScopeBinding.Class | ChildObjectScopeBinding.Instance;

        /// <inheritdoc/>
        public Type[] ParentTypes => new Type[] { typeof(ICdssLibraryRepositoryMetadata) };

        /// <inheritdoc/>
        public object Add(Type scopingType, object scopingKey, object item)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public object Get(Type scopingType, object scopingKey, object key)
        {
            // Scoping key 
            if (scopingKey is Guid scopedObjectUuid || Guid.TryParse(scopingKey?.ToString(), out scopedObjectUuid))
            {
                var scope = this.m_cdssLibraryRepository.Get(scopedObjectUuid, null);

            }
            else if (this.m_cdssLibraryRepository.TryResolveReference(key.ToString(), out var resolved))
            {
                return new CdssLibraryDefinitionInfo(resolved, true);
            }
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public IQueryResultSet Query(Type scopingType, object scopingKey, NameValueCollection filter)
        {
            // Scoping key 
            if (scopingKey is Guid scopedObjectUuid || Guid.TryParse(scopingKey?.ToString(), out scopedObjectUuid))
            {
                var scope = this.m_cdssLibraryRepository.Get(scopedObjectUuid, null);

            }
            else if (this.m_cdssLibraryRepository.TryResolveReference(filter["ref"], out var resolved))
            {
                return new object[] { new CdssLibraryDefinitionInfo(resolved, true) }.AsResultSet();
            }

            return null;
        }

        /// <inheritdoc/>
        public object Remove(Type scopingType, object scopingKey, object key)
        {
            throw new NotSupportedException();
        }
    }
}
