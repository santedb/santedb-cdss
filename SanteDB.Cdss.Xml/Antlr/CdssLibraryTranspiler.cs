﻿using Antlr4.Runtime;
using SanteDB.Cdss.Xml.Model;
using SanteDB.Cdss.Xml.Model.Actions;
using SanteDB.Cdss.Xml.Model.Assets;
using SanteDB.Cdss.Xml.Model.Expressions;
using SanteDB.Core.Model.Serialization;
using SharpCompress;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SanteDB.Cdss.Xml.Antlr
{
    /// <summary>
    /// An class which transpiles string data from the CDSS text file format into an executable <see cref="CdssLibraryDefinition"/>
    /// </summary>
    public static class CdssLibraryTranspiler
    {
        /// <summary>
        /// Transpiles the text format CDSS from <paramref name="inputStream"/> into a <see cref="CdssLibraryDefinition"/>
        /// </summary>
        /// <param name="inputStream">The input stream containing the plain text CDSS definition</param>
        /// <param name="includeSourceMap">True if the output should include the source map</param>
        /// <returns>The transpiled CDSS library</returns>
        public static CdssLibraryDefinition Transpile(Stream inputStream, bool includeSourceMap)
        {
            var antlrStream = new AntlrInputStream(inputStream);
            var lexer = new CdssLibraryLexer(antlrStream);
            var processedTokens = new CommonTokenStream(lexer);
            var parser = new CdssLibraryParser(processedTokens);
            parser.RemoveErrorListeners();
            lexer.RemoveErrorListeners();
            var errorListener = new CdssTranspileErrorListener();
            parser.AddErrorListener(errorListener);
            var result = parser.library();

            // Process
            var visitor = new CdssLibraryVisitor(includeSourceMap, (inputStream as FileStream)?.Name ?? ":memory:");
            return visitor.VisitLibrary(result);
        }

        /// <summary>
        /// Un-transpile the library definition
        /// </summary>
        public static String UnTranspile(CdssLibraryDefinition cdssLibraryDefinition)
        {

            using (var sw = new StringWriter())
            {
                sw.WriteLine("// This file has been reverse-engineered from a transpiled library definition");
                sw.WriteLine("// and does not represent the original source of the decision support logic");
                sw.WriteLine("// Generated by SanteDB v{0} on {1:o}", typeof(CdssLibraryDefinition).Assembly.GetName().Version, DateTime.Now);
                cdssLibraryDefinition.EmitCdssText(sw, 0);
                return sw.ToString();
            }
        }

        private static void EmitCdssText(this CdssLibraryDefinition cdssLibrary, StringWriter writer, int indentation = 0)
        {
            var indentationStr = new String('\t', indentation);
            cdssLibrary.Include?.ForEach(o =>
            {
                if (o.StartsWith("#"))
                    writer.WriteLine("{0}include <{1}>", indentationStr, o.Substring(1));
                else
                    writer.WriteLine("{0}include \"{1}\"", indentationStr, o);
            });

            writer.WriteLine("{0}define library \"{1}\"", indentationStr, cdssLibrary.Name);
            cdssLibrary.EmitHavingStatements(writer, indentation);
            cdssLibrary.Metadata?.EmitMetadata(writer, indentation + 1);

            writer.WriteLine("{0}\tas", indentationStr);

            cdssLibrary.Definitions.ForEach(d =>
            {
                switch (d)
                {
                    case CdssDecisionLogicBlockDefinition l:
                        l.EmitCdssText(writer, indentation + 1);
                        break;
                    case CdssDatasetDefinition s:
                        s.EmitCdssText(writer, indentation + 1);
                        break;
                }
            });
            writer.WriteLine("{0}end library", indentationStr);
        }

        private static void EmitCdssText(this CdssDecisionLogicBlockDefinition cdssDecisionLogic, StringWriter writer, int indentation = 0)
        {
            var indentationStr = new String('\t', indentation);
            writer.WriteLine("{0}define logic \"{1}\"", indentationStr, cdssDecisionLogic.Name);
            cdssDecisionLogic.EmitHavingStatements(writer, indentation);
            writer.Write("{0}\thaving context {1}", indentationStr, cdssDecisionLogic.Context.TypeXml);
            if (cdssDecisionLogic.When != null)
            {
                writer.Write(" when ");
                cdssDecisionLogic.When.WhenComputation.EmitCdssText(writer, indentation + 2);
                writer.WriteLine();
            }
            cdssDecisionLogic.Metadata?.EmitMetadata(writer, indentation + 1);


            writer.WriteLine("{0}\tas", indentationStr);

            cdssDecisionLogic.Definitions?.ForEach(o => o.EmitCdssText(writer, indentation + 1));

            writer.WriteLine("{0}end logic", indentationStr);
        }

        private static void EmitCdssText(this CdssComputableAssetDefinition assetDefinition, StringWriter writer, int indentation = 0)
        {
            switch(assetDefinition)
            {
                case CdssFactAssetDefinition fact:
                    fact.EmitCdssText(writer, indentation);
                    break;
                case CdssModelAssetDefinition model:
                    model.EmitCdssText(writer, indentation);
                    break;
                case CdssProtocolAssetDefinition protocol:
                    protocol.EmitCdssText(writer, indentation);
                    break;
                case CdssRuleAssetDefinition rule:
                    rule.EmitCdssText(writer, indentation);
                    break;
            }
        }

        private static void EmitCdssText(this CdssFactAssetDefinition cdssFact, StringWriter writer, int indentation = 0)
        {
            var indentationStr = new String('\t', indentation);
            writer.WriteLine("{0}define fact \"{1}\"", indentationStr, cdssFact.Name);
            cdssFact.EmitHavingStatements(writer, indentation);
            switch(cdssFact.ValueType)
            {
                case CdssValueType.Boolean:
                    writer.WriteLine("{0}\thaving type bool", indentationStr);
                    break;
                case CdssValueType.Date:
                    writer.WriteLine("{0}\thaving type date", indentationStr);
                    break;
                case CdssValueType.Integer:
                    writer.WriteLine("{0}\thaving type int", indentationStr);
                    break;
                case CdssValueType.Long:
                    writer.WriteLine("{0}\thaving type long", indentationStr);
                    break;
                case CdssValueType.Real:
                    writer.WriteLine("{0}\thaving type real", indentationStr);
                    break;
                case CdssValueType.String:
                    writer.WriteLine("{0}\thaving type string", indentationStr);
                    break;
            }

            if(cdssFact.IsNegated)
            {
                writer.WriteLine("{0}\thaving negation true", indentationStr);
            }
            if(cdssFact.Priority != 0)
            {
                writer.WriteLine("{0}\thaving priority {1}", indentationStr, cdssFact.Priority);
            }
            cdssFact.Metadata?.EmitMetadata(writer, indentation + 1);


            writer.WriteLine("{0}as", indentationStr);

            writer.Write("{0}\t", indentationStr);
            cdssFact.FactComputation.EmitCdssText(writer, indentation + 1);

            writer.WriteLine("\r\n{0}end fact", indentationStr);
        }

        private static void EmitCdssText(this CdssModelAssetDefinition cdssModel, StringWriter writer, int indentation = 0)
        {
            var indentationStr = new String('\t', indentation);
            writer.WriteLine("{0}define model \"{1}\"", indentationStr, cdssModel.Name);
            cdssModel.EmitHavingStatements(writer, indentation);
            if(cdssModel.Model is string str)
            {
                writer.WriteLine("{0}\thaving format json", indentationStr);
            }
            else
            {
                writer.WriteLine("{0}\thaving format xml", indentationStr);
            }
            cdssModel.Metadata?.EmitMetadata(writer, indentation + 1);

            writer.WriteLine("{0}as\r\n\t{0}$$", indentationStr);

            if(cdssModel.Model is string str1)
            {
                writer.Write(str1);
            }
            else
            {
                using (var xw = XmlWriter.Create(writer, new XmlWriterSettings()
                {
                    Indent = true,
                    CloseOutput = false,
                    Encoding = Encoding.UTF8
                }))
                {
                    var xsz = XmlModelSerializerFactory.Current.CreateSerializer(cdssModel.Model.GetType());
                    xsz.Serialize(xw, cdssModel.Model);
                }
            }


            writer.WriteLine("\r\n\t{0}$$\r\n{0}end model", indentationStr);

        }

        private static void EmitCdssText(this CdssRuleAssetDefinition cdssRule, StringWriter writer, int indentation = 0)
        {
            var indentationStr = new String('\t', indentation);
            writer.WriteLine("{0}define rule \"{1}\"", indentationStr, cdssRule.Name);
            cdssRule.EmitHavingStatements(writer, indentation);
            if (cdssRule.Priority != 0)
            {
                writer.WriteLine("{0}\thaving priority {1}", indentationStr, cdssRule.Priority);
            }
            cdssRule.Metadata?.EmitMetadata(writer, indentation + 1);
            writer.WriteLine("{0}as", indentationStr);

            if(cdssRule.When != null)
            {
                writer.WriteLine("{0}\twhen", indentationStr);
                writer.Write("{0}\t\t", indentationStr);
                cdssRule.When.WhenComputation.EmitCdssText(writer, indentation + 2);
                writer.WriteLine();
            }

            writer.WriteLine("{0}\tthen", indentationStr);
            cdssRule.Actions.EmitCdssText(writer, indentation + 2);
            writer.WriteLine("{0}end rule", indentationStr);
        }

        private static void EmitCdssText(this CdssActionCollectionDefinition actionCollection, StringWriter writer, int indentation = 0)
        {
            var indentationStr = new String('\t', indentation);
            actionCollection.Actions?.ForEach(a =>
            {
                switch(a)
                {
                    case CdssRuleReferenceActionDefinition ruleRef:
                        writer.Write("{0}apply ", indentationStr);
                        if(ruleRef.RuleName.StartsWith("#"))
                        {
                            writer.WriteLine("<{0}>", ruleRef.RuleName.Substring(1));
                        }
                        else
                        {
                            writer.WriteLine("\"{0}\"", ruleRef.RuleName);
                        }
                        break;
                    case CdssRepeatActionDefinition repeat:
                        repeat.EmitCdssText(writer, indentation);
                        break;
                    case CdssProposeActionDefinition propose:
                        propose.EmitCdssText(writer, indentation);
                        break;
                    case CdssPropertyAssignActionDefinition assign:
                        assign.EmitCdssText(writer, indentation);
                        break;
                    case CdssIssueActionDefinition raise:
                        raise.EmitCdssText(writer, indentation);
                        break;
                    case CdssInlineRuleActionDefinition rule:
                        rule.EmitCdssText(writer, indentation);
                        break;
                }

            });
        }
        private static void EmitCdssText(this CdssRepeatActionDefinition cdssRepeat, StringWriter writer, int indentation = 0)
        {
            var indentationStr = new String('\t', indentation);
            writer.Write("{0}repeat ", indentationStr);
            if(cdssRepeat.Until != null)
            {
                writer.Write("until ");
                cdssRepeat.Until.WhenComputation.EmitCdssText(writer, indentation);
            }
            else
            {
                writer.Write("for {0} iterations ", cdssRepeat.Iterations);
                if(!String.IsNullOrEmpty(cdssRepeat.IterationVariable))
                {
                    writer.Write(" track-by {0}", cdssRepeat.IterationVariable);
                }
            }

            cdssRepeat.Actions?.EmitCdssText(writer, indentation + 1);

            writer.WriteLine("{0}end repeat", indentationStr);
                 
        }

        private static void EmitCdssText(this CdssProposeActionDefinition cdssPropose, StringWriter writer, int indentation = 0)
        {
            var indentationStr = new String('\t', indentation);
            writer.Write("{0}propose ", indentationStr);
            if(!String.IsNullOrEmpty(cdssPropose.Name))
            {
                writer.Write("\"{0}\"", cdssPropose.Name);
            }

            if (!String.IsNullOrEmpty(cdssPropose.Model.ReferencedModel))
            {
                writer.WriteLine(" having model \"{0}\"", cdssPropose.Model.ReferencedModel);
            }
            else
            {
                writer.WriteLine(" having model ");
                if (cdssPropose.Model.Model is String str)
                {
                    writer.WriteLine("{0}having format json\r\n{0}as\r\n{0}$$\r\n\t{1}\r\n{0}$$\r\n{0}end model", indentationStr, str);
                }
                else
                {
                    writer.WriteLine("{0}having format xml\r\n{0}as\r\n{0}$$", indentationStr);
                    using (var xw = XmlWriter.Create(writer, new XmlWriterSettings()
                    {
                        Indent = true,
                        CloseOutput = false,
                        Encoding = Encoding.UTF8
                    }))
                    {
                        var xsz = XmlModelSerializerFactory.Current.CreateSerializer(cdssPropose.Model.Model.GetType());
                        xsz.Serialize(xw, cdssPropose.Model.Model);
                    }
                    writer.WriteLine("{0}$$\r\n{0}end model", indentationStr);
                }
            }
            cdssPropose.Metadata?.EmitMetadata(writer, indentation + 1);
            writer.WriteLine("{0}as", indentationStr);

            cdssPropose.Assignment.ForEach(o => o.EmitCdssText(writer, indentation + 1));

            writer.WriteLine("{0}end propose", indentationStr);

        }

        private static void EmitCdssText(this CdssPropertyAssignActionDefinition cdssAssign, StringWriter writer, int indentation = 0)
        {
            var indentationStr = new String('\t', indentation);
            writer.Write("{0}assign ", indentationStr);
            switch(cdssAssign.ContainedExpression)
            {
                case string s:
                    writer.Write("const \"{0}\"", s);
                    break;
                case CdssExpressionDefinition expr:
                    expr.EmitCdssText(writer, indentation);
                    break;
            }

            writer.Write(" to {0}", cdssAssign.Path);
            if(cdssAssign.OverwriteValue)
            {
                writer.Write(" overwrite");
            }
            writer.WriteLine();
        }

        private static void EmitCdssText(this CdssIssueActionDefinition cdssIssue, StringWriter writer, int indentation = 0)
        {
            var indentationStr = new String('\t', indentation);
            writer.WriteLine("{0}raise having priority {1} ", indentationStr, cdssIssue.IssueToRaise.Priority == Core.BusinessRules.DetectedIssuePriorityType.Error ? "danger" : cdssIssue.IssueToRaise.Priority == Core.BusinessRules.DetectedIssuePriorityType.Warning ? "warn" : "info");
            if(cdssIssue.IssueToRaise.TypeKey != Guid.Empty)
            {
                writer.WriteLine("{0}\thaving type {{{1}}}", indentationStr, cdssIssue.IssueToRaise.TypeKey);
            }
            if(!String.IsNullOrEmpty(cdssIssue.Id))
            {
                writer.Write("{0}\thaving id <{1}>", indentationStr, cdssIssue.Id);
            }
            cdssIssue.Metadata?.EmitMetadata(writer, indentation + 1);

            writer.WriteLine("{0}$$", indentationStr);
            writer.WriteLine("\t{0}{1}", indentationStr, cdssIssue.IssueToRaise.Text);
            writer.WriteLine("{0}$$", indentationStr);
        }

        private static void EmitCdssText(this CdssInlineRuleActionDefinition cdssInlineRule, StringWriter writer, int indentation = 0)
        {
            var indentationStr = new String('\t', indentation);

            // Define an inline rule
            writer.WriteLine("{0}rule", indentationStr);
            cdssInlineRule.Metadata?.EmitMetadata(writer, indentation + 1);
            writer.WriteLine("{0}as", indentationStr);
            if(cdssInlineRule.When != null)
            {
                writer.Write("{0}\twhen", indentationStr);
                cdssInlineRule.When.WhenComputation.EmitCdssText(writer, indentation + 1);
                writer.WriteLine();
            }
            writer.WriteLine("{0}\tthen", indentationStr);
            cdssInlineRule.Actions.EmitCdssText(writer, indentation + 2);
            writer.WriteLine("{0}end rule");
        }

        private static void EmitCdssText(this CdssProtocolAssetDefinition cdssProtocol, StringWriter writer, int indentation = 0)
        {
            var indentationStr = new String('\t', indentation);
            writer.WriteLine("{0}define protocol \"{1}\"", indentationStr, cdssProtocol.Name);
            cdssProtocol.EmitHavingStatements(writer, indentation);
            if (cdssProtocol.Priority != 0)
            {
                writer.WriteLine("{0}\thaving priority {1}", indentationStr, cdssProtocol.Priority);
            }
            cdssProtocol.Scopes?.ForEach(o =>
            {
                if (!String.IsNullOrEmpty(o.Id))
                {
                    writer.WriteLine("{0}\thaving scope <{1}>", indentationStr, o.Id);
                }
                else if(!String.IsNullOrEmpty(o.Name))
                {
                    writer.WriteLine("{0}\thaving scope \"{1}\"", indentationStr, o.Name);
                }
            });
            cdssProtocol.Metadata?.EmitMetadata(writer, indentation + 1);
            writer.WriteLine("{0}as", indentationStr);

            if (cdssProtocol.When != null)
            {
                writer.WriteLine("{0}\twhen", indentationStr);
                writer.Write("{0}\t\t", indentationStr);
                cdssProtocol.When.WhenComputation.EmitCdssText(writer, indentation + 2);
                writer.WriteLine();
            }

            writer.WriteLine("{0}\tthen", indentationStr);
            cdssProtocol.Actions.EmitCdssText(writer, indentation + 2);
            writer.WriteLine("{0}end protocol", indentationStr);
        }

        private static void EmitCdssText(this CdssExpressionDefinition cdssExpression, StringWriter writer, int indentation = 0)
        {
            var indentationStr = new String('\t', indentation);
            switch(cdssExpression)
            {
                case CdssHdsiExpressionDefinition hdsi:
                    writer.Write("hdsi($${0}$$", hdsi.ExpressionValue);
                    if(hdsi.Scope != CdssHdsiExpressionScopeType.Context)
                    {
                        writer.Write(" scoped-to proposal");
                    }
                    if(hdsi.IsNegated)
                    {
                        writer.Write(" negated");
                    }
                    writer.Write(")");
                    break;
                case CdssCsharpExpressionDefinition csharp:
                    writer.Write("csharp($${0}$$)", csharp.ExpressionValue);
                    break;
                case CdssAggregateExpressionDefinition agg:
                    writer.WriteLine("{0}(", agg is CdssAnyExpressionDefinition ? "any" : agg is CdssAllExpressionDefinition ? "all" : "none");
                    agg.ContainedExpressions?.ForEach(e => {
                        writer.Write("{0}\t", indentationStr);
                        e.EmitCdssText(writer, indentation + 1);
                        if (e != agg.ContainedExpressions.Last())
                        {
                            writer.WriteLine(",", indentationStr);
                        }
                        else
                        {
                            writer.WriteLine();
                        }
                    });
                    writer.WriteLine("{0})", indentationStr);
                    break;
                case CdssFactReferenceExpressionDefinition fact:
                    writer.Write("\"{0}\"", fact.FactName);
                    break;
                case CdssQueryExpressionDefinition query:
                    writer.WriteLine("query(");
                    if(!String.IsNullOrEmpty(query.SourceCollectionHdsi))
                    {
                        writer.WriteLine("{0}\tfrom hdsi($${1}$$)", indentationStr, query.SourceCollectionHdsi);
                    }
                    if(!string.IsNullOrEmpty(query.FilterHdsi))
                    {
                        writer.WriteLine("{0}\twhere hdsi($${1}$$)", indentationStr, query.FilterHdsi);
                    }
                    if(!String.IsNullOrEmpty(query.SelectHdsi))
                    {
                        writer.WriteLine("{0}\tselect {1} hdsi($${2}$$)", indentationStr, query.SelectorFunction.ToString().ToLowerInvariant(), query.SelectHdsi);
                    }
                    if(!string.IsNullOrEmpty(query.OrderByHdsi))
                    {
                        writer.WriteLine("{0}\torder by hdsi($${1}$$)", indentationStr, query.OrderByHdsi);
                    }
                    writer.WriteLine("{0})", indentationStr);
                    break;
            }
        }
        private static void EmitCdssText(this CdssDatasetDefinition cdssDataset, StringWriter writer, int indentation = 0)
        {

            var indentationStr = new String('\t', indentation);
            writer.WriteLine("{0}define data \"{1}\"", indentationStr, cdssDataset.Name);
            cdssDataset.EmitHavingStatements(writer, indentation);
            cdssDataset.Metadata?.EmitMetadata(writer, indentation + 1);

            writer.WriteLine("{0}as $$", indentationStr);
            writer.WriteLine(Encoding.UTF8.GetString(cdssDataset.RawData));
            writer.WriteLine("{0}$$\r\n{0}end data", indentationStr);

        }

        private static void EmitHavingStatements(this CdssBaseObjectDefinition cdssBaseObject, StringWriter writer, int indentation = 0)
        {
            var indentationStr = new String('\t', indentation);
            if (!String.IsNullOrEmpty(cdssBaseObject.Id))
            {
                writer.WriteLine("{0}\thaving id <{1}>", indentationStr, cdssBaseObject.Id);
            }
            if (cdssBaseObject.Uuid != Guid.Empty)
            {
                writer.WriteLine("{0}\thaving uuid {{{1}}}", indentationStr, cdssBaseObject.Uuid);
            }
            if (!String.IsNullOrEmpty(cdssBaseObject.Oid))
            {
                writer.WriteLine("{0}\thaving oid \"{1}\"", indentationStr, cdssBaseObject.Oid);
            }
           

            if (cdssBaseObject.StatusSpecified || cdssBaseObject.Status != CdssObjectState.Unknown)
            {
                var statusStr = cdssBaseObject.Status == CdssObjectState.Active ? "active" : cdssBaseObject.Status == CdssObjectState.Retired ? "retired" :
                    cdssBaseObject.Status == CdssObjectState.TrialUse ? "trial-use" : "dont-use";
                writer.WriteLine("{0}\thaving status {1}", indentationStr, statusStr);
            }

        }

        private static void EmitMetadata(this CdssObjectMetadata objectMetadata, StringWriter writer, int indentation = 0)
        {
            var indentationStr = new String('\t', indentation);
            writer.WriteLine("{0}with metadata", indentationStr);
            objectMetadata.Authors?.ForEach(o => writer.WriteLine("{0}\tauthor {1}", indentationStr, o));

            if(!string.IsNullOrEmpty(objectMetadata.Version))
            {
                writer.WriteLine("{0}\tversion {1}", indentationStr, objectMetadata.Version);
            }
            objectMetadata.Documentation?.Split('\n').ForEach(d => writer.WriteLine("{0}\tdoc {1}", indentationStr, d.Trim()));
            writer.WriteLine("{0}end metadata", indentationStr);
        }
    }
}
