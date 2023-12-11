/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-5-19
 */
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.XmlLinq
{

    /// <summary>
    /// Gets or sets the binary expression operator
    /// </summary>
    [XmlType(nameof(BinaryOperatorType), Namespace = "http://santedb.org/cdss")]
    public enum BinaryOperatorType
    {
        /// <summary>
        /// The <see cref="BinaryExpression.Left"/> is equal to <see cref="BinaryExpression.Right"/>
        /// </summary>
        [XmlEnum("eq")]
        Equal,
        /// <summary>
        /// The <see cref="BinaryExpression.Left"/> is less than <see cref="BinaryExpression.Right"/>
        /// </summary>
        [XmlEnum("lt")]
        LessThan,
        /// <summary>
        /// The <see cref="BinaryExpression.Left"/> is less than or equal to <see cref="BinaryExpression.Right"/>
        /// </summary>
        [XmlEnum("lte")]
        LessThanOrEqual,
        /// <summary>
        /// The <see cref="BinaryExpression.Left"/> is greater than <see cref="BinaryExpression.Right"/>
        /// </summary>
        [XmlEnum("gt")]
        GreaterThan,
        /// <summary>
        /// The <see cref="BinaryExpression.Left"/> is greater than or equal to <see cref="BinaryExpression.Right"/>
        /// </summary>
        [XmlEnum("gte")]
        GreaterThanOrEqual,
        /// <summary>
        /// The <see cref="BinaryExpression.Left"/> is not equal to <see cref="BinaryExpression.Right"/>
        /// </summary>
        [XmlEnum("ne")]
        NotEqual,
        /// <summary>
        /// The <see cref="BinaryExpression.Left"/> must evaluate to true and the <see cref="BinaryExpression.Right"/> must evaluate to true
        /// </summary>
        [XmlEnum("and")]
        AndAlso,
        /// <summary>
        /// The <see cref="BinaryExpression.Left"/> must evaluate to true or the <see cref="BinaryExpression.Right"/> must evaluate to true
        /// </summary>
        [XmlEnum("or")]
        OrElse,
        /// <summary>
        /// The <see cref="BinaryExpression.Left"/> is to be added to the <see cref="BinaryExpression.Right"/>
        /// </summary>
        [XmlEnum("add")]
        Add,
        /// <summary>
        /// The <see cref="BinaryExpression.Left"/> is to be subtracted from the <see cref="BinaryExpression.Right"/>
        /// </summary>
        [XmlEnum("sub")]
        Subtract,
        /// <summary>
        /// The <see cref="BinaryExpression.Left"/> is an instance of <see cref="BinaryExpression.Right"/>
        /// </summary>
        [XmlEnum("is")]
        TypeIs
    }

    /// <summary>
    /// Represents an XML binary expression
    /// </summary>
    [XmlType(nameof(XmlBinaryExpression), Namespace = "http://santedb.org/cdss")]
    public class XmlBinaryExpression : XmlExpression
    {

        /// <summary>
        /// Represents a binary expression
        /// </summary>
        public XmlBinaryExpression()
        {
            Parts = new List<XmlExpression>();
        }

        /// <summary>
        /// Initialize context
        /// </summary>
        public override void InitializeContext(XmlExpression context)
        {
            base.InitializeContext(context);
            foreach (var itm in Parts)
            {
                itm.InitializeContext(this);
            }
        }

        /// <summary>
        /// Creates an XmlBinaryExpression from the specified binary expression
        /// </summary>
        public XmlBinaryExpression(BinaryExpression expr)
        {
            BinaryOperatorType opType = BinaryOperatorType.AndAlso;
            if (!Enum.TryParse(expr.NodeType.ToString(), out opType))
            {
                throw new ArgumentOutOfRangeException(nameof(Expression.NodeType));
            }

            Operator = opType;

            Parts = new List<XmlExpression>() {
                FromExpression(expr.Left),
                FromExpression(expr.Right)
            };
        }

        /// <summary>
        /// Gets or sets the operator of the binary expression
        /// </summary>
        [XmlAttribute("operator")]
        public BinaryOperatorType Operator { get; set; }

        /// <summary>
        /// Gets or sets the left side of the expression
        /// </summary>
        [XmlElement("constantExpression", typeof(XmlConstantExpression))]
        [XmlElement("memberExpression", typeof(XmlMemberExpression))]
        [XmlElement("parameterExpression", typeof(XmlParameterExpression))]
        [XmlElement("binaryExpression", typeof(XmlBinaryExpression))]
        [XmlElement("unaryExpression", typeof(XmlUnaryExpression))]
        [XmlElement("methodCallExpression", typeof(XmlMethodCallExpression))]
        [XmlElement("typeBinaryExpression", typeof(XmlTypeBinaryExpression))]
        public List<XmlExpression> Parts { get; set; }

        /// <summary>
        /// Get the type of the binary
        /// </summary>
        public override Type Type
        {
            get
            {
                switch (Operator)
                {
                    case BinaryOperatorType.Add:
                    case BinaryOperatorType.Subtract:
                        return Parts[0].Type;
                    default:
                        return typeof(bool);
                }
            }
        }

        /// <summary>
        /// Converts the object to an expression
        /// </summary>
        /// <returns></returns>
        public override Expression ToExpression()
        {
            if (Parts.Count < 2)
            {
                throw new InvalidOperationException("At least two parts must be in a expression tree");
            }

            // We basically take the two parts and construct those :) 
            ExpressionType type = ExpressionType.Add;
            if (!Enum.TryParse(Operator.ToString(), out type))
            {
                throw new ArgumentOutOfRangeException(nameof(ExpressionType));
            }

            Queue<XmlExpression> parts = new Queue<XmlExpression>(Parts);
            var retVal = Expression.MakeBinary(type, parts.Dequeue().ToExpression(), parts.Dequeue().ToExpression());

            while (parts.Count > 0)
            {
                retVal = Expression.MakeBinary(type, retVal, parts.Dequeue().ToExpression());
            }

            return retVal;
        }
    }
}
