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
 * User: fyfej
 * Date: 2023-11-27
 */
using System;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// CDSS value types
    /// </summary>
    [XmlType(nameof(CdssValueType), Namespace = "http://santedb.org/cdss")]
    public enum CdssValueType
    {
        [XmlEnum("auto")]
        Unspecified = 0,
        /// <summary>
        /// The CDSS value type is <see cref="DateTimeOffset"/>
        /// </summary>
        [XmlEnum("date")]
        Date,
        /// <summary>
        /// The CDSS value type is an <see cref="int"/>
        /// </summary>
        [XmlEnum("integer")]
        Integer,
        /// <summary>
        /// Represents a 64 bit signed integer
        /// </summary>
        [XmlEnum("long")]
        Long,
        /// <summary>
        /// The CDSS value type is a <see cref="string"/>
        /// </summary>
        [XmlEnum("string")]
        String,
        /// <summary>
        /// The CDSS value type is a <see cref="bool"/>
        /// </summary>
        [XmlEnum("bool")]
        Boolean,
        /// <summary>
        /// The CDSS value type is a <see cref="float"/>
        /// </summary>
        [XmlEnum("real")]
        Real

    }
}