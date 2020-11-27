﻿/*
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
 * Date: 2020-5-1
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Cdss.Xml
{
    /// <summary>
    /// Represents the interface to Cdss context
    /// </summary>
    public interface ICdssContext
    {

        /// <summary>
        /// Sets the variable value
        /// </summary>
        void Var(String varName, Object value);

        /// <summary>
        /// Gets the variable name
        /// </summary>
        Object Var(String varName);
    }
}
