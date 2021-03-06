﻿/*
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2021-2-9
 */
using ExpressionEvaluator;
using SanteDB.Cdss.Xml.Model.XmlLinq;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.Roles;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// Represents a when clause
    /// </summary>
    [XmlType(nameof(ProtocolWhenClauseCollection), Namespace = "http://santedb.org/cdss")]
    public class ProtocolWhenClauseCollection
    {

        // Tracer
        private Tracer m_tracer = Tracer.GetTracer(typeof(ProtocolWhenClauseCollection));

        /// <summary>
        /// Operator 
        /// </summary>
        [XmlAttribute("evaluation")]
        public BinaryOperatorType Operator { get; set; }

        /// <summary>
        /// Clause evelators
        /// </summary>
        [XmlElement("hdsiExpression", typeof(WhenClauseHdsiExpression))]
        [XmlElement("expressionGrouping", typeof(ProtocolWhenClauseCollection))]
        [XmlElement("linqXmlExpression", typeof(XmlLambdaExpression))]
        [XmlElement("linqExpression", typeof(String))]
        public List<object> Clause { get; set; }

        /// <summary>
        /// Gets the debugging view of this protocol
        /// </summary>
        [XmlIgnore]
        public String DebugView { get; private set; }

        // Lock
        private Object m_lockObject = new object();

        /// <summary>
        /// Gets the current context reference value 
        /// </summary>
        [ThreadStatic]
        private static ICdssContext st_contextReference;

        /// <summary>
        /// Compile the expression
        /// </summary>
        public Expression Compile<TData>(CdssContext<TData> context)
        {
            ParameterExpression expressionParm = Expression.Parameter(typeof(CdssContext<TData>), "_scope");
            Expression body = null;
            // Iterate and perform binary operations
            foreach (var itm in this.Clause)
            {

                Expression clauseExpr = null;
                if (itm is ProtocolWhenClauseCollection)
                {
                    clauseExpr = Expression.Invoke((itm as ProtocolWhenClauseCollection).Compile<TData>(context), expressionParm);
                }
                else if (itm is WhenClauseHdsiExpression)
                {
                    var varDict = new Dictionary<String, Func<Object>>();
                    foreach (var varRef in context.Variables)
                        varDict.Add(varRef, () => st_contextReference.Var(varRef));

                    var hdsiExpr = itm as WhenClauseHdsiExpression;
                    clauseExpr = QueryExpressionParser.BuildLinqExpression<TData>(NameValueCollection.ParseQueryString(hdsiExpr.Expression), varDict);
                    clauseExpr = Expression.Invoke(clauseExpr, Expression.MakeMemberAccess(expressionParm, typeof(CdssContext<TData>).GetProperty("Target")));
                    if (hdsiExpr.NegationIndicator)
                        clauseExpr = Expression.Not(clauseExpr);
                    this.m_tracer.TraceVerbose("Converted WHEN {0} > {1}", hdsiExpr.Expression, clauseExpr);
                }
                else if (itm is XmlLambdaExpression)
                {
                    var xmlLambda = itm as XmlLambdaExpression;
                    (itm as XmlLambdaExpression).InitializeContext(null);
                    // replace parameter
                    clauseExpr = Expression.Invoke(((itm as XmlLambdaExpression).ToExpression() as LambdaExpression), expressionParm);
                }
                else
                {
                    CompiledExpression<bool> exp = new CompiledExpression<bool>(itm as String);
                    exp.TypeRegistry = new TypeRegistry();
                    exp.TypeRegistry.RegisterDefaultTypes();
                    exp.TypeRegistry.RegisterType<TData>();
                    exp.TypeRegistry.RegisterType<Guid>();
                    exp.TypeRegistry.RegisterType<TimeSpan>();

                    //exp.TypeRegistry.RegisterSymbol("data", expressionParm);
                    exp.ScopeCompile<CdssContext<TData>>();
                    //Func<TData, bool> d = exp.ScopeCompile<TData>();
                    var linqAction = exp.GenerateLambda<Func<CdssContext<TData>, bool>, CdssContext<TData>>(true, false);
                    clauseExpr = Expression.Invoke(linqAction, expressionParm);
                    //clauseExpr = Expression.Invoke(d, expressionParm);
                }

                // Append to master expression
                if (body == null)
                    body = clauseExpr;
                else
                    body = Expression.MakeBinary((ExpressionType)Enum.Parse(typeof(ExpressionType), this.Operator.ToString()), body, clauseExpr);
            }

            // Wrap and compile
            var objParm = Expression.Parameter(typeof(Object));
            var bodyCondition = Expression.Lambda<Func<CdssContext<TData>, bool>>(body, expressionParm);
            var invoke = Expression.Invoke(
                bodyCondition,
                Expression.Convert(objParm, typeof(CdssContext<TData>))
            );
            var uncompiledExpression = Expression.Lambda<Func<Object, bool>>(invoke, objParm);
            this.DebugView = uncompiledExpression.ToString();
            this.m_compiledExpression = uncompiledExpression.Compile();
            return uncompiledExpression;
        }

        // Compiled
        private Func<Object, bool> m_compiledExpression = null;

        /// <summary>
        /// Evaluate the "when" clause
        /// </summary>
        public bool Evaluate<TData>(CdssContext<TData> context)
        {

            if (this.m_compiledExpression == null)
                this.Compile<TData>(context);

            lock (m_lockObject)
            {
                st_contextReference = context;
                return this.m_compiledExpression.Invoke(context);
            }
        }
    }

    /// <summary>
    /// Represents a simple HDSI expression
    /// </summary>
    [XmlType(nameof(WhenClauseHdsiExpression), Namespace = "http://santedb.org/cdss")]
    public class WhenClauseHdsiExpression
    {

        /// <summary>
        /// Only when the data element DOES NOT match 
        /// </summary>
        [XmlAttribute("negationIndicator")]
        public bool NegationIndicator { get; set; }

        /// <summary>
        /// Represents the expression
        /// </summary>
        [XmlText]
        public String Expression { get; set; }

    }
}