﻿/*
 * Based on OpenIZ, Copyright (C) 2015 - 2019 Mohawk College of Applied Arts and Technology
 * Copyright (C) 2019 - 2020, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2019-11-27
 */
using SanteDB.Core.Model.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// Protocol collection
    /// </summary>
    [XmlType(nameof(ProtocolCollection), Namespace = "http://santedb.org/cdss")]
    public class ProtocolCollection : DecisionSupportBaseElement
    {
        private static XmlSerializer s_xsz = XmlModelSerializerFactory.Current.CreateSerializer(typeof(ProtocolCollection));

        /// <summary>
        /// Loads the protocol collection from a stream
        /// </summary>
        public static ProtocolCollection Load(Stream s)
        {
            return s_xsz.Deserialize(s) as ProtocolCollection;
        }

        /// <summary>
        /// Gets or sets the protocol definitions
        /// </summary>
        [XmlElement("protocol")]
        public List<ProtocolDefinition> Protocols { get; set; }
    }
}
