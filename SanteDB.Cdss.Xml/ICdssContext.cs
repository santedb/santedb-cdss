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
