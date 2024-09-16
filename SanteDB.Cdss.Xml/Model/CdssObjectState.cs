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
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// State of the CDSS object
    /// </summary>
    [XmlType(nameof(CdssObjectState), Namespace = "http://santedb.org/cdss")]
    public enum CdssObjectState
    {
        /// <summary>
        /// The object has no known state
        /// </summary>
        [XmlEnum("unk")]
        Unknown = 0,
        /// <summary>
        /// The object is intended for trial use
        /// </summary>
        [XmlEnum("trial-use")]
        TrialUse = 1,
        /// <summary>
        /// The object is active
        /// </summary>
        [XmlEnum("active")]
        Active = 2,
        /// <summary>
        /// The object is retired
        /// </summary>
        [XmlEnum("retired")]
        Retired = 3,
        /// <summary>
        /// The object should not be used
        /// </summary>
        [XmlEnum("dont-use")]
        DontUse = 4

    }
}