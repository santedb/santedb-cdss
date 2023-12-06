using Newtonsoft.Json;
using SanteDB.Cdss.Xml.Model;
using SanteDB.Core.Cdss;
using SanteDB.Core.Model.Interfaces;
using System;
using System.Net.Sockets;
using System.Xml;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Ami
{

    /// <summary>
    /// A REST based wrapper for the <see cref="CdssLibraryDefinition"/>
    /// </summary>
    [XmlType(nameof(CdssLibraryDefinitionInfo), Namespace = "http://santedb.org/cdss")]
    [XmlRoot(nameof(CdssLibraryDefinitionInfo), Namespace = "http://santedb.org/cdss")]
    public class CdssLibraryDefinitionInfo : IVersionedData, IBaseData
    {

        /// <summary>
        /// Serializer CTOR
        /// </summary>
        public CdssLibraryDefinitionInfo()
        {
            
        }

        /// <summary>
        /// Create a cdss library definition info
        /// </summary>
        public CdssLibraryDefinitionInfo(ICdssLibrary libraryEntry, bool summaryOnly)
        {
            this.VersionKey = libraryEntry.StorageMetadata?.VersionKey;
            this.VersionSequence = libraryEntry.StorageMetadata?.VersionSequence;
            this.IsHeadVersion = libraryEntry.StorageMetadata?.IsHeadVersion ?? true;
            this.CreatedByKey = libraryEntry.StorageMetadata?.CreatedByKey;
            this.Key = libraryEntry.StorageMetadata?.Key;
            this.CreationTime = libraryEntry.StorageMetadata?.CreationTime ?? DateTimeOffset.Now;
            this.Library = summaryOnly ?
                new CdssLibraryDefinition()
                {
                    Id = libraryEntry.Id,
                    Uuid = libraryEntry.Uuid,
                    Oid = libraryEntry.Oid,
                    Name = libraryEntry.Name,
                    UuidSpecified = true,
                    Metadata = new CdssObjectMetadata() { Documentation =  libraryEntry.Documentation, Version = libraryEntry.Version }
                } : (libraryEntry as XmlProtocolLibrary)?.Library.Clone() as CdssLibraryDefinition;
            if (this.Library.TranspileSourceReference != null)
            {
                this.Library.TranspileSourceReference = new CdssTranspileMapMetaData()
                {
                    SourceFileName = this.Library.TranspileSourceReference.SourceFileName,
                    StartPosition = this.Library.TranspileSourceReference.StartPosition,
                    EndPoisition = this.Library.TranspileSourceReference.EndPoisition
                };
            }

            this.ObsoletedByKey = libraryEntry.StorageMetadata?.ObsoletedByKey;
            this.ObsoletionTime = libraryEntry.StorageMetadata?.ObsoletionTime;
            this.PreviousVersionKey = libraryEntry.StorageMetadata?.PreviousVersionKey;
        }


        /// <inheritdoc/>
        [XmlElement("sequence"), JsonProperty("sequence")]
        public long? VersionSequence { get; set; }

        /// <inheritdoc/>
        [XmlElement("version"), JsonProperty("version")]
        public Guid? VersionKey { get; set; }

        /// <inheritdoc/>
        [XmlElement("previousVersion"), JsonProperty("previousVersion")]
        public Guid? PreviousVersionKey { get; set; }

        /// <inheritdoc/>
        [XmlElement("head"), JsonProperty("head")]
        public bool IsHeadVersion { get; set; }

        /// <inheritdoc/>
        [XmlElement("id"), JsonProperty("id")]
        public Guid? Key { get; set; }

        /// <inheritdoc/>
        [XmlIgnore, JsonIgnore]
        public string Tag => this.VersionKey.ToString();

        /// <inheritdoc/>
        [XmlIgnore, JsonIgnore]
        public DateTimeOffset ModifiedOn => this.ObsoletionTime ?? this.CreationTime;

        /// <summary>
        /// Gets or sets the library
        /// </summary>
        [XmlElement("library"), JsonProperty("library")]
        public CdssLibraryDefinition Library { get; set; }

        /// <inheritdoc/>
        [XmlElement("createdBy"), JsonProperty("createdBy")]
        public Guid? CreatedByKey { get; set; }

        /// <inheritdoc/>
        [XmlElement("obsoletedBy"), JsonProperty("obsoletedBy")]
        public Guid? ObsoletedByKey { get; set; }

        /// <summary>
        /// Serialization for <see cref="CreationTime"/>
        /// </summary>
        [XmlElement("creationTime"), JsonProperty("creationTime")]
        public string CreationTimeXml
        {
            get => XmlConvert.ToString(this.CreationTime);
            set
            {
                if(!string.IsNullOrEmpty(value))
                {
                    this.CreationTime = XmlConvert.ToDateTimeOffset(value);
                }
            }
        }

        /// <inheritdoc/>
        [XmlIgnore, JsonIgnore]
        public DateTimeOffset CreationTime { get; set; }

        /// <summary>
        /// XmlSerialization for <see cref="ObsoletionTime"/>
        /// </summary>
        [XmlElement("obsoletionTime"), JsonProperty("obsoletionTime")]
        public string ObsoletionTimeXml
        {
            get => this.ObsoletionTime.HasValue ? XmlConvert.ToString(this.ObsoletionTime.Value) : null;
            set
            {
                if(!String.IsNullOrEmpty(value))
                {
                    this.ObsoletionTime = XmlConvert.ToDateTimeOffset(value);
                }
            }
        }

        /// <inheritdoc/>
        [XmlIgnore, JsonIgnore]
        public DateTimeOffset? ObsoletionTime { get; set; }
    }
}