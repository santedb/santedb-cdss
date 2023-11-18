using SanteDB.Cdss.Xml.Model.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// Implements are CDSS definitions which have an <see cref="CdssActionCollectionDefinition"/>
    /// </summary>
    internal interface IHasCdssActions
    {

        /// <summary>
        /// Gets or sets the actions
        /// </summary>
        CdssActionCollectionDefinition Actions { get; set; }
    }
}
