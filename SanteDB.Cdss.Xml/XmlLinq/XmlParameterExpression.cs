/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2024-6-21
 */
using System;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.XmlLinq
{
    /// <summary>
    /// Represents an expression that accesses a named parameter
    /// </summary>
    [XmlType(nameof(XmlParameterExpression), Namespace = "http://santedb.org/cdss")]
    public class XmlParameterExpression : XmlExpression
    {
        // Expression
        private ParameterExpression m_expression;

        /// <summary>
        /// Creates the parameter expression
        /// </summary>
        public XmlParameterExpression()
        {

        }

        /// <summary>
        /// Represents a parameter expression
        /// </summary>
        public XmlParameterExpression(ParameterExpression expr)
        {
            m_expression = expr;
            ParameterName = expr.Name;
            TypeXml = expr.Type.AssemblyQualifiedName;
        }

        /// <summary>
        /// Gets or sets the type 
        /// </summary>
        [XmlAttribute("type")]
        public string TypeXml { get; set; }

        /// <summary>
        /// Gets or sets the parameter name
        /// </summary>
        [XmlAttribute("parameterName")]
        public string ParameterName { get; set; }

        /// <summary>
        /// Gets the type of object
        /// </summary>
        public override Type Type
        {
            get
            {
                return Type.GetType(TypeXml);
            }
        }

        /// <summary>
        /// Create a parameter expression
        /// </summary>
        public override Expression ToExpression()
        {
            if (m_expression != null)
            {
                return m_expression;
            }

            if (Type == null)
            {
                throw new InvalidOperationException("Type not set");
            }

            // Is there some parameter in the parent context?
            XmlExpression xe = Parent;
            while (xe != null && m_expression == null)
            {
                m_expression = (xe as XmlLambdaExpression)?.Parameters.Find(o => o.ParameterName == ParameterName)?.ToExpression() as ParameterExpression;
                xe = xe.Parent;
            }

            if (m_expression == null)
            {
                m_expression = Expression.Parameter(Type, ParameterName);
            }

            return m_expression;
        }
    }
}