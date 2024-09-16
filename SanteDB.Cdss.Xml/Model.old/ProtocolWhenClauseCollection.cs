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
using DynamicExpresso;
using Newtonsoft.Json;
using SanteDB.Core;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Cdss;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Serialization;
using SanteDB.Cdss.Xml.XmlLinq;

namespace SanteDB.Cdss.Xml.Model
{
    /// <summary>
    /// Represents a when clause
    /// </summary>
    [XmlType(nameof(ProtocolWhenClauseCollection), Namespace = "http://santedb.org/cdss")]
    [JsonObject(nameof(ProtocolWhenClauseCollection))]
    public class ProtocolWhenClauseCollection : DecisionSupportBaseElement
    {


        // Tracer
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(ProtocolWhenClauseCollection));

        /// <summary>
        /// Operator
        /// </summary>
        [XmlAttribute("evaluation"), JsonProperty("evaluation")]
        public BinaryOperatorType Operator { get; set; }

        /// <summary>
        /// Clause evelators
        /// </summary>
        [XmlElement("hdsiExpression", typeof(WhenClauseHdsiExpression))]
        [XmlElement("expressionGrouping", typeof(ProtocolWhenClauseCollection))]
        [XmlElement("linqXmlExpression", typeof(XmlLambdaExpression))]
        [XmlElement("linqExpression", typeof(String))]
        [XmlElement("reference", typeof(CdssObjectReference))]
        [JsonProperty("clause")]
        public List<object> Clause { get; set; }

        /// <summary>
        /// Gets the debugging view of this protocol
        /// </summary>
        [XmlIgnore, JsonIgnore]
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
                switch (itm)
                {
                    case ProtocolWhenClauseCollection pwcc:
                        clauseExpr = Expression.Invoke(pwcc.Compile<TData>(context), expressionParm);
                        break;
                    case WhenClauseHdsiExpression hdsiExpr:
                        var varDict = new Dictionary<String, Func<Object>>();
                        foreach (var varRef in context.Variables)
                        {
                            varDict.Add(varRef, () => st_contextReference?.Get(varRef));
                        }

                        clauseExpr = QueryExpressionParser.BuildLinqExpression<TData>(hdsiExpr.Expression.ParseQueryString(), "s", varDict, safeNullable: true, forceLoad: true, lazyExpandVariables: true);
                        clauseExpr = Expression.Invoke(clauseExpr, Expression.MakeMemberAccess(expressionParm, typeof(CdssContext<TData>).GetProperty("Target")));
                        if (hdsiExpr.NegationIndicator)
                        {
                            clauseExpr = Expression.Not(clauseExpr);
                        }

                        this.m_tracer.TraceVerbose("Converted WHEN {0} > {1}", hdsiExpr.Expression, clauseExpr);
                        break;
                    case XmlLambdaExpression xmlLambda:
                        (itm as XmlLambdaExpression).InitializeContext(null);
                        // replace parameter
                        clauseExpr = Expression.Invoke(((itm as XmlLambdaExpression).ToExpression() as LambdaExpression), expressionParm);
                        break;
                    case string str:
                        var interpreter = new Interpreter(InterpreterOptions.Default)
                        .Reference(typeof(TData))
                        .Reference(typeof(Guid))
                        .Reference(typeof(TimeSpan))
                        .Reference(typeof(Types))
                        .EnableReflection();
                        var linqAction = interpreter.ParseAsExpression<Func<CdssContext<TData>, bool>>(str, "_");
                        clauseExpr = Expression.Invoke(linqAction, expressionParm);
                        break;
                    case CdssObjectReference reference:
                        return reference.Resolve<ProtocolWhenClauseCollection>().Compile(context);
                }


                // Append to master expression
                if (body == null)
                {
                    body = clauseExpr;
                }
                else
                {
                    body = Expression.MakeBinary((ExpressionType)Enum.Parse(typeof(ExpressionType), this.Operator.ToString()), body, clauseExpr);
                }
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
            {
                this.Compile<TData>(context);
            }

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
        [XmlAttribute("negationIndicator"), JsonProperty("negationIndicator")]
        public bool NegationIndicator { get; set; }

        /// <summary>
        /// Represents the expression
        /// </summary>
        [XmlText, JsonProperty("expression")]
        public String Expression { get; set; }
    }

    /// <summary>
    /// References another when-clause
    /// </summary>
    [XmlType(nameof(CdssObjectReference), Namespace = "http://santedb.org/cdss")]
    public class CdssObjectReference
    {

        // Resolved object
        private object m_resolved = null;

        /// <summary>
        /// The identifier of the library from which the CDSS object reference should be pulled
        /// </summary>
        [XmlAttribute("library"), JsonProperty("library")]
        public String Library { get; set; }

        /// <summary>
        /// The reference to the other clause
        /// </summary>
        [XmlText, JsonProperty("reference")]
        public string Reference { get; set; }

        /// <summary>
        /// Resolve the object reference
        /// </summary>
        internal TTarget Resolve<TTarget>() where TTarget : DecisionSupportBaseElement
        {
            if(this.m_resolved is TTarget retVal)
            {
                return retVal;
            }
            var resolveService = ApplicationServiceContext.Current.GetService<ICdssAssetRepository>();
            this.m_resolved = retVal = resolveService?.Find(o=>o.Id == this.Library && o.Classification == CdssAssetClassification.DecisionSupportLibrary).OfType<ICdssLibraryAsset>().FirstOrDefault()?.ResolveElement<TTarget>(this.Reference);
            return retVal;
        }
    }
}