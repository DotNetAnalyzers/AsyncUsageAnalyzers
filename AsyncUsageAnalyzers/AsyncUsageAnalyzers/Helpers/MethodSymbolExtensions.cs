// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace AsyncUsageAnalyzers.Helpers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;

    internal static class MethodSymbolExtensions
    {
        public static bool HasAsyncSignature(this IMethodSymbol symbol, bool treatAsyncVoidAsAsync = false)
        {
            // void-returning methods are not asynchronous according to their signature, even if they use `async`
            if (symbol.ReturnsVoid)
            {
                if (treatAsyncVoidAsAsync)
                {
                    return symbol.IsAsync;
                }

                return false;
            }

            if (!symbol.IsAsync)
            {
                // This check conveniently covers Task and Task<T> by ignoring the `1 in Task<T>.
                if (!string.Equals(nameof(Task), symbol.ReturnType?.Name, StringComparison.Ordinal))
                {
                    return false;
                }

                if (!string.Equals(typeof(Task).Namespace, symbol.ReturnType?.ContainingNamespace?.ToString(), StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsTestMethod(this IMethodSymbol methodSymbol)
        {
            foreach (AttributeData attributeData in methodSymbol.GetAttributes())
            {
                var attributeClass = attributeData.AttributeClass;
                if (attributeClass == null)
                {
                    continue;
                }

                if (string.Equals(attributeClass.Name, "TestMethodAttribute", StringComparison.Ordinal)
                    || string.Equals(attributeClass.Name, "FactAttribute", StringComparison.Ordinal)
                    || string.Equals(attributeClass.Name, "TheoryAttribute", StringComparison.Ordinal)
                    || string.Equals(attributeClass.Name, "TestAttribute", StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsOverrideOrImplementation(this IMethodSymbol symbol)
        {
            if (symbol.IsOverride)
            {
                return true;
            }

            if (!symbol.ExplicitInterfaceImplementations.IsDefaultOrEmpty)
            {
                return true;
            }

            if ((symbol.ContainingType.TypeKind == TypeKind.Class || symbol.ContainingType.TypeKind == TypeKind.Struct)
                && symbol.DeclaredAccessibility == Accessibility.Public)
            {
                // As a final check, make sure the method isn't implicitly implementing an interface method
                foreach (INamedTypeSymbol interfaceType in symbol.ContainingType.AllInterfaces)
                {
                    foreach (var member in interfaceType.GetMembers(symbol.Name))
                    {
                        if (Equals(symbol.ContainingType.FindImplementationForInterfaceMember(member), symbol))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
