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
 * User: fyfej
 * Date: 2023-11-27
 */
using System;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.XmlLinq
{

    /// <summary>
    /// Gets or sets the binary expression operator
    /// </summary>
    [XmlType(nameof(UnaryOperatorType), Namespace = "http://santedb.org/cdss")]
    public enum UnaryOperatorType
    {
        /// <summary>
        /// The operation is a NOT! operator
        /// </summary>
        [XmlEnum("not")]
        Not,
        /// <summary>
        /// The operation is a conversion
        /// </summary>
        [XmlEnum("convert")]
        Convert,
        /// <summary>
        /// The operation is a negation
        /// </summary>
        [XmlEnum("neg")]
        Negate,
        /// <summary>
        /// Th eoperation is a type as
        /// </summary>
        [XmlEnum("as")]
        TypeAs
    }

    /// <summary>
    /// Represents a unary expression
    /// </summary>
    [XmlType(nameof(XmlUnaryExpression), Namespace = "http://santedb.org/cdss")]
    public class XmlUnaryExpression : XmlBoundExpression
    {
        /// <summary>
        /// Creates a new unary expression
        /// </summary>
        public XmlUnaryExpression()
        {

        }

        /// <summary>
        /// Create the unary expression from a .net expression
        /// </summary>
        public XmlUnaryExpression(UnaryExpression expr) : base(expr)
        {
            UnaryOperatorType uop = UnaryOperatorType.Negate;
            if (!Enum.TryParse(expr.NodeType.ToString(), out uop))
            {
                throw new ArgumentOutOfRangeException(nameof(UnaryExpression.NodeType));
            }

            Operator = uop;
            TypeXml = expr.Type.AssemblyQualifiedName;
        }

        /// <summary>
        /// Gets or sets the unary operator
        /// </summary>
        [XmlAttribute("operator")]
        public UnaryOperatorType Operator { get; set; }

        /// <summary>
        /// Gets or sets the type
        /// </summary>
        [XmlAttribute("type")]
        public string TypeXml { get; set; }

        /// <summary>
        /// Gets the type of expression
        /// </summary>
        public override Type Type
        {
            get
            {
                switch (Operator)
                {
                    case UnaryOperatorType.Convert:
                    case UnaryOperatorType.TypeAs:
                        return Type.GetType(TypeXml);
                    default:
                        return Object?.Type;
                }
            }
        }

        /// <summary>
        /// Convert this to a .NET expression
        /// </summary>
        /// <returns></returns>
        public override Expression ToExpression()
        {
            ExpressionType uop = ExpressionType.Parameter;
            if (!Enum.TryParse(Operator.ToString(), out uop))
            {
                throw new ArgumentOutOfRangeException(nameof(ExpressionType));
            }

            return Expression.MakeUnary(uop, Object.ToExpression(), Type);
        }
    }
}