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
 * Date: 2024-6-21
 */
using RestSrvr;
using RestSrvr.Attributes;
using SanteDB.Cdss.Xml.Antlr;
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Cdss.Xml.Model;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Cdss;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Http;
using SanteDB.Core.i18n;
using SanteDB.Core.Interop;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Security;
using SanteDB.Core.Services;
using SanteDB.Rest.Common;
using SanteDB.Rest.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;

namespace SanteDB.Cdss.Xml.Ami
{
    /// <summary>
    /// Represents an AMI handler for the <see cref="CdssLibraryDefinition"/>
    /// </summary>
    public class CdssLibraryDefinitionResourceHandler : ChainedResourceHandlerBase, ICheckoutResourceHandler
    {
        private readonly ICdssLibraryRepository m_cdssLibraryRepository;
        private readonly IResourceCheckoutService m_checkService;

        /// <summary>
        /// DI constructor
        /// </summary>
        public CdssLibraryDefinitionResourceHandler(ICdssLibraryRepository cdssLibraryRepository, IResourceCheckoutService checkoutService = null, ILocalizationService localizationService = null) : base(localizationService)
        {
            this.m_cdssLibraryRepository = cdssLibraryRepository;
            this.m_checkService = checkoutService;
        }

        /// <inheritdoc/>
        public override string ResourceName => nameof(CdssLibraryDefinition);

        /// <inheritdoc/>
        public override Type Type => typeof(ICdssLibraryRepositoryMetadata);

        /// <inheritdoc/>
        public override Type Scope => Type.GetType("SanteDB.Rest.AMI.IAmiServiceContract, SanteDB.Rest.AMI");

        /// <inheritdoc/>
        public override ResourceCapabilityType Capabilities => ResourceCapabilityType.Get |
            ResourceCapabilityType.GetVersion |
            ResourceCapabilityType.History |
            ResourceCapabilityType.Search |
            ResourceCapabilityType.Create |
            ResourceCapabilityType.CreateOrUpdate |
            ResourceCapabilityType.Update |
            ResourceCapabilityType.Delete;

        /// <inheritdoc/>
        [Demand(PermissionPolicyIdentifiers.CreateClinicalProtocolConfigurationDefinition)]
        public override object Create(object data, bool updateIfExists) => this.InsertOrUpdateCdssLibrary(data);

        /// <summary>
        /// Perform the insert or update on the CDSS library
        /// </summary>
        private object InsertOrUpdateCdssLibrary(object data, Func<ICdssLibrary, bool> validator = null)
        {
            switch (data)
            {
                case CdssLibraryDefinition definition:
                    {

                        var xmlLibrary = new XmlProtocolLibrary(definition);
                        if (validator?.Invoke(xmlLibrary) == false)
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                        var retVal = this.m_cdssLibraryRepository.InsertOrUpdate(xmlLibrary);
                        return new CdssLibraryDefinitionInfo(retVal, true);
                    }
                case CdssLibraryDefinitionInfo definitionInfo:
                    {
                        var xmlLibrary = new XmlProtocolLibrary(definitionInfo.Library);
                        if (validator?.Invoke(xmlLibrary) == false)
                        {
                            throw new ArgumentOutOfRangeException();
                        }

                        var retVal = this.m_cdssLibraryRepository.InsertOrUpdate(xmlLibrary);
                        return new CdssLibraryDefinitionInfo(retVal, true);
                    }
                case String stringData:
                    {
                        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(stringData)))
                        {
                            try
                            {
                                var library = CdssLibraryTranspiler.Transpile(ms, true);
                                var xmlLibrary = new XmlProtocolLibrary(library);
                                if (validator?.Invoke(xmlLibrary) == false)
                                {
                                    throw new ArgumentOutOfRangeException();
                                }

                                var retVal = this.m_cdssLibraryRepository.InsertOrUpdate(xmlLibrary);
                                return new CdssLibraryDefinitionInfo(retVal, true);
                            }
                            catch (CdssTranspilationException e)
                            {
                                throw new DetectedIssueException(Core.BusinessRules.DetectedIssuePriorityType.Error, "error.cdss.transpile", e.Message, Guid.Empty, e);
                            }
                        }
                    }
                case IEnumerable<MultiPartFormData> multiForm:
                    {
                        var sourceFile = multiForm.FirstOrDefault(o => o.IsFile);
                        using (var ms = new MemoryStream(sourceFile.Data))
                        {
                            try
                            {
                                CdssLibraryDefinition library = null;
                                bool fromSource = sourceFile.FileName.EndsWith(".cdss");
                                if (fromSource)
                                {
                                    library = CdssLibraryTranspiler.Transpile(ms, true);
                                }
                                else
                                {
                                    library = CdssLibraryDefinition.Load(ms);
                                }
                                var xmlLibrary = new XmlProtocolLibrary(library);
                                if (validator?.Invoke(xmlLibrary) == false)
                                {
                                    throw new ArgumentOutOfRangeException();
                                }

                                var retVal = this.m_cdssLibraryRepository.InsertOrUpdate(xmlLibrary);
                                return new CdssLibraryDefinitionInfo(retVal, fromSource);
                            }
                            catch (CdssTranspilationException e)
                            {
                                var issues = e.Errors.Select(o => new DetectedIssue(DetectedIssuePriorityType.Error, "error.cdss.transpile", $"{o.Message} @{o.Line}:{o.Column}", Guid.Empty)).ToList();
                                issues.Insert(0, new DetectedIssue(Core.BusinessRules.DetectedIssuePriorityType.Error, "error.cdss.transpile", e.Message, Guid.Empty));
                                throw new DetectedIssueException(issues);
                            }
                        }
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException(nameof(data), String.Format(ErrorMessages.ARGUMENT_INCOMPATIBLE_TYPE, typeof(CdssLibraryDefinition), data.GetType()));
                    }
            }
        }

        /// <inheritdoc/>
        [Demand(PermissionPolicyIdentifiers.DeleteClinicalProtocolConfigurationDefinition)]
        public override object Delete(object key)
        {
            var keyIsUuid = key is Guid uuid || Guid.TryParse(key.ToString(), out uuid);
            if (!keyIsUuid)
            {
                throw new ArgumentOutOfRangeException(nameof(key), String.Format(ErrorMessages.INVALID_FORMAT, key, Guid.Empty));
            }
            var retVal = this.m_cdssLibraryRepository.Remove(uuid) as XmlProtocolLibrary;
            return retVal.Library;
        }

        /// <inheritdoc/>
        [Demand(PermissionPolicyIdentifiers.ReadMetadata)]
        public override object Get(object id, object versionId)
        {
            var keyIsUuid = id is Guid uuid || Guid.TryParse(id.ToString(), out uuid);
            var versionIdSpecified = (versionId is Guid versionUuid || Guid.TryParse(versionId?.ToString(), out versionUuid)) && versionUuid != Guid.Empty;
            if (!keyIsUuid)
            {
                throw new ArgumentOutOfRangeException(nameof(id), String.Format(ErrorMessages.INVALID_FORMAT, id, Guid.Empty));
            }

            var retVal = this.m_cdssLibraryRepository.Get(uuid, versionUuid) as XmlProtocolLibrary;
            if (retVal == null)
            {
                throw new KeyNotFoundException(id.ToString());
            }

            switch (RestOperationContext.Current.IncomingRequest.QueryString["_format"])
            {
                case "xml":
                    var library = retVal.Library.Clone();
                    library.TranspileSourceReference = null;
                    RestOperationContext.Current.OutgoingResponse.AddHeader("Content-Disposition", $"attachment;filename=\"{retVal.Name}.xml\"");
                    RestOperationContext.Current.OutgoingResponse.ContentType = "application/xml";
                    return library;
                case "txt":
                    RestOperationContext.Current.OutgoingResponse.AddHeader("Content-Disposition", $"attachment;filename=\"{retVal.Name}.cdss\"");
                    RestOperationContext.Current.OutgoingResponse.ContentType = "text/plain";
                    return new MemoryStream(retVal.Library.TranspileSourceReference?.OriginalSource ?? Encoding.UTF8.GetBytes(CdssLibraryTranspiler.UnTranspile(retVal.Library)));
                default:
                    return new CdssLibraryDefinitionInfo(retVal, versionIdSpecified);
            }
        }

        /// <inheritdoc/>
        [Demand(PermissionPolicyIdentifiers.ReadMetadata)]
        [UrlParameter("oid", typeof(String), "The OID of the library to retrieve")]
        [UrlParameter("name", typeof(String), "The human readable name to filter")]
        [UrlParameter("uuid", typeof(String), "The declared UUID of the library")]
        [UrlParameter("id", typeof(String), "The logical CDSS identifier for the library")]
        public override IQueryResultSet Query(NameValueCollection queryParameters)
        {
            var originalQuery = new NameValueCollection(queryParameters);
            queryParameters.Remove("modifiedOn");
            queryParameters.Remove("creationTime");
            queryParameters.Remove("obsoletionTime");

            if (!String.IsNullOrEmpty(queryParameters["_id"]))
            {
                queryParameters.Add("uuid", queryParameters["_id"]);
                queryParameters.Remove("_id");
            }

            var query = QueryExpressionParser.BuildLinqExpression<ICdssLibrary>(queryParameters);
            var repositoryResult = this.m_cdssLibraryRepository.Find(query);

            var storageFilter = originalQuery
                    .ToDictionary()
                    .Where(o => o.Key == "modifiedOn" || o.Key == "obsoletionTime" || o.Key == "creationTime")
                    .ToDictionary(o => o.Key == "modifiedOn" ? "creationTime" : o.Key, o => (object)o.Value);
            if (storageFilter.Any())
            {
                var filter = QueryExpressionParser.BuildLinqExpression<ICdssLibraryRepositoryMetadata>(storageFilter.ToNameValueCollection()).Compile();
                repositoryResult = repositoryResult.OfType<ICdssLibrary>().ToArray().Where(o => filter(o.StorageMetadata)).AsResultSet();
            }
            return new TransformQueryResultSet<ICdssLibrary, CdssLibraryDefinitionInfo>(repositoryResult, (a) => new CdssLibraryDefinitionInfo(a, true));
        }

        /// <inheritdoc/>
        [Demand(PermissionPolicyIdentifiers.AlterClinicalProtocolConfigurationDefinition)]
        public override object Update(object data)
        {
            return this.InsertOrUpdateCdssLibrary(data, (lib) =>
            {
                return lib.Uuid == Guid.Empty || RestOperationContext.Current.IncomingRequest.RawUrl.Contains(lib.Uuid.ToString());
            });
        }

        /// <inheritdoc/>
        [Demand(PermissionPolicyIdentifiers.AlterClinicalProtocolConfigurationDefinition)]
        public object CheckOut(object key)
        {
            _ = key is Guid uuid || Guid.TryParse(key.ToString(), out uuid);
            var library = this.m_cdssLibraryRepository.Get(uuid, null);
            if (library != null &&
                this.m_checkService?.Checkout<ICdssLibrary>(uuid) == false)
            {
                throw new ObjectLockedException();
            }
            return null;
        }

        /// <inheritdoc/>
        [Demand(PermissionPolicyIdentifiers.AlterClinicalProtocolConfigurationDefinition)]
        public object CheckIn(object key)
        {
            var match = _ = key is Guid uuid || Guid.TryParse(key.ToString(), out uuid);
            var library = this.m_cdssLibraryRepository.Get(uuid, null);
            if (library != null &&
                this.m_checkService?.Checkin<ICdssLibrary>(uuid) == false)
            {
                throw new ObjectLockedException();
            }
            return null;
        }
    }
}
