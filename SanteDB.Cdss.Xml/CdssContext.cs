/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2021-8-5
 */

using SanteDB.Core.Model.Map;
using System;
using System.Collections.Generic;

namespace SanteDB.Cdss.Xml
{
    /// <summary>
    /// Parameter manager for the CDSS
    /// </summary>
    public class CdssContext<TModel> : ICdssContext
    {
        /// <summary>
        /// Parameter registration
        /// </summary>
        private class ParameterRegistration
        {
            /// <summary>
            /// Type of the parameter
            /// </summary>
            public Type Type { get; set; }

            /// <summary>
            /// Value of the parameter
            /// </summary>
            public object Value { get; set; }
        }

        /// <summary>
        /// Get the CDSS context
        /// </summary>
        public CdssContext()
        {
        }

        /// <summary>
        /// Gets the target
        /// </summary>
        public CdssContext(TModel target)
        {
            this.Target = target;
        }

        // Parameter values
        private Dictionary<String, ParameterRegistration> m_parameters = new Dictionary<String, ParameterRegistration>();

        /// <summary>
        /// Gets or sets the target of the context
        /// </summary>
        public TModel Target { get; private set; }

        /// <summary>
        /// Get the variables
        /// </summary>
        public IEnumerable<String> Variables => m_parameters.Keys;

        /// <summary>
        /// Sets the specified variable name
        /// </summary>
        public void Var(String parameterName, object value)
        {
            if (!this.m_parameters.TryGetValue(parameterName, out ParameterRegistration registration))
            {
                registration = new ParameterRegistration()
                {
                    Type = value.GetType(),
                    Value = value
                };
                this.m_parameters.Add(parameterName, registration);
            }

            registration.Value = value;
        }

        /// <summary>
        /// Get the parameter name
        /// </summary>
        public object Var(String parameterName)
        {
            if (this.m_parameters.TryGetValue(parameterName, out ParameterRegistration registration) &&
                MapUtil.TryConvert(registration.Value, registration.Type, out object retVal))
                return retVal;
            else
                return null;
        }

        /// <summary>
        /// Get strongly type parameter
        /// </summary>
        public TValue Var<TValue>(String parameterName)
        {
            if (this.m_parameters.TryGetValue(parameterName, out ParameterRegistration retVal))
                return (TValue)retVal.Value;
            return default(TValue);
        }

        /// <summary>
        /// Decalare a  variable
        /// </summary>
        public void Declare(string variableName, Type variableType)
        {
            if (!this.m_parameters.ContainsKey(variableName))
                this.m_parameters.Add(variableName, new ParameterRegistration()
                {
                    Type = variableType,
                    Value = null
                });
        }
    }
}