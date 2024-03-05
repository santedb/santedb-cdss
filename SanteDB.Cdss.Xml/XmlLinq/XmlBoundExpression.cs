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
    /// Represents an XmlExpression bound to another expression
    /// </summary>
    [XmlType(nameof(XmlBoundExpression), Namespace = "http://santedb.org/cdss")]
    public abstract class XmlBoundExpression : XmlExpression
    {
        /// <summary>
        /// Creates the bound expression
        /// </summary>
        public XmlBoundExpression()
        {

        }

        /// <summary>
        /// Initialize context
        /// </summary>
        public override void InitializeContext(XmlExpression context)
        {
            base.InitializeContext(context);
            Object?.InitializeContext(this);
        }

        /// <summary>
        /// Creates the bound expression
        /// </summary>
        public XmlBoundExpression(Expression expr)
        {
            Object = FromExpression(expr);
        }

        /// <summary>
        /// Creates type bound expression
        /// </summary>
        /// <param name="expr"></param>
        public XmlBoundExpression(TypeBinaryExpression expr)
        {
            Object = FromExpression(expr.Expression);
        }

        /// <summary>
        /// Creates the bound expression
        /// </summary>
        public XmlBoundExpression(MemberExpression expr)
        {
            Object = FromExpression(expr.Expression);
        }

        /// <summary>
        /// Creates the bound expression
        /// </summary>
        public XmlBoundExpression(MethodCallExpression expr)
        {
            Object = FromExpression(expr.Object);
        }

        /// <summary>
        /// Creates the bound expression
        /// </summary>
        public XmlBoundExpression(UnaryExpression expr)
        {
            Object = FromExpression(expr.Operand);
        }

        /// <summary>
        /// Gets or sets the explicit type (for unbound methods)
        /// </summary>
        [XmlAttribute("staticClass")]
        public string StaticClassXml { get; set; }


        /// <summary>
        /// Gets the method class
        /// </summary>
        [XmlIgnore]
        public Type StaticClass
        {
            get
            {
                if (StaticClassXml == null)
                {
                    return null;
                }

                return Type.GetType(StaticClassXml);
            }
        }

        /// <summary>
        /// Gets or sets the expression
        /// </summary>
        [XmlElement("constantExpression", typeof(XmlConstantExpression))]
        [XmlElement("memberExpression", typeof(XmlMemberExpression))]
        [XmlElement("parameterExpression", typeof(XmlParameterExpression))]
        [XmlElement("binaryExpression", typeof(XmlBinaryExpression))]
        [XmlElement("unaryExpression", typeof(XmlUnaryExpression))]
        [XmlElement("methodCallExpression", typeof(XmlMethodCallExpression))]
        [XmlElement("typeBinaryExpression", typeof(XmlTypeBinaryExpression))]
        public XmlExpression Object { get; set; }
    }
}