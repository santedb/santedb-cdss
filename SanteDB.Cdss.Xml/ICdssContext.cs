using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Cdss.Xml
{
    /// <summary>
    /// Represents a CDSS context which can be used to retrieve or set variables
    /// </summary>
    /// <remarks>
    /// This is only a wrapper for HDSI expressions so that variables can be fetched from the context
    /// </remarks>
    internal interface ICdssContext
    {

        
        /// <summary>
        /// Gets a variable value by name
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <returns></returns>
        object GetValue(String name);

    }
}
