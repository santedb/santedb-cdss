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
using SanteDB;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.XmlLinq
{
    /// <summary>
    /// Represents a call expression
    /// </summary>
    [XmlType(nameof(XmlMethodCallExpression), Namespace = "http://santedb.org/cdss")]
    public class XmlMethodCallExpression : XmlBoundExpression
    {

        /// <summary>
        /// Call expression ctor
        /// </summary>
        public XmlMethodCallExpression()
        {
            Parameters = new XmlExpressionList();
        }

        /// <summary>
        /// Initialize context
        /// </summary>
        public override void InitializeContext(XmlExpression context)
        {
            base.InitializeContext(context);
            Parameters.InitializeContext(this);
        }

        /// <summary>
        /// Create call expression from .net call expression
        /// </summary>
        /// <param name="expr">The method call expression to represent in XML</param>
        public XmlMethodCallExpression(MethodCallExpression expr) : base(expr)
        {
            MethodName = expr.Method.Name;
            Parameters = new XmlExpressionList(expr.Arguments);

            // Static so we need to know where to find the thing
            if (expr.Method.IsStatic)
            {
                StaticClassXml = expr.Method.DeclaringType.AssemblyQualifiedName;
                if (expr.Method.IsGenericMethod)
                {
                    MethodTypeArgumentXml = expr.Method.GetGenericArguments().Select(o => o.AssemblyQualifiedName).ToArray();
                }
            }
        }

        /// <summary>
        /// Method type argument
        /// </summary>
        [XmlElement("methodTypeArgument")]
        public string[] MethodTypeArgumentXml { get; set; }


        /// <summary>
        /// Gets or sets the method name
        /// </summary>
        [XmlAttribute("methodName")]
        public string MethodName { get; set; }

        /// <summary>
        /// Represents the parameter list to the call
        /// </summary>
        [XmlElement("argument")]
        public XmlExpressionList Parameters { get; set; }

        /// <summary>
        /// Get the type of this method call
        /// </summary>
        public override Type Type
        {
            get
            {
                // Can we just go?
                if (MethodTypeArgumentXml == null)
                {
                    return (StaticClass ?? Object?.Type)?.GetRuntimeMethod(MethodName, Parameters.Item.Select(o => o.Type).ToArray())?.ReturnType;
                }
                else
                {
                    var mi = StaticClass.GetRuntimeMethods().FirstOrDefault(o => o.Name == MethodName && o.GetParameters().Length == Parameters.Item.Count);
                    var methodInfo = mi.MakeGenericMethod(MethodTypeArgumentXml.Select(o => Type.GetType(o)).ToArray());
                    return methodInfo.ReturnType;
                }
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
            else if (string.IsNullOrEmpty(MethodName))
            {
                throw new InvalidOperationException("Missing method name");
            }

            var parameters = Parameters.Item.Select(o => o.ToExpression());
            var methodInfo = (StaticClass ?? Object?.Type).GetRuntimeMethod(MethodName, parameters.Select(o => o.Type).ToArray());
            if (methodInfo == null && MethodTypeArgumentXml != null)
            {
                var mi = StaticClass.GetRuntimeMethods().FirstOrDefault(o => o.Name == MethodName && o.GetParameters().Length == Parameters.Item.Count);
                methodInfo = mi.MakeGenericMethod(MethodTypeArgumentXml.Select(o => Type.GetType(o)).ToArray());
            }
            if (methodInfo == null)
            {
                throw new InvalidOperationException(string.Format("Could not find method {0} in type {1}", MethodName, Object.Type));
            }

            return Expression.Call(Object?.ToExpression(), methodInfo, parameters.ToArray());
        }
    }
}