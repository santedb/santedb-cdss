/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-5-19
 */
using DynamicExpresso;
using SanteDB.Cdss.Xml.Model;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Map;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SanteDB.Cdss.Xml
{
    /// <summary>
    /// Parameter manager for the CDSS
    /// </summary>
    internal class CdssContext<TTarget>  : ICdssContext
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
        public CdssContext(TTarget target)
        {
            this.Target = target;
        }

        // Parameter values
        private readonly IDictionary<String, ParameterRegistration> m_variables = new Dictionary<String, ParameterRegistration>();
        private readonly IDictionary<String, Object> m_factCache = new ConcurrentDictionary<String, Object>();
        private readonly IDictionary<String, CdssComputableAssetDefinition> m_factDefinitions = new Dictionary<String, CdssComputableAssetDefinition>();

        /// <summary>
        /// Gets or sets the target of the context
        /// </summary>
        public TTarget Target { get; private set; }

        /// <summary>
        /// Get the variables
        /// </summary>
        public IEnumerable<String> Variables => m_variables.Keys;

        /// <summary>
        /// Property indexer for variable name
        /// </summary>
        /// <param name="variableName">The name of the variable to fetch</param>
        /// <returns>The variable value (if it is declared) for <paramref name="variableName"/></returns>
        public object this[string variableName]
        {
            get => this.GetValue(variableName);
            set => this.SetValue(variableName, value);
        }

        /// <summary>
        /// Sets the specified variable name
        /// </summary>
        public void SetValue(String parameterName, object value)
        {
            if (!this.m_variables.TryGetValue(parameterName, out ParameterRegistration registration))
            {
                registration = new ParameterRegistration()
                {
                    Type = value.GetType(),
                    Value = value
                };
                this.m_variables.Add(parameterName, registration);
            }

            registration.Value = value;
        }

        /// <summary>
        /// Get the parameter name
        /// </summary>
        public object GetValue(String parameterOrFactName)
        {
            if (this.m_variables.TryGetValue(parameterOrFactName, out ParameterRegistration registration) &&
                MapUtil.TryConvert(registration.Value, registration.Type, out object retVal))
            {
                return retVal;
            }
            else if(this.m_factCache.TryGetValue(parameterOrFactName, out var cache))
            {
                return cache;
            }
            else if(this.m_factDefinitions.TryGetValue(parameterOrFactName, out var defn))
            {
                var value = defn.Compute(this);
                this.m_factCache.Add(parameterOrFactName, value);
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get strongly type parameter
        /// </summary>
        public TValue GetValue<TValue>(String parameterOrFactName) => (TValue)this.GetValue(parameterOrFactName);

        /// <summary>
        /// Decalare a  variable
        /// </summary>
        public void Declare(string variableName, Type variableType)
        {
            if (!this.m_variables.ContainsKey(variableName))
            {
                this.m_variables.Add(variableName, new ParameterRegistration()
                {
                    Type = variableType,
                    Value = null
                });
            }
        }

        /// <summary>
        /// Declares a fact registration to the CDSS context
        /// </summary>
        public void Declare(CdssComputableAssetDefinition fact)
        {
            if (!String.IsNullOrEmpty(fact.Name))
            {
                this.m_factCache.Add(fact.Name, fact);
            }
            if (!String.IsNullOrEmpty(fact.Id))
            {
                this.m_factCache.Add($"#{fact.Id}", fact);
            }
        }

        /// <summary>
        /// Clear all evaluated fact values 
        /// </summary>
        public void ClearEvaluatedFacts()
        {
            this.m_factCache.Clear();
        }

        /// <summary>
        /// Get an expression interpreter for this CDSS context
        /// </summary>
        /// <returns></returns>
        internal Interpreter GetExpressionInterpreter()
        {
            var expressionInterpreter = new Interpreter(InterpreterOptions.LambdaExpressions | InterpreterOptions.Default | InterpreterOptions.LateBindObject)
                               .Reference(typeof(TimeSpan))
                               .Reference(typeof(Guid))
                               .Reference(typeof(DateTimeOffset));
            return expressionInterpreter;
        }
    }
}