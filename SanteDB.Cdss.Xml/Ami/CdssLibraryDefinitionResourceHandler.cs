using SanteDB.Cdss.Xml.Model;
using SanteDB.Core.Cdss;
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
        public override Type Type => typeof(CdssLibraryDefinition);

        /// <inheritdoc/>
        public override Type Scope => Type.GetType("SanteDB.Rest.AMI.IAmiServiceContract, SanteDB.Rest.AMI");

        /// <inheritdoc/>
        public override ResourceCapabilityType Capabilities => ResourceCapabilityType.Get | ResourceCapabilityType.GetVersion |
            ResourceCapabilityType.Search | ResourceCapabilityType.Create | ResourceCapabilityType.CreateOrUpdate | ResourceCapabilityType.Update | ResourceCapabilityType.Delete;

        /// <inheritdoc/>
        [Demand(PermissionPolicyIdentifiers.CreateClinicalProtocolConfigurationDefinition)]
        public override object Create(object data, bool updateIfExists)
        {
            if(data is CdssLibraryDefinition definition)
            {
                var xmlLibrary = new XmlProtocolLibrary(definition);
                var retVal = this.m_cdssLibraryRepository.InsertOrUpdate(xmlLibrary);
                return xmlLibrary.Library;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(data), String.Format(ErrorMessages.ARGUMENT_INCOMPATIBLE_TYPE, typeof(CdssLibraryDefinition), data.GetType()));
            }
        }

        /// <inheritdoc/>
        [Demand(PermissionPolicyIdentifiers.DeleteClinicalProtocolConfigurationDefinition)]
        public override object Delete(object key)
        {
            var keyIsUuid = key is Guid uuid || Guid.TryParse(key.ToString(), out uuid);
            if(!keyIsUuid)
            {
                throw new ArgumentOutOfRangeException(nameof(key) , String.Format(ErrorMessages.INVALID_FORMAT, key, Guid.Empty));
            }
            var retVal = this.m_cdssLibraryRepository.Remove(uuid) as XmlProtocolLibrary;
            return retVal.Library;
        }

        /// <inheritdoc/>
        [Demand(PermissionPolicyIdentifiers.ReadMetadata)]
        public override object Get(object id, object versionId)
        {
            var keyIsUuid = id is Guid uuid || Guid.TryParse(id.ToString(), out uuid);
            if(!keyIsUuid)
            {
                throw new ArgumentOutOfRangeException(nameof(id), String.Format(ErrorMessages.INVALID_FORMAT, id, Guid.Empty));
            }
            var retVal = this.m_cdssLibraryRepository.Get(uuid) as XmlProtocolLibrary;
            if(retVal == null)
            {
                throw new KeyNotFoundException(id.ToString());
            }
            return retVal.Library;
        }

        /// <inheritdoc/>
        [Demand(PermissionPolicyIdentifiers.ReadMetadata)]
        public override IQueryResultSet Query(NameValueCollection queryParameters)
        {
            var query = QueryExpressionParser.BuildLinqExpression<ICdssLibrary>(queryParameters);
            return new TransformQueryResultSet<ICdssLibrary, CdssLibraryDefinition>(this.m_cdssLibraryRepository.Find(query), (a) =>
            {
                if (a is XmlProtocolLibrary pl)
                {
                    return pl.Library;
                }
                else
                {
                    return new CdssLibraryDefinition()
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Oid = a.Oid,
                        Uuid = a.Uuid,
                        Metadata = new CdssObjectMetadata()
                        {
                            Documentation = "This definition comes from a third party provider and cannot be represented here"
                        }
                    };
                }
            });
        }

        /// <inheritdoc/>
        [Demand(PermissionPolicyIdentifiers.AlterClinicalProtocolConfigurationDefinition)]
        public override object Update(object data)
        {
            if(data is CdssLibraryDefinition definition)
            {
                return (this.m_cdssLibraryRepository.InsertOrUpdate(new XmlProtocolLibrary(definition)) as XmlProtocolLibrary).Library;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(data), String.Format(ErrorMessages.ARGUMENT_INCOMPATIBLE_TYPE, typeof(CdssLibraryDefinition), data.GetType()));
            }
        }
    }
}
