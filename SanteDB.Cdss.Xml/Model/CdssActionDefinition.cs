using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
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
        /// <typeparam name="TContext">The type of target in the context</typeparam>
        /// <param name="cdssContext">The CDSS context</param>
        internal abstract void Execute<TContext>(CdssContext<TContext> cdssContext);

    }
}