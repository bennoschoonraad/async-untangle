using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Untangle.DiagnosticAnalyzers
{


    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AsyncHasAwait : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AsyncHasAwait-001";
        private static readonly LocalizableString Title = "Async methods should contain at least one await";
        private static readonly LocalizableString MessageFormat = "Method: {}, is async and should contain at least one await";
        private static readonly LocalizableString Description =
            @"An async method should contain at least one await.
             The async keyword signals to the compiler that the method is designed to be asynchronous and will, at some point, await a task. 
             If a method is marked as async but contains no await expressions, it will run synchronously.";
        private const string Category = "Design";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            // Skip methods that are not async
            if (!methodDeclaration.Modifiers.Any(SyntaxKind.AsyncKeyword))
            {
                return;
            }

            // Check if the method body contains an 'await' expression.
            bool hasAwait = methodDeclaration.DescendantNodes().Any(n => n.IsKind(SyntaxKind.AwaitExpression));

            if (!hasAwait)
            {
                var diagnostic = Diagnostic.Create(Rule, methodDeclaration.GetLocation(), methodDeclaration.Identifier.Text);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
