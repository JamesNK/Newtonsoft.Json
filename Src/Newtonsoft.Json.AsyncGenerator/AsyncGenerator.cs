#region License
// Copyright (c) 2022 Anton Tykhyy
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Newtonsoft.Json.AsyncGenerator
{
    [Generator]
    public class AsyncGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            Execute(context, "Reader", ReaderExtra);
            Execute(context, "Writer", WriterExtra, "WriteTokenInternalSyncReadingAsync", "WriteEndInternalAsync");
        }

        private void ReaderExtra(GeneratorExecutionContext context, INamedTypeSymbol jsr, Dictionary<ISymbol, string> jsrm)
        {
            foreach (IMethodSymbol method in context.Compilation.GetTypeByMetadataName("Newtonsoft.Json.Linq.JRaw").GetMembers("Create"))
                jsrm.Add(method, "Async");

            foreach (IMethodSymbol method in context.Compilation.GetTypeByMetadataName("Newtonsoft.Json.Linq.JToken").GetMembers("ReadFrom"))
                jsrm.Add(method, "Async");

            foreach (IMethodSymbol method in context.Compilation.GetTypeByMetadataName("Newtonsoft.Json.JsonWriter").GetMembers("WriteTokenInternal"))
                jsrm.Add(method, "SyncReadingAsync");
        }

        private void WriterExtra(GeneratorExecutionContext context, INamedTypeSymbol jsw, Dictionary<ISymbol, string> jswm)
        {
            foreach (IMethodSymbol method in context.Compilation.GetTypeByMetadataName("Newtonsoft.Json.Serialization.JsonProperty").GetMembers("WritePropertyName"))
                jswm.Add(method, "Async");

            foreach (IMethodSymbol method in context.Compilation.GetTypeByMetadataName("Newtonsoft.Json.Linq.JToken").GetMembers("WriteTo"))
                jswm.Add(method, "Async");
        }

        private void Execute(GeneratorExecutionContext context, string readerOrWriter,
            Action<GeneratorExecutionContext, INamedTypeSymbol, Dictionary<ISymbol, string>> addMethods, params string[] skipMethodNames)
        {
            // find all accessible async methods and their sync counterparts
            var jsrw = context.Compilation.GetTypeByMetadataName($"Newtonsoft.Json.Json{readerOrWriter}");
            var rwms = new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default);
            var skip = new HashSet<string>(skipMethodNames);
            foreach (var member in jsrw.GetMembers())
            {
                if (!(member is IMethodSymbol method) || method.DeclaredAccessibility == Accessibility.Private || !method.Name.EndsWith("Async") || skip.Contains(method.Name))
                    continue;

                var added = false;
                foreach (IMethodSymbol sync in jsrw.GetMembers(method.Name.Substring(0, method.Name.Length - "Async".Length)))
                {
                    if (sync.Parameters.Length + 1 != method.Parameters.Length)
                        continue;

                    for (var i = 0; i < sync.Parameters.Length; ++i)
                        if (!sync.Parameters[i].Type.Equals(method.Parameters[i].Type, SymbolEqualityComparer.Default))
                            goto skip;

                    if (added)
                    {
                        added = false;
                        break;
                    }

                    added = true;
                    rwms.Add(sync, "Async");
                skip:;
                }

                if (!added)
                    context.ReportDiagnostic(Diagnostic.Create("GEN0000", "Source Generator",
                        $"{method} does not have exactly one matching synchronous counterpart.",
                        DiagnosticSeverity.Error, DiagnosticSeverity.Error, true, 0, false, location: method.Locations[0]));
            }

            // add a few special methods on other types which consume Json{readerOrWriter}
            addMethods(context, jsrw, rwms);

            // collect candidates for conversion
            var jsirw = context.Compilation.GetTypeByMetadataName($"Newtonsoft.Json.Serialization.JsonSerializerInternal{readerOrWriter}");
            var maybe = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
            var locms = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
            foreach (var member in jsirw.GetMembers())
                if (member is IMethodSymbol method && !method.IsAsync)
                    foreach (var param in method.Parameters)
                        if (param.Type.Equals(jsrw, SymbolEqualityComparer.Default))
                        {
                            // pick up hand-written async methods
                            if (jsirw.GetMembers(method.Name + "Async").Length != 0)
                            {
                                locms.Add(method);
                            }
                            else
                                maybe.Add(method);

                            break;
                        }

            // compute the set of methods to convert by transitive closure
            var tocvt = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
            while (true)
            {
                var workDone = false;

                foreach (var method in maybe)
                {
                    var syntax =(CSharpSyntaxNode) method.DeclaringSyntaxReferences[0].GetSyntax();
                    var walker = new MustConvertWalker(rwms.Keys, locms, context.Compilation.GetSemanticModel(syntax.SyntaxTree));
                    syntax.Accept(walker);
                    if (walker.MustConvert)
                    {
                        maybe.Remove(method);
                        tocvt.Add(method);
                        locms.Add(method);
                        workDone = true;
                        break;
                    }
                }

                if (!workDone)
                    break;
            }

            // convert methods and generate output source file
            var tree   = System.Linq.Enumerable.First(tocvt).DeclaringSyntaxReferences[0].SyntaxTree;
            var model  = context.Compilation.GetSemanticModel(tree);
            var output = new System.IO.StringWriter();
            output.Write(@"// This code was generated by a tool.
// Changes to this file may cause incorrect behavior
// and will be lost if the code is regenerated.

");
            tree.GetText().Write(output, new Microsoft.CodeAnalysis.Text.TextSpan(0, tree.GetCompilationUnitRoot().Members.Span.Start));
            output.Write($@"#if HAVE_ASYNC
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Newtonsoft.Json.Serialization
{{
    partial class JsonSerializerInternal{readerOrWriter}
    {{");

            foreach (IMethodSymbol method in tocvt)
            {
                var syntax =(CSharpSyntaxNode) method.DeclaringSyntaxReferences[0].GetSyntax();
                new ConvertRewriter(method, rwms, locms, model).Visit(syntax).WriteTo(output);
            }

            // NB: preprocessor trivia attach to following item, which means that an #endif after a converted method
            // that is preceded by #if (in this case HAVE_DYNAMIC) is lost if the following method is not converted
            // here, I can get away with adding an extra #endif because HAVE_ASYNC implies HAVE_DYNAMIC, but yuck
            output.WriteLine(@"
    }
}
#endif
#endif");

            context.AddSource($"JsonSerializerInternal{readerOrWriter}.Async.g.cs", output.ToString());
        }

        sealed class MustConvertWalker : CSharpSyntaxWalker
        {
            private readonly ICollection<ISymbol> m_rwms ;
            private readonly ICollection<ISymbol> m_locms;
            private readonly SemanticModel        m_model;

            public bool MustConvert { get; private set; }

            public MustConvertWalker(ICollection<ISymbol> rwms, ICollection<ISymbol> locms, SemanticModel model)
            {
                m_rwms  = rwms ;
                m_locms = locms;
                m_model = model;
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                base.VisitInvocationExpression(node);
                var info = m_model.GetSymbolInfo(node);
                if (info.Symbol != null &&(m_rwms.Contains(info.Symbol) || m_locms.Contains(info.Symbol)))
                    MustConvert = true;
            }
        }

        sealed class ConvertRewriter : CSharpSyntaxRewriter
        {
            private readonly static SyntaxToken         AsyncToken    ;
            private readonly static SyntaxTriviaList    LeadingSpace  ;
            private readonly static SyntaxTriviaList    TrailingSpace ;
            private readonly static TypeSyntax          TaskType      ;
            private readonly static ParameterSyntax     CtParameter   ;
            private readonly static ArgumentSyntax      CtArgument1   ;
            private readonly static ArgumentSyntax      CtArgumentN   ;
            private readonly static SimpleNameSyntax    ConfigureAwait;
            private readonly static ArgumentListSyntax  FalseArgs     ;

            private readonly IMethodSymbol               m_method;
            private readonly ICollection<ISymbol>        m_locms ;
            private readonly SemanticModel               m_model ;
            private readonly Dictionary<ISymbol, string> m_rwms  ;

            static ConvertRewriter()
            {
                LeadingSpace   = SyntaxFactory.ParseLeadingTrivia(" ");
                TrailingSpace  = SyntaxFactory.ParseTrailingTrivia(" ");
                CtParameter    = SyntaxFactory.Parameter(default, default,
                    SyntaxFactory.ParseTypeName(typeof(System.Threading.CancellationToken).Name)
                        .WithLeadingTrivia(LeadingSpace)
                        .WithTrailingTrivia(TrailingSpace),
                    SyntaxFactory.Identifier("cancellationToken"),
                    default);

                AsyncToken     = SyntaxFactory.Token(SyntaxKind.AsyncKeyword).WithTrailingTrivia(TrailingSpace);
                TaskType       = SyntaxFactory.ParseTypeName(typeof(System.Threading.Tasks.Task).Name);
                CtArgument1    = SyntaxFactory.Argument(SyntaxFactory.IdentifierName(CtParameter.Identifier));
                CtArgumentN    = CtArgument1.WithLeadingTrivia(LeadingSpace);

                ConfigureAwait = SyntaxFactory.IdentifierName("ConfigureAwait");
                FalseArgs      = SyntaxFactory.ArgumentList(
                    new SeparatedSyntaxList<ArgumentSyntax>().Add(
                        SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression))));
            }

            public ConvertRewriter(IMethodSymbol method, Dictionary<ISymbol, string> rwms, ICollection<ISymbol> locms, SemanticModel model)
            {
                m_method = method;
                m_rwms   = rwms  ;
                m_locms  = locms ;
                m_model  = model ;
            }

            public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                var returnType = m_method.ReturnsVoid ? TaskType :
                    SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier("Task"),
                        SyntaxFactory.TypeArgumentList(new SeparatedSyntaxList<TypeSyntax>().Add(node.ReturnType.WithoutTrivia())));

                SyntaxTokenList modifiers;
                if (node.Modifiers.Any())
                {
                    modifiers  = node.Modifiers.Add(AsyncToken);
                    returnType = returnType.WithTriviaFrom(node.ReturnType);
                }
                else
                {
                    modifiers  = SyntaxFactory.TokenList(AsyncToken.WithLeadingTrivia(node.ReturnType.GetLeadingTrivia()));
                    returnType = returnType.WithTrailingTrivia(node.ReturnType.GetTrailingTrivia());
                }

                return node
                    .WithModifiers(modifiers)
                    .WithReturnType(returnType)
                    .WithIdentifier(SyntaxFactory.Identifier(node.Identifier.Text + "Async"))
                    .WithBody((BlockSyntax)VisitBlock(node.Body))
                    .AddParameterListParameters(CtParameter);
            }

            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax originalNode)
            {
                var node = (InvocationExpressionSyntax) base.VisitInvocationExpression(originalNode);
                var info = m_model.GetSymbolInfo(originalNode);
                if (info.Symbol == null)
                    return node;

                if (m_rwms.TryGetValue(info.Symbol, out var suffix))
                    return MakeAwait(node,((MemberAccessExpressionSyntax) node.Expression).WithName(
                        SyntaxFactory.IdentifierName(info.Symbol.Name + suffix)).WithLeadingTrivia(LeadingSpace));

                if (m_locms.Contains(info.Symbol))
                    return MakeAwait(node,
                        SyntaxFactory.IdentifierName(info.Symbol.Name + "Async").WithLeadingTrivia(LeadingSpace));

                return node;
            }

            private SyntaxNode MakeAwait(InvocationExpressionSyntax node, ExpressionSyntax expression) =>
                SyntaxFactory.AwaitExpression(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, node
                            .WithoutTrivia()
                            .WithExpression(expression)
                            .AddArgumentListArguments(node.ArgumentList.Arguments.Any() ? CtArgumentN : CtArgument1),
                            ConfigureAwait),
                        FalseArgs))
                    .WithTriviaFrom(node);
        }
    }
}
