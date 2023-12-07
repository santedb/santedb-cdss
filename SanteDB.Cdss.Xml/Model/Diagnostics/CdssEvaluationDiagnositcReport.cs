using SanteDB.Cdss.Xml.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.Diagnostics
{
    /// <summary>
    /// Represents a serialization of a <see cref="CdssDebugSessionData"/> object
    /// </summary>
    [XmlType(nameof(CdssEvaluationDiagnositcReport), Namespace = "http://santedb.org/cdss")]
    public class CdssEvaluationDiagnositcReport
    {
    }
}
