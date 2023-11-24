using Newtonsoft.Json.Serialization;
using RestSrvr;
using RestSrvr.Attributes;
using SanteDB.Cdss.Xml.Antlr;
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Cdss.Xml.Model;
using SanteDB.Core.Cdss;
using SanteDB.Core.Exceptions;
using SanteDB.Core.Http;
using SanteDB.Core.i18n;
using SanteDB.Core.Interop;
using SanteDB.Core.Model.Attributes;
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
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace SanteDB.Cdss.Xml.Ami
{
    /// <summary>
    /// Represents an AMI handler for the <see cref="CdssLibraryDefinition"/>
    /// </summary>
    public class CdssLibraryDefinitionResourceHandler : ChainedResourceHandlerBase
    {
        private readonly ICdssLibraryRepository m_cdssLibraryRepository;

        /// <summary>
        /// DI constructor
        /// </summary>
        public CdssLibraryDefinitionResourceHandler(ICdssLibraryRepository cdssLibraryRepository, ILocalizationService localizationService) : base(localizationService)
        {
            this.m_cdssLibraryRepository = cdssLibraryRepository;
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
                                return new CdssLibraryDefinitionInfo(xmlLibrary, fromSource);
                            }
                            catch (CdssTranspilationException e)
                            {
                                throw new DetectedIssueException(Core.BusinessRules.DetectedIssuePriorityType.Error, "error.cdss.transpile", e.Message, Guid.Empty, e);
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

            switch(RestOperationContext.Current.IncomingRequest.QueryString["_format"])
            {
                case "xml":
                    RestOperationContext.Current.OutgoingResponse.AddHeader("Content-Disposition", $"attachment;filename=\"{retVal.Name}.xml\"");
                    RestOperationContext.Current.OutgoingResponse.ContentType = "application/xml";
                    return retVal.Library;
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
            var query = QueryExpressionParser.BuildLinqExpression<ICdssLibrary>(queryParameters);
            return new TransformQueryResultSet<ICdssLibrary, CdssLibraryDefinitionInfo>(this.m_cdssLibraryRepository.Find(query), (a) => new CdssLibraryDefinitionInfo(a, true));
        }

        /// <inheritdoc/>
        [Demand(PermissionPolicyIdentifiers.AlterClinicalProtocolConfigurationDefinition)]
        public override object Update(object data)
        {
            return this.InsertOrUpdateCdssLibrary(data, (lib) =>
            {

                return lib.Uuid == Guid.Empty || RestOperationContext.Current.IncomingRequest.RawUrl.Contains(lib.Uuid.ToString());
                return true;
            });
        }
    }
}
