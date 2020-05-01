using SanteDB.Core.Model.Map;
using System;
using System.Collections.Generic;
using System.Text;

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
        /// Decalre a  variable
        /// </summary>
        public void Declare(string variableName, Type variableType)
        {
            if(!this.m_parameters.ContainsKey(variableName))
                this.m_parameters.Add(variableName, new ParameterRegistration()
                {
                    Type = variableType,
                    Value = null
                });
        }
    }

}
