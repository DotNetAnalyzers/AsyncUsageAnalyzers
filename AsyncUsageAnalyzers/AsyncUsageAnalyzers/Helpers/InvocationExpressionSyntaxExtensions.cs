// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

/* Contributor: Tomasz Maczyński */

namespace AsyncUsageAnalyzers.Helpers
{
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class InvocationExpressionSyntaxExtensions
    {
        public static bool TryGetMethodSymbolByTypeNameAndMethodName(
            this InvocationExpressionSyntax invocationExpression,
            SemanticModel semanticModel,
            string fullyQualifiedName,
            string methodName,
            out IMethodSymbol methodSymbol)
        {
            methodSymbol = ModelExtensions.GetSymbolInfo(semanticModel, invocationExpression).Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                return false;
            }

            var threadTypeMetadata = semanticModel.Compilation.GetTypeByMetadataName(fullyQualifiedName);
            if (!threadTypeMetadata.Equals(methodSymbol.ReceiverType))
            {
                return false;
            }

            if (methodSymbol.Name != methodName)
            {
                return false;
            }

            return true;
        }

        public static bool IsInsideAsyncCode(this InvocationExpressionSyntax invocationExpression, ref SyntaxNode enclosingMethodOrFunctionDeclaration)
        {
            foreach (var syntaxNode in invocationExpression.Ancestors())
            {
                var methodDeclaration = syntaxNode as MethodDeclarationSyntax;
                if (methodDeclaration != null)
                {
                    enclosingMethodOrFunctionDeclaration = syntaxNode;
                    return HasAsyncMethodModifier(methodDeclaration);
                }

                // This handles also AnonymousMethodExpressionSyntax since AnonymousMethodExpressionSyntax inherits from AnonymousFunctionExpressionSyntax
                var anonymousFunction = syntaxNode as AnonymousFunctionExpressionSyntax;
                if (anonymousFunction != null)
                {
                    enclosingMethodOrFunctionDeclaration = syntaxNode;
                    return IsAsyncAnonymousFunction(anonymousFunction);
                }
            }

            return false;
        }

        public static bool IsInsideAsyncCode(this InvocationExpressionSyntax invocationExpression)
        {
            SyntaxNode enclosingMethodOrFunctionDeclaration = null;
            return invocationExpression.IsInsideAsyncCode(ref enclosingMethodOrFunctionDeclaration);
        }

        private static bool HasAsyncMethodModifier(MethodDeclarationSyntax methodDeclaration) =>
            methodDeclaration.Modifiers.Any(x => x.Kind() == SyntaxKind.AsyncKeyword);

        private static bool IsAsyncAnonymousFunction(AnonymousFunctionExpressionSyntax anonymousFunctionExpressionSyntax) =>
            anonymousFunctionExpressionSyntax.AsyncKeyword.Kind() == SyntaxKind.AsyncKeyword;

    }
}
