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
using System.Reflection;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.XmlLinq
{
    /// <summary>
    /// Represents a expression that access an expression
    /// </summary>
    [XmlType(nameof(XmlMemberExpression), Namespace = "http://santedb.org/cdss")]
    public class XmlMemberExpression : XmlBoundExpression
    {

        /// <summary>
        /// Creates a new member expression
        /// </summary>
        public XmlMemberExpression()
        {

        }

        /// <summary>
        /// Creates a new member expression from a .net expression
        /// </summary>
        /// <param name="expr"></param>
        public XmlMemberExpression(MemberExpression expr) : base(expr)
        {
            MemberName = expr.Member.Name;
            if (expr.Expression == null)
            {
                StaticClassXml = expr.Member.DeclaringType.AssemblyQualifiedName;
            }
        }

        /// <summary>
        /// Gets or sets the member name
        /// </summary>
        [XmlAttribute("memberName")]
        public string MemberName { get; set; }

        /// <summary>
        /// Get the type of this expression
        /// </summary>
        public override Type Type
        {
            get
            {
                return (StaticClass ?? Object?.Type).GetRuntimeProperty(MemberName)?.PropertyType ?? Object.Type.GetRuntimeField(MemberName)?.FieldType;
            }
        }

        /// <summary>
        /// Convert to expression
        /// </summary>
        public override Expression ToExpression()
        {
            // validate
            if (Object == null && StaticClass == null)
            {
                throw new InvalidOperationException("Bound object is required");
            }
            else if (string.IsNullOrEmpty(MemberName))
            {
                throw new InvalidOperationException("Missing method name");
            }

            MemberInfo memberInfo = (MemberInfo)(StaticClass ?? Object?.Type).GetRuntimeProperty(MemberName) ??
                (StaticClass ?? Object?.Type).GetRuntimeField(MemberName);
            if (memberInfo == null)
            {
                throw new InvalidOperationException(string.Format("Could not find member {0} in type {1}", MemberName, Object.Type));
            }

            return Expression.MakeMemberAccess(Object?.ToExpression(), memberInfo);
        }
    }
}
