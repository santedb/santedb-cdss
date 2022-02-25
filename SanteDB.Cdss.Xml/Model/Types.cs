using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// This class exists so variables can be retrieved via the Get() function from CDSS C# expressions
    /// </summary>
    internal static class Types
    {
        public const int Integer = 0;
        public const bool Boolean = false;
        public const String String = "";
        public const double Double = 0.0d;
        public static readonly DateTime DateTime = DateTime.MinValue;
    }

}
