/*
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
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using SanteDB.Cdss.Xml.Exceptions;
using SanteDB.Cdss.Xml.i18n;
using SanteDB.Cdss.Xml.Model;
using SanteDB.Cdss.Xml.Model.Actions;
using SanteDB.Cdss.Xml.Model.Assets;
using SanteDB.Cdss.Xml.Model.Expressions;
using SanteDB.Core.BusinessRules;
using SanteDB.Core.Http;
using SanteDB.Core.i18n;
using SanteDB.Core.Model.Acts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using ZstdSharp.Unsafe;

namespace SanteDB.Cdss.Xml.Antlr
{
    /// <summary>
    /// Represents a library visitor
    /// </summary>
    internal class CdssLibraryVisitor : CdssLibraryBaseVisitor<CdssLibraryDefinition>
    {
        private CdssLibraryDefinition m_cdssLibrary;
        private readonly Stack<CdssBaseObjectDefinition> m_currentObject = new Stack<CdssBaseObjectDefinition>();
        private readonly Stack<CdssExpressionDefinition> m_currentComputationStack = new Stack<CdssExpressionDefinition>();

        private static readonly Regex s_extractNamedIdentifier = new Regex(@"^\<([a-z][0-9a-z\._]+?)\>$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex s_extractStringContents = new Regex(@"^""((?:\.|(\""\"")|[^\""""\n])*)""$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex s_extractMultilineString = new Regex(@"\$\$((.|\r|\n)*)\$\$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private readonly bool m_includeMap;
        private readonly string m_sourcePath;

        public CdssLibraryVisitor(bool includeMapData, String sourcePath)
        {
            this.m_includeMap = includeMapData;
            this.m_sourcePath = sourcePath;
        }

        private TCdssBaseObject CreateCdssObject<TCdssBaseObject>(ParserRuleContext context) where TCdssBaseObject : CdssBaseObjectDefinition, new()
        {
            var retVal = new TCdssBaseObject();
            if (this.m_includeMap)
            {
                retVal.TranspileSourceReference = new CdssTranspileMapMetaData(context.Start.Line, context.Start.Column, context.Stop.Line, context.Stop.Column);
                retVal.TranspileSourceReference.SourceFileName = this.m_sourcePath;
            }
            return retVal;
        }

        private TCdssExpression CreateCdssExpression<TCdssExpression>(ParserRuleContext context) where TCdssExpression : CdssExpressionDefinition, new()
        {
            var retVal = new TCdssExpression();
            if (this.m_includeMap)
            {
                retVal.TranspileSourceReference = new CdssTranspileMapMetaData(context.Start.Line, context.Start.Column, context.Stop.Line, context.Stop.Column);
                retVal.TranspileSourceReference.SourceFileName = this.m_sourcePath;
            }
            return retVal;
        }

        public override CdssLibraryDefinition VisitLibrary([NotNull] CdssLibraryParser.LibraryContext context)
        {
            this.m_cdssLibrary = this.CreateCdssObject<CdssLibraryDefinition>(context);
            this.m_cdssLibrary.Include = new List<string>();

            base.VisitLibrary(context);
            return this.m_cdssLibrary;
        }

        public override CdssLibraryDefinition VisitInclude_definition([NotNull] CdssLibraryParser.Include_definitionContext context)
        {
            // Named ID include?
            ITerminalNode identifiedInclude = context.NAMED_ID(),
                namedInclude = context.STRING();

            if (this.TryExtractIdentifier(identifiedInclude, out var include))
            {
                this.m_cdssLibrary.Include.Add($"#{include}");
            }
            else if (this.TryExtractString(namedInclude, out include))
            {
                this.m_cdssLibrary.Include.Add(include);
            }
            else
            {
                throw new CdssTranspilationException(context.Start, CdssTranspileErrors.MISSING_INCLUDE_TARGET);
            }
            return base.VisitInclude_definition(context);
        }

        public override CdssLibraryDefinition VisitLibrary_definition([NotNull] CdssLibraryParser.Library_definitionContext context)
        {
            var libraryNameToken = context.STRING();
            if (this.TryExtractString(libraryNameToken, out var libraryName))
            {
                this.m_cdssLibrary.Name = libraryName;
            }
            else
            {
                throw new CdssTranspilationException(context.Start, CdssTranspileErrors.MISSING_NAME_TOKEN);

            }
            this.m_currentObject.Push(this.m_cdssLibrary);

            return base.VisitLibrary_definition(context);
        }

        public override CdssLibraryDefinition VisitHaving_id([NotNull] CdssLibraryParser.Having_idContext context)
        {
            if (this.m_currentObject == null)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.DEPENDENT_PROPERTY_NULL, nameof(m_currentObject)));
            }

            var idToken = context.NAMED_ID();
            if (this.TryExtractIdentifier(idToken, out var identifier))
            {
                this.m_currentObject.Peek().Id = identifier;
            }
            else
            {
                throw new CdssTranspilationException(context.Start, CdssTranspileErrors.MISSING_ID_TOKEN);

            }
            return base.VisitHaving_id(context);
        }

        public override CdssLibraryDefinition VisitHaving_oid([NotNull] CdssLibraryParser.Having_oidContext context)
        {
            if (this.m_currentObject == null)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.DEPENDENT_PROPERTY_NULL, nameof(m_currentObject)));
            }

            var oidToken = context.OID_DOTTED();

            if (this.TryExtractString(oidToken, out var oid))
            {
                this.m_currentObject.Peek().Oid = oid;
            }
            else
            {
                throw new CdssTranspilationException(context.Start, CdssTranspileErrors.MISSING_OID_TOKEN);

            }
            return base.VisitHaving_oid(context);
        }

        public override CdssLibraryDefinition VisitHaving_status([NotNull] CdssLibraryParser.Having_statusContext context)
        {
            if (this.m_currentObject == null)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.DEPENDENT_PROPERTY_NULL, nameof(m_currentObject)));
            }

            var statusToken = context.STATUS_VAL();

            m_currentObject.Peek().StatusSpecified = true;
            switch (statusToken?.GetText()?.ToLowerInvariant())
            {
                case "active":
                    m_currentObject.Peek().Status = CdssObjectState.Active;
                    break;
                case "trial-use":
                    m_currentObject.Peek().Status = CdssObjectState.TrialUse;
                    break;
                case "dont-use":
                    m_currentObject.Peek().Status = CdssObjectState.DontUse;
                    break;
                default:
                    throw new CdssTranspilationException(context.Start, CdssTranspileErrors.MISSING_STATUS_TOKEN);
            }
            return base.VisitHaving_status(context);
        }

        public override CdssLibraryDefinition VisitHaving_uuid([NotNull] CdssLibraryParser.Having_uuidContext context)
        {
            if (this.m_currentObject == null)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.DEPENDENT_PROPERTY_NULL, nameof(m_currentObject)));
            }

            if (this.TryExtractString(context.STRING(), out var uuidString) && Guid.TryParse(uuidString, out var uuid) || this.TryExtractUuid(context.UUIDV4(), out uuid))
            {
                this.m_currentObject.Peek().Uuid = uuid;
                this.m_currentObject.Peek().UuidSpecified = true;
            }
            else
            {
                throw new CdssTranspilationException(context.Start, CdssTranspileErrors.MISSING_UUID_TOKEN);

            }
            return base.VisitHaving_uuid(context);
        }

        public override CdssLibraryDefinition VisitLogic_block([NotNull] CdssLibraryParser.Logic_blockContext context)
        {

            var logicBlock = this.CreateCdssObject<CdssDecisionLogicBlockDefinition>(context);
            this.m_cdssLibrary.Definitions = this.m_cdssLibrary.Definitions ?? new List<CdssBaseObjectDefinition>();

            this.m_cdssLibrary.Definitions.Add(logicBlock);
            var logicNameToken = context.GetToken(CdssLibraryLexer.STRING, 0);
            if (this.TryExtractString(logicNameToken, out var logicBlockName))
            {
                logicBlock.Name = logicBlockName;
            }
            else
            {
                throw new CdssTranspilationException(context.Start, CdssTranspileErrors.MISSING_NAME_TOKEN);
            }
            this.m_currentObject.Push(logicBlock);
            var retVal = base.VisitLogic_block(context);
            this.m_currentObject.Pop();
            return retVal;
        }

        public override CdssLibraryDefinition VisitMetadata_statement([NotNull] CdssLibraryParser.Metadata_statementContext context)
        {
            if (this.m_currentObject == null)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.DEPENDENT_PROPERTY_NULL, nameof(m_currentObject)));
            }
            this.m_currentObject.Peek().Metadata = new CdssObjectMetadata();
            return base.VisitMetadata_statement(context);
        }

        public override CdssLibraryDefinition VisitMetadata_documentation_statement([NotNull] CdssLibraryParser.Metadata_documentation_statementContext context)
        {
            if (this.m_currentObject == null)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.DEPENDENT_PROPERTY_NULL, nameof(m_currentObject)));
            }

            var documentationText = context.GetToken(CdssLibraryLexer.DOCUMENTATION, 0);

            if (String.IsNullOrEmpty(this.m_currentObject.Peek().Metadata.Documentation))
            {
                this.m_currentObject.Peek().Metadata.Documentation = documentationText.GetText().Substring(4).Trim();
            }
            else
            {
                this.m_currentObject.Peek().Metadata.Documentation += $"\r\n{documentationText.GetText().Substring(4).Trim()}";
            }
            return base.VisitMetadata_documentation_statement(context);
        }

        public override CdssLibraryDefinition VisitMetadata_version_statement([NotNull] CdssLibraryParser.Metadata_version_statementContext context)
        {
            if (this.m_currentObject == null)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.DEPENDENT_PROPERTY_NULL, nameof(m_currentObject)));
            }

            ITerminalNode versionText = context.GetToken(CdssLibraryLexer.VERSION_VAL, 0) ??
                context.GetToken(CdssLibraryLexer.FLOAT, 0);

            this.m_currentObject.Peek().Metadata.Version = versionText.GetText().Trim();
            return base.VisitMetadata_version_statement(context);
        }

        public override CdssLibraryDefinition VisitMetadata_author_statement([NotNull] CdssLibraryParser.Metadata_author_statementContext context)
        {
            if (this.m_currentObject == null)
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.DEPENDENT_PROPERTY_NULL, nameof(m_currentObject)));
            }

            var authorText = context.GetToken(CdssLibraryLexer.AUTHOR, 0);
            this.m_currentObject.Peek().Metadata.Authors = this.m_currentObject.Peek().Metadata.Authors ?? new List<string>();
            this.m_currentObject.Peek().Metadata.Authors.Add(authorText.GetText().Substring(6).Trim());
            return base.VisitMetadata_author_statement(context);
        }

        public override CdssLibraryDefinition VisitData_block([NotNull] CdssLibraryParser.Data_blockContext context)
        {
            var dataBlock = this.CreateCdssObject<CdssDatasetDefinition>(context);
            this.m_cdssLibrary.Definitions = this.m_cdssLibrary.Definitions ?? new List<CdssBaseObjectDefinition>();
            this.m_cdssLibrary.Definitions.Add(dataBlock);
            var nameToken = context.GetToken(CdssLibraryLexer.STRING, 0);
            if (this.TryExtractString(nameToken, out var name))
            {
                dataBlock.Name = name;
            }
            else
            {
                throw new CdssTranspilationException(context.Start, CdssTranspileErrors.MISSING_NAME_TOKEN);
            }

            if (this.TryExtractMultilineString(context.GetToken(CdssLibraryLexer.MULTILINE_STRING, 0), out var data))
            {
                dataBlock.CompressionScheme = Core.Http.Description.HttpCompressionAlgorithm.Gzip;
                dataBlock.RawData = Encoding.UTF8.GetBytes(data);
            }
            else
            {
                throw new CdssTranspilationException(context.Start, CdssTranspileErrors.MISSING_DATA_TOKEN);

            }

            return base.VisitData_block(context);
        }

        public override CdssLibraryDefinition VisitHaving_context([NotNull] CdssLibraryParser.Having_contextContext context)
        {
            if (!(this.m_currentObject.Peek() is CdssDecisionLogicBlockDefinition logicBlock))
            {
                throw new InvalidOperationException(String.Format(ErrorMessages.DEPENDENT_PROPERTY_NULL, nameof(m_currentObject)));
            }

            var classTypeToken = context.GetToken(CdssLibraryLexer.CLASS_TYPE, 0);
            logicBlock.Context = new CdssResourceTypeReference() { TypeXml = classTypeToken.GetText() };
            if (logicBlock.Context.Type == null)
            {
                throw new CdssTranspilationException(context.Start, String.Format(CdssTranspileErrors.INVALID_CLASS_REF, logicBlock.Context.TypeXml));
            }

            return base.VisitHaving_context(context);
        }

        public override CdssLibraryDefinition VisitFact_reference_or_computation([NotNull] CdssLibraryParser.Fact_reference_or_computationContext context)
        {
            // Is this just a reference - if so just reference the type
            if (this.TryExtractString(context.GetToken(CdssLibraryLexer.STRING, 0), out var reference))
            {
                var refr = this.CreateCdssExpression<CdssFactReferenceExpressionDefinition>(context);
                refr.FactName = reference;
                this.AddComputation(refr);
            }
            else if (this.TryExtractIdentifier(context.GetToken(CdssLibraryLexer.NAMED_ID, 0), out reference))
            {
                var refr = this.CreateCdssExpression<CdssFactReferenceExpressionDefinition>(context);
                refr.FactName = $"#{reference}";
                this.AddComputation(refr);
            }
            else if (context.children.OfType<CdssLibraryParser.Fact_computationContext>().SingleOrDefault() == null)
            {
                throw new CdssTranspilationException(context.Start, CdssTranspileErrors.COMPUTATION_MISSING);
            }

            // Append to the 
            return base.VisitFact_reference_or_computation(context);
        }

        public override CdssLibraryDefinition VisitAll_logic([NotNull] CdssLibraryParser.All_logicContext context)
        {
            if (!context.children.All(o => o is CdssLibraryParser.Fact_reference_or_computationContext || o is CdssLibraryParser.Fact_computationContext || o is TerminalNodeImpl))
            {
                throw new CdssTranspilationException(context.Start, CdssTranspileErrors.COMPUTATION_MISSING);
            }

            var expression = this.CreateCdssExpression<CdssAllExpressionDefinition>(context);
            this.AddComputation(expression);
            this.m_currentComputationStack.Push(expression);
            var retVal = base.VisitAll_logic(context);
            this.m_currentComputationStack.Pop();
            return retVal;
        }

        public override CdssLibraryDefinition VisitAny_logic([NotNull] CdssLibraryParser.Any_logicContext context)
        {
            if (!context.children.All(o => o is CdssLibraryParser.Fact_reference_or_computationContext || o is CdssLibraryParser.Fact_computationContext || o is TerminalNodeImpl))
            {
                throw new CdssTranspilationException(context.Start, CdssTranspileErrors.COMPUTATION_MISSING);
            }

            var expression = this.CreateCdssExpression<CdssAnyExpressionDefinition>(context);
            this.AddComputation(expression);
            this.m_currentComputationStack.Push(expression);
            var retVal = base.VisitAny_logic(context);
            this.m_currentComputationStack.Pop();
            return retVal;
        }

        public override CdssLibraryDefinition VisitNone_logic([NotNull] CdssLibraryParser.None_logicContext context)
        {
            if (!context.children.All(o => o is CdssLibraryParser.Fact_reference_or_computationContext || o is CdssLibraryParser.Fact_computationContext || o is TerminalNodeImpl))
            {
                throw new CdssTranspilationException(context.Start, CdssTranspileErrors.COMPUTATION_MISSING);
            }

            var expression = this.CreateCdssExpression<CdssNoneExpressionDefinition>(context);
            this.AddComputation(expression);
            this.m_currentComputationStack.Push(expression);
            var retVal = base.VisitNone_logic(context);
            this.m_currentComputationStack.Pop();
            return retVal;
        }

        public override CdssLibraryDefinition VisitHdsi_logic([NotNull] CdssLibraryParser.Hdsi_logicContext context)
        {
            if (context.Parent is CdssLibraryParser.Query_logicContext)
            {
                ;
            }
            else if (this.TryExtractString(context.STRING(), out var hdsi) || this.TryExtractMultilineString(context.MULTILINE_STRING(), out hdsi))
            {
                var hdsiExpr = this.CreateCdssExpression<CdssHdsiExpressionDefinition>(context);
                hdsiExpr.ExpressionValue = hdsi;
                hdsiExpr.Scope = context.PROPOSAL() != null ? CdssHdsiExpressionScopeType.CurrentObject : CdssHdsiExpressionScopeType.Context;
                hdsiExpr.IsNegated = context.NEGATED() != null;
                this.AddComputation(hdsiExpr);
            }
            else
            {
                throw new CdssTranspilationException(context.Start, CdssTranspileErrors.MISSING_HDSI_EXPRESSION);
            }
            return base.VisitHdsi_logic(context);
        }

        public override CdssLibraryDefinition VisitCsharp_logic([NotNull] CdssLibraryParser.Csharp_logicContext context)
        {
            if (this.TryExtractString(context.STRING(), out var csharp) || this.TryExtractMultilineString(context.MULTILINE_STRING(), out csharp))
            {
                var cshrp = this.CreateCdssExpression<CdssCsharpExpressionDefinition>(context);
                cshrp.ExpressionValue = csharp;
                this.AddComputation(cshrp);
            }
            else
            {
                throw new CdssTranspilationException(context.Start, CdssTranspileErrors.MISSING_CSHARP_EXPRESSION);
            }
            return base.VisitCsharp_logic(context);
        }


        public override CdssLibraryDefinition VisitQuery_logic([NotNull] CdssLibraryParser.Query_logicContext context)
        {

            // Get the HDSI expressions
            var hdsiChildren = context.children.OfType<CdssLibraryParser.Hdsi_logicContext>().Select(o =>
            {
                if (this.TryExtractString(o.STRING(), out var hdsi) || this.TryExtractMultilineString(o.MULTILINE_STRING(), out hdsi))
                {
                    return hdsi;
                }
                else
                {
                    throw new CdssTranspilationException(context.Start, CdssTranspileErrors.MISSING_HDSI_EXPRESSION);
                }
            }).ToArray();
            if (hdsiChildren.Length <= 3)
            {
                throw new CdssTranspilationException(context.Start, CdssTranspileErrors.MISSING_QUERY_PARAMETERS);
            }

            CdssCollectionSelectorType collectionType = CdssCollectionSelectorType.First;
            switch (context.AGG_SELECTOR()?.GetText().ToLowerInvariant())
            {
                case "last":
                    collectionType = CdssCollectionSelectorType.Last;
                    break;
                case "single":
                    collectionType = CdssCollectionSelectorType.Single;
                    break;
            }

            this.AddComputation(new CdssQueryExpressionDefinition()
            {
                SourceCollectionHdsi = hdsiChildren[0],
                FilterHdsi = hdsiChildren[1],
                SelectorFunction = collectionType,
                SelectHdsi = hdsiChildren[2],
                OrderByHdsi = hdsiChildren.Length > 3 ? hdsiChildren[3] : null
            });
            return base.VisitQuery_logic(context);
        }

        public override CdssLibraryDefinition VisitFact_normalization([NotNull] CdssLibraryParser.Fact_normalizationContext context)
        {
            if (!(this.m_currentObject.Peek() is CdssFactAssetDefinition factAssetDefinition))
            {
                throw new CdssTranspilationException(context.Start, String.Format(CdssTranspileErrors.EXPRESSION_CANNOT_BE_APPLIED_IN_CONTEXT, this.m_currentObject.Peek()));
            }

            var normalizationInstruction = this.CreateCdssObject<CdssFactNormalizationDefinition>(context);
            factAssetDefinition.Normalize = factAssetDefinition.Normalize ?? new List<CdssFactNormalizationDefinition>();
            factAssetDefinition.Normalize.Add(normalizationInstruction);
            this.m_currentObject.Push(normalizationInstruction);
            var retVal = base.VisitFact_normalization(context);
            this.m_currentObject.Pop();
            return retVal;
        }

        public override CdssLibraryDefinition VisitDefine_fact([NotNull] CdssLibraryParser.Define_factContext context)
        {
            if (!(m_currentObject.Peek() is CdssDecisionLogicBlockDefinition logicBlock))
            {
                throw new CdssTranspilationException(context.Start, String.Format(CdssTranspileErrors.EXPRESSION_CANNOT_BE_APPLIED_IN_CONTEXT, this.m_currentObject.Peek()));
            }
            var factDefinition = this.CreateCdssObject<CdssFactAssetDefinition>(context);
            if (this.TryExtractString(context.STRING(), out var name))
            {
                factDefinition.Name = name;
            }
            else
            {
                throw new CdssTranspilationException(context.Start, CdssTranspileErrors.MISSING_NAME_TOKEN);
            }

            // Process
            logicBlock.Definitions = logicBlock.Definitions ?? new List<CdssComputableAssetDefinition>();
            logicBlock.Definitions.Add(factDefinition);
            this.m_currentObject.Push(factDefinition);
            var retVal = base.VisitDefine_fact(context);
            this.m_currentObject.Pop();
            return retVal;
        }

        public override CdssLibraryDefinition VisitHaving_negation([NotNull] CdssLibraryParser.Having_negationContext context)
        {
            if (m_currentObject.Peek() is CdssFactAssetDefinition cdssFactAssetDefinition && Boolean.TryParse(context.BOOL_VAL()?.GetText() ?? "true", out var negated))
            {
                cdssFactAssetDefinition.IsNegated = negated;
            }
            else
            {
                throw new CdssTranspilationException(context.Start, String.Format(CdssTranspileErrors.EXPRESSION_CANNOT_BE_APPLIED_IN_CONTEXT, this.m_currentObject.Peek()));
            }
            return base.VisitHaving_negation(context);
        }

        public override CdssLibraryDefinition VisitHaving_type([NotNull] CdssLibraryParser.Having_typeContext context)
        {
            if (m_currentObject.Peek() is CdssFactAssetDefinition cdssFactAssetDefinition)
            {
                switch (context.PRIMITIVE_TYPE().GetText().ToLowerInvariant())
                {
                    case "bool":
                        cdssFactAssetDefinition.ValueType = CdssValueType.Boolean;
                        break;
                    case "string":
                        cdssFactAssetDefinition.ValueType = CdssValueType.String;
                        break;
                    case "date":
                        cdssFactAssetDefinition.ValueType = CdssValueType.Date;
                        break;
                    case "long":
                        cdssFactAssetDefinition.ValueType = CdssValueType.Long;
                        break;
                    case "real":
                        cdssFactAssetDefinition.ValueType = CdssValueType.Real;
                        break;
                    case "int":
                        cdssFactAssetDefinition.ValueType = CdssValueType.Integer;
                        break;
                    default:
                        throw new CdssTranspilationException(context.Start, CdssTranspileErrors.FACT_UNKNOWN_PRIMITIVE_TYPE);
                }
                cdssFactAssetDefinition.ValueTypeSpecified = true;
            }
            else
            {
                throw new CdssTranspilationException(context.Start, String.Format(CdssTranspileErrors.EXPRESSION_CANNOT_BE_APPLIED_IN_CONTEXT, this.m_currentObject.Peek()));
            }
            return base.VisitHaving_type(context);
        }


        public override CdssLibraryDefinition VisitDefine_model([NotNull] CdssLibraryParser.Define_modelContext context)
        {
            if (!(m_currentObject.Peek() is CdssDecisionLogicBlockDefinition logicBlock))
            {
                throw new CdssTranspilationException(context.Start, String.Format(CdssTranspileErrors.EXPRESSION_CANNOT_BE_APPLIED_IN_CONTEXT, this.m_currentObject.Peek()));
            }
            var modelDefinition = this.CreateCdssObject<CdssModelAssetDefinition>(context);
            if (this.TryExtractString(context.STRING(), out var name))
            {
                modelDefinition.Name = name;
            }
            else
            {
                throw new CdssTranspilationException(context.Start, CdssTranspileErrors.MISSING_NAME_TOKEN);
            }

            // Get the format of the data 
            if (this.TryExtractMultilineString(context.MULTILINE_STRING(), out var definition))
            {
                modelDefinition.Model = this.ParseModelDefinition(definition, context.FORMAT_REF()?.GetText().ToLowerInvariant());
            }
            else
            {
                throw new CdssTranspilationException(context.Start, CdssTranspileErrors.MODEL_MISSING_DEFINITION_DATA);
            }

            logicBlock.Definitions = logicBlock.Definitions ?? new List<CdssComputableAssetDefinition>();
            logicBlock.Definitions.Add(modelDefinition);
            this.m_currentObject.Push(modelDefinition);
            var retVal = base.VisitDefine_model(context);
            this.m_currentObject.Pop();
            return retVal;
        }

        public override CdssLibraryDefinition VisitDefine_rule([NotNull] CdssLibraryParser.Define_ruleContext context)
        {
            if (!(m_currentObject.Peek() is CdssDecisionLogicBlockDefinition logicBlock))
            {
                throw new CdssTranspilationException(context.Start, String.Format(CdssTranspileErrors.EXPRESSION_CANNOT_BE_APPLIED_IN_CONTEXT, this.m_currentObject.Peek()));
            }
            var ruleDefinition = this.CreateCdssObject<CdssRuleAssetDefinition>(context);
            if (this.TryExtractString(context.STRING(), out var name))
            {
                ruleDefinition.Name = name;
            }
            else
            {
                throw new CdssTranspilationException(context.Start, CdssTranspileErrors.MISSING_NAME_TOKEN);
            }

            logicBlock.Definitions = logicBlock.Definitions ?? new List<CdssComputableAssetDefinition>();
            logicBlock.Definitions.Add(ruleDefinition);
            this.m_currentObject.Push(ruleDefinition);
            var retVal = base.VisitDefine_rule(context);
            this.m_currentObject.Pop();
            return retVal;
        }

        public override CdssLibraryDefinition VisitDefine_protocol([NotNull] CdssLibraryParser.Define_protocolContext context)
        {
            if (!(m_currentObject.Peek() is CdssDecisionLogicBlockDefinition logicBlock))
            {
                throw new CdssTranspilationException(context.Start, String.Format(CdssTranspileErrors.EXPRESSION_CANNOT_BE_APPLIED_IN_CONTEXT, this.m_currentObject.Peek()));
            }
            var protocolDefinition = this.CreateCdssObject<CdssProtocolAssetDefinition>(context);
            if (this.TryExtractString(context.STRING(), out var name))
            {
                protocolDefinition.Name = name;
            }
            else
            {
                throw new CdssTranspilationException(context.Start, CdssTranspileErrors.MISSING_NAME_TOKEN);
            }

            logicBlock.Definitions = logicBlock.Definitions ?? new List<CdssComputableAssetDefinition>();
            logicBlock.Definitions.Add(protocolDefinition);
            this.m_currentObject.Push(protocolDefinition);
            var retVal = base.VisitDefine_protocol(context);
            this.m_currentObject.Pop();
            return retVal;
        }

        public override CdssLibraryDefinition VisitApply_action_statement([NotNull] CdssLibraryParser.Apply_action_statementContext context)
        {
            if (!(m_currentObject.Peek() is CdssActionCollectionDefinition collectionDefinition))
            {
                throw new CdssTranspilationException(context.Start, String.Format(CdssTranspileErrors.EXPRESSION_CANNOT_BE_APPLIED_IN_CONTEXT, this.m_currentObject.Peek()));
            }

            if (this.TryExtractString(context.STRING(), out var reference))
            {
                var ruleReferenceAction = this.CreateCdssObject<CdssRuleReferenceActionDefinition>(context);
                ruleReferenceAction.RuleName = reference;
                this.m_currentObject.Push(ruleReferenceAction);
                var retVal = base.VisitApply_action_statement(context);
                collectionDefinition.Actions.Add(ruleReferenceAction);
                this.m_currentObject.Pop();
                return retVal;
            }
            else
            {
                throw new CdssTranspilationException(context.Start, CdssTranspileErrors.MISSING_NAME_TOKEN);

            }

        }

        public override CdssLibraryDefinition VisitRepeat_action_statement([NotNull] CdssLibraryParser.Repeat_action_statementContext context)
        {
            if (!(m_currentObject.Peek() is CdssActionCollectionDefinition collectionDefinition))
            {
                throw new CdssTranspilationException(context.Start, String.Format(CdssTranspileErrors.EXPRESSION_CANNOT_BE_APPLIED_IN_CONTEXT, this.m_currentObject.Peek()));
            }

            var repeatDefinition = this.CreateCdssObject<CdssRepeatActionDefinition>(context);
            collectionDefinition.Actions.Add(repeatDefinition);
            repeatDefinition.Actions = new CdssActionCollectionDefinition();
            if (context.FOR() != null && Int32.TryParse(context.INTEGER().GetText(), out var reps))
            {
                repeatDefinition.Iterations = reps;
                repeatDefinition.IterationsSpecified = true;
            }
            if (context.TRACKBY() != null)
            {
                repeatDefinition.IterationVariable = context.LITERAL()?.GetText() ?? context.HDSI_EXPR()?.GetText();
            }

            this.m_currentObject.Push(repeatDefinition);
            var retVal = base.VisitRepeat_action_statement(context);
            this.m_currentObject.Pop();
            return retVal;
        }

        public override CdssLibraryDefinition VisitThen_action_statements([NotNull] CdssLibraryParser.Then_action_statementsContext context)
        {
            switch (this.m_currentObject.Peek())
            {
                case IHasCdssActions cdssActionContainer:
                    cdssActionContainer.Actions = cdssActionContainer.Actions ?? this.CreateCdssObject<CdssActionCollectionDefinition>(context);
                    this.m_currentObject.Push(cdssActionContainer.Actions);
                    var retVal = base.VisitThen_action_statements(context);
                    this.m_currentObject.Pop();
                    return retVal;
                case CdssActionCollectionDefinition collection:
                    return base.VisitThen_action_statements(context);
                default:
                    throw new CdssTranspilationException(context.Start, String.Format(CdssTranspileErrors.EXPRESSION_CANNOT_BE_APPLIED_IN_CONTEXT, this.m_currentObject.Peek()));
            }
        }

        public override CdssLibraryDefinition VisitPropose_action_statement([NotNull] CdssLibraryParser.Propose_action_statementContext context)
        {
            if (!(m_currentObject.Peek() is CdssActionCollectionDefinition actionCollection))
            {
                throw new CdssTranspilationException(context.Start, String.Format(CdssTranspileErrors.EXPRESSION_CANNOT_BE_APPLIED_IN_CONTEXT, this.m_currentObject.Peek()));
            }

            var proposeAction = this.CreateCdssObject<CdssProposeActionDefinition>(context);
            proposeAction.Assignment = new List<CdssPropertyAssignActionDefinition>();
            actionCollection.Actions.Add(proposeAction);
            this.m_currentObject.Push(proposeAction);
            var retVal = base.VisitPropose_action_statement(context);
            this.m_currentObject.Pop();
            return retVal;
        }

        public override CdssLibraryDefinition VisitAssign_action_statement([NotNull] CdssLibraryParser.Assign_action_statementContext context)
        {

            var assignAction = this.CreateCdssObject<CdssPropertyAssignActionDefinition>(context);
            assignAction.Path = context.HDSI_EXPR()?.GetText();
            assignAction.OverwriteValue = true;
            if (String.IsNullOrEmpty(assignAction.Path))
            {
                throw new CdssTranspilationException(context.Start, CdssTranspileErrors.ASSIGN_MISSING_PATH);
            }

            // Is it a reference?
            if (context.CONST() != null)
            {
                if (!this.TryExtractString(context.STRING(), out var fixedString))
                {
                    assignAction.ContainedExpression = context.INTEGER()?.GetText() ??
                        context.BOOL_VAL()?.GetText() ??
                        context.FLOAT()?.GetText() ??
                        context.UUIDV4()?.GetText();
                }
                else
                {
                    assignAction.ContainedExpression = fixedString;
                }
            }
            else if (this.TryExtractString(context.STRING(), out var reference))
            {
                var fr = this.CreateCdssExpression<CdssFactReferenceExpressionDefinition>(context);
                fr.FactName = reference;
                assignAction.ContainedExpression = fr;
            }


            this.m_currentObject.Push(assignAction);
            var retVal = base.VisitAssign_action_statement(context);
            this.m_currentObject.Pop();

            switch (m_currentObject.Peek())
            {
                case CdssProposeActionDefinition proposeAction:
                    proposeAction.Assignment.Add(assignAction);
                    break;
                case CdssActionCollectionDefinition actionCollection:
                    actionCollection.Actions.Add(assignAction);
                    break;
                default:
                    throw new CdssTranspilationException(context.Start, String.Format(CdssTranspileErrors.EXPRESSION_CANNOT_BE_APPLIED_IN_CONTEXT, this.m_currentObject.Peek()));
            }
            return retVal;
        }

        public override CdssLibraryDefinition VisitHaving_model([NotNull] CdssLibraryParser.Having_modelContext context)
        {
            if (!(m_currentObject.Peek() is CdssProposeActionDefinition proposeDefinition))
            {
                throw new CdssTranspilationException(context.Start, String.Format(CdssTranspileErrors.EXPRESSION_CANNOT_BE_APPLIED_IN_CONTEXT, this.m_currentObject.Peek()));
            }

            if (this.TryExtractString(context.STRING(), out var reference))
            {
                proposeDefinition.Model = new CdssModelAssetDefinition() { ReferencedModel = reference };
            }
            else if (this.TryExtractMultilineString(context.MULTILINE_STRING(), out var inlineModel))
            {
                proposeDefinition.Model = this.CreateCdssObject<CdssModelAssetDefinition>(context);
                proposeDefinition.Model.Model = this.ParseModelDefinition(inlineModel, context.FORMAT_REF()?.GetText().ToLowerInvariant());
            }
            return base.VisitHaving_model(context);
        }

        private object ParseModelDefinition(string modelContent, string formatIdentifier)
        {
            switch (formatIdentifier?.ToLowerInvariant() ?? "json")
            {
                case "json":
                    return modelContent;
                case "xml":
                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(modelContent)))
                    {
                        return new XmlBodySerializer().DeSerialize(ms, new ContentType("application/xml"), typeof(Act));
                    }
                default:
                    throw new InvalidOperationException(String.Format(CdssTranspileErrors.UNKNOWN_MODEL_FORMAT, formatIdentifier));
            }
        }

        public override CdssLibraryDefinition VisitWhen_guard_condition([NotNull] CdssLibraryParser.When_guard_conditionContext context)
        {

            var whenCondition = this.CreateCdssObject<CdssWhenDefinition>(context);
            switch (this.m_currentObject.Peek())
            {
                case CdssDecisionLogicBlockDefinition logicBlock:
                    logicBlock.When = whenCondition;
                    break;
                case CdssRuleAssetDefinition ruleDefinition:
                    ruleDefinition.When = whenCondition;
                    break;
                case CdssInlineRuleActionDefinition inlineRuleActionDefinition:
                    inlineRuleActionDefinition.When = whenCondition;
                    break;
                case CdssRepeatActionDefinition repeatDefinition:
                    repeatDefinition.Until = whenCondition;
                    break;
                case CdssFactNormalizationDefinition normalizeDefinition:
                    normalizeDefinition.When = whenCondition;
                    break;
                default:
                    throw new InvalidOperationException(String.Format(CdssTranspileErrors.EXPRESSION_CANNOT_BE_APPLIED_IN_CONTEXT, this.m_currentObject.Peek()));
            }

            this.m_currentObject.Push(whenCondition);
            var retVal = base.VisitWhen_guard_condition(context);
            this.m_currentObject.Pop();
            return retVal;
        }

        public override CdssLibraryDefinition VisitHaving_scope([NotNull] CdssLibraryParser.Having_scopeContext context)
        {
            if (!(m_currentObject.Peek() is CdssProtocolAssetDefinition protocolAssetDefinition))
            {
                throw new CdssTranspilationException(context.Start, String.Format(CdssTranspileErrors.EXPRESSION_CANNOT_BE_APPLIED_IN_CONTEXT, this.m_currentObject.Peek()));
            }

            protocolAssetDefinition.Scopes = protocolAssetDefinition.Scopes ?? new List<CdssProtocolGroupDefinition>();
            if (this.TryExtractString(context.STRING(), out var scopeString))
            {
                protocolAssetDefinition.Scopes.Add(new CdssProtocolGroupDefinition() { Name = scopeString });
            }
            else if (this.TryExtractIdentifier(context.NAMED_ID(), out scopeString))
            {
                protocolAssetDefinition.Scopes.Add(new CdssProtocolGroupDefinition() { Id = scopeString });
            }
            else
            {
                throw new CdssTranspilationException(context.Start, CdssTranspileErrors.MISSING_SCOPE_ID);
            }

            return base.VisitHaving_scope(context);
        }

        public override CdssLibraryDefinition VisitRaise_action_statement([NotNull] CdssLibraryParser.Raise_action_statementContext context)
        {
            if (!(m_currentObject.Peek() is CdssActionCollectionDefinition actionCollection))
            {
                throw new CdssTranspilationException(context.Start, String.Format(CdssTranspileErrors.EXPRESSION_CANNOT_BE_APPLIED_IN_CONTEXT, this.m_currentObject.Peek()));
            }

            var raiseAlert = this.CreateCdssObject<CdssIssueActionDefinition>(context);
            raiseAlert.IssueToRaise = new Core.BusinessRules.DetectedIssue();
            actionCollection.Actions.Add(raiseAlert);
            if (this.TryExtractString(context.STRING(), out var issueText) || this.TryExtractMultilineString(context.MULTILINE_STRING(), out issueText))
            {
                raiseAlert.IssueToRaise.Text = issueText;
            }
            else
            {
                throw new CdssTranspilationException(context.Start, CdssTranspileErrors.MISSING_ISSUE_TEXT);
            }

            switch (context.ISSUE_PRIORITY_VAL()?.GetText().ToLowerInvariant() ?? "info")
            {
                case "info":
                    raiseAlert.IssueToRaise.Priority = Core.BusinessRules.DetectedIssuePriorityType.Information;
                    break;
                case "error":
                case "danger":
                case "alert":
                    raiseAlert.IssueToRaise.Priority = Core.BusinessRules.DetectedIssuePriorityType.Error;
                    break;
                case "warn":
                    raiseAlert.IssueToRaise.Priority = Core.BusinessRules.DetectedIssuePriorityType.Warning;
                    break;
                default:
                    throw new CdssTranspilationException(context.Start, CdssTranspileErrors.UNKNOWN_ISSUE_PRIORITY);
            }

            if (this.TryExtractString(context.UUIDV4(), out var uuidString) && Guid.TryParse(uuidString, out var uuid))
            {
                raiseAlert.IssueToRaise.TypeKey = uuid;
            }
            else if(context.ISSUE_TYPE() != null)
            {
                switch(context.ISSUE_TYPE().GetText().ToLowerInvariant() ?? "other")
                {
                    case "safety-concern":
                        raiseAlert.IssueToRaise.TypeKey = DetectedIssueKeys.SafetyConcernIssue;
                        break;
                    case "security":
                        raiseAlert.IssueToRaise.TypeKey = DetectedIssueKeys.SecurityIssue;
                        break;
                    case "constraint":
                        raiseAlert.IssueToRaise.TypeKey = DetectedIssueKeys.FormalConstraintIssue;
                        break;
                    case "codification":
                        raiseAlert.IssueToRaise.TypeKey = DetectedIssueKeys.CodificationIssue;
                        break;
                    case "other":
                        raiseAlert.IssueToRaise.TypeKey = DetectedIssueKeys.OtherIssue;
                        break;
                    case "already-done":
                        raiseAlert.IssueToRaise.TypeKey = DetectedIssueKeys.AlreadyDoneIssue;
                        break;
                    case "invalid-data":
                        raiseAlert.IssueToRaise.TypeKey = DetectedIssueKeys.AlreadyDoneIssue;
                        break;
                    case "privacy":
                        raiseAlert.IssueToRaise.TypeKey = DetectedIssueKeys.PrivacyIssue;
                        break;
            }
            }

            this.m_currentObject.Push(raiseAlert);
            var retVal = base.VisitRaise_action_statement(context);
            this.m_currentObject.Pop();
            return retVal;
        }

        public override CdssLibraryDefinition VisitInline_rule_definition([NotNull] CdssLibraryParser.Inline_rule_definitionContext context)
        {
            if (!(m_currentObject.Peek() is CdssProtocolAssetDefinition protocolDefinition))
            {
                throw new CdssTranspilationException(context.Start, String.Format(CdssTranspileErrors.EXPRESSION_CANNOT_BE_APPLIED_IN_CONTEXT, this.m_currentObject.Peek()));
            }
            var inlineRule = this.CreateCdssObject<CdssInlineRuleActionDefinition>(context);
            protocolDefinition.Actions = protocolDefinition.Actions ?? new CdssActionCollectionDefinition();
            protocolDefinition.Actions.Actions.Add(inlineRule);

            this.m_currentObject.Push(inlineRule);
            var retVal = base.VisitInline_rule_definition(context);
            this.m_currentObject.Pop();
            return retVal;
        }

        public override CdssLibraryDefinition VisitHaving_priority([NotNull] CdssLibraryParser.Having_priorityContext context)
        {
            if (!(m_currentObject.Peek() is CdssComputableAssetDefinition assetCollection))
            {
                throw new CdssTranspilationException(context.Start, String.Format(CdssTranspileErrors.EXPRESSION_CANNOT_BE_APPLIED_IN_CONTEXT, this.m_currentObject.Peek()));
            }

            if (Int32.TryParse(context.INTEGER()?.GetText(), out var pri))
            {
                assetCollection.Priority = pri;
            }
            else
            {
                assetCollection.Priority = Int32.MaxValue;
            }

            return base.VisitHaving_priority(context);
        }
        private void AddComputation(CdssExpressionDefinition expressionDefinition)
        {
            // Are we in an aggregate method?
            if (this.m_currentComputationStack.IsNullOrEmpty())
            {
                switch (this.m_currentObject.Peek())
                {
                    case CdssWhenDefinition whenCondition:
                        whenCondition.WhenComputation = expressionDefinition;
                        break;
                    case CdssFactNormalizationDefinition normalizeDefinition:
                        normalizeDefinition.EmitExpression = expressionDefinition;
                        break;
                    case CdssFactAssetDefinition factAssetDefinition:
                        factAssetDefinition.FactComputation = expressionDefinition;
                        break;
                    case CdssPropertyAssignActionDefinition propertyAssignActionDefinition:
                        propertyAssignActionDefinition.ContainedExpression = expressionDefinition;
                        break;
                    case CdssRepeatActionDefinition repeatActionDefinition:
                        repeatActionDefinition.Until = new CdssWhenDefinition() { WhenComputation = expressionDefinition };
                        break;
                    default:
                        throw new InvalidOperationException(String.Format(CdssTranspileErrors.EXPRESSION_CANNOT_BE_APPLIED_IN_CONTEXT, this.m_currentObject.Peek()));
                }
            }
            else if (this.m_currentComputationStack.Peek() is CdssAggregateExpressionDefinition ownerAgg)
            {
                ownerAgg.ContainedExpressions.Add(expressionDefinition);
            }
            else
            {
                throw new InvalidOperationException(CdssTranspileErrors.MULTI_EXPRESSION_MUST_BE_AGGREGATE);
            }

        }

        /// <summary>
        /// Try to extract uuid in {UUID} format
        /// </summary>
        private bool TryExtractUuid(ITerminalNode uuidToken, out Guid result)
        {
            return Guid.TryParse(uuidToken?.GetText(), out result);
        }

        private bool TryExtractString(ITerminalNode stringToken, out string result)
        {
            var match = s_extractStringContents.Match(stringToken?.GetText() ?? String.Empty);
            if (match.Success)
            {
                result = match.Groups[1].Value.Replace("\"\"", "\""); // remove double quote escape
            }
            else
            {
                result = String.Empty;
            }
            return match.Success;
        }

        private bool TryExtractIdentifier(ITerminalNode namedIdentifierToken, out string result)
        {
            var match = s_extractNamedIdentifier.Match(namedIdentifierToken?.GetText() ?? String.Empty);
            if (match.Success)
            {
                result = match.Groups[1].Value;
            }
            else
            {
                result = String.Empty;
            }
            return match.Success;
        }

        private bool TryExtractMultilineString(ITerminalNode multiLineStringToken, out string result)
        {
            var match = s_extractMultilineString.Match(multiLineStringToken?.GetText() ?? String.Empty);
            if (match.Success)
            {
                result = match.Groups[1].Value.Trim();
            }
            else
            {
                result = String.Empty;
            }
            return match.Success;
        }
    }
}
