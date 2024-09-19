/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 */
using System;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.XmlLinq
{
    /// <summary>
    /// XmlType binary expression
    /// </summary>
    [XmlType(nameof(XmlTypeBinaryExpression), Namespace = "http://santedb.org/cdss")]
    public class XmlTypeBinaryExpression : XmlBoundExpression
    {

        /// <summary>
        /// Serializer ctor
        /// </summary>
        public XmlTypeBinaryExpression()
        {

        }

        /// <summary>
        /// Constructs the type operand
        /// </summary>
        public XmlTypeBinaryExpression(TypeBinaryExpression expr) : base(expr)
        {
            QueryTypeXml = expr.TypeOperand.AssemblyQualifiedName;
        }

        /// <summary>
        /// Query the type of Xml
        /// </summary>
        [XmlAttribute("operandType")]
        public string QueryTypeXml { get; set; }

        /// <summary>
        /// Gets the type this expression is querying for
        /// </summary>
        public Type QueryType
        {
            get
            {
                return Type.GetType(QueryTypeXml);
            }
        }

        /// <summary>
        /// Gets the type check binary
        /// </summary>
        public override Type Type
        {
            get
            {
                return typeof(bool);
            }
        }

        /// <summary>
        /// Convert to an expression
        /// </summary>
        public override Expression ToExpression()
        {
            return Expression.TypeIs(Object?.ToExpression(), QueryType);
        }
    }
}