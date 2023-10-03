using SanteDB.Core.Model;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Actions
{
    /// <summary>
    /// CDSS action definition is a special kind of computable definition specifically intended to execute an action against the current context
    /// </summary>
    [XmlType(nameof(CdssActionDefinition), Namespace = "http://santedb.org/cdss")]
    public abstract class CdssActionDefinition : CdssBaseObjectDefinition
    {

        /// <summary>
        /// Execute the action against <paramref name="cdssContext"/>
        /// </summary>
        /// <param name="cdssContext">The CDSS context</param>
        internal abstract void Execute(CdssContext cdssContext);

    }
}