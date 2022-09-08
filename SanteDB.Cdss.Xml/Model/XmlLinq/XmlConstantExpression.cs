/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2022-5-30
 */
using System;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model.XmlLinq
{
    /// <summary>
    /// Represents an expression that is a constant
    /// </summary>
    [XmlInclude(typeof(int))]
    [XmlInclude(typeof(decimal))]
    [XmlInclude(typeof(Guid))]
    [XmlInclude(typeof(DateTime))]
    [XmlInclude(typeof(String))]
    [XmlInclude(typeof(bool))]
    [XmlType(nameof(XmlConstantExpression), Namespace = "http://santedb.org/cdss")]
    public class XmlConstantExpression : XmlExpression
    {
        /// <summary>
        /// Create constant expression
        /// </summary>
        public XmlConstantExpression()
        {

        }

        /// <summary>
        /// Create constant expression from the specitifed expression
        /// </summary>
        /// <param name="constantExpression"></param>
        public XmlConstantExpression(ConstantExpression expr)
        {
            this.Value = expr.Value;
        }

        /// <summary>
        /// Get the type of this expression
        /// </summary>
        public override Type Type
        {
            get
            {
                return this.Value?.GetType();
            }
        }

        /// <summary>
        /// Value of the constant expression
        /// </summary>
        [XmlElement("value")]
        public Object Value { get; set; }

        /// <summary>
        /// Create the specified constant expression
        /// </summary>
        public override Expression ToExpression()
        {
            return Expression.Constant(this.Value);
        }
    }
}
