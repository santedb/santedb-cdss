﻿/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.XmlLinq
{
    /// <summary>
    /// Represents an expression collection
    /// </summary>
    [XmlType(nameof(XmlExpressionList), Namespace = "http://santedb.org/cdss")]
    public class XmlExpressionList
    {

        /// <summary>
        /// Represents an expression list
        /// </summary>
        public XmlExpressionList()
        {

        }
        /// <summary>
        /// Initialize context
        /// </summary>
        public virtual void InitializeContext(XmlExpression context)
        {
            foreach (var itm in Item)
            {
                itm.InitializeContext(context);
            }
        }

        /// <summary>
        /// Creates a new xml expression list
        /// </summary>
        public XmlExpressionList(IEnumerable<Expression> expr)
        {
            Item = new List<XmlExpression>(expr.Select(o => XmlExpression.FromExpression(o)));
        }

        /// <summary>
        /// Represents the list of items
        /// </summary>
        [XmlElement("constantExpression", typeof(XmlConstantExpression))]
        [XmlElement("memberExpression", typeof(XmlMemberExpression))]
        [XmlElement("parameterExpression", typeof(XmlParameterExpression))]
        [XmlElement("binaryExpression", typeof(XmlBinaryExpression))]
        [XmlElement("unaryExpression", typeof(XmlUnaryExpression))]
        [XmlElement("methodCallExpression", typeof(XmlMethodCallExpression))]
        [XmlElement("lambdaExpression", typeof(XmlLambdaExpression))]
        [XmlElement("typeBinaryExpression", typeof(XmlTypeBinaryExpression))]
        public List<XmlExpression> Item { get; set; }


    }
}