// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace AsyncUsageAnalyzers.Test.Helpers
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    /// <summary>
    /// Metadata references used to create test projects.
    /// </summary>
    internal static class MetadataReferences
    {
        internal static readonly MetadataReference CorlibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location).WithAliases(ImmutableArray.Create("global", "corlib"));
        internal static readonly MetadataReference SystemReference = MetadataReference.CreateFromFile(typeof(System.Diagnostics.Debug).Assembly.Location).WithAliases(ImmutableArray.Create("global", "system"));
        internal static readonly MetadataReference SystemCoreReference = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
        internal static readonly MetadataReference CSharpSymbolsReference = MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);
        internal static readonly MetadataReference CodeAnalysisReference = MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);

        internal static readonly MetadataReference SystemThreadingTasksReference;
        internal static readonly MetadataReference SystemThreadingTasksExtensionsReference;

        static MetadataReferences()
        {
            if (typeof(ValueTask<>).Assembly == typeof(string).Assembly)
            {
                // mscorlib contains ValueTask<TResult>, so no need to add a separate reference
                SystemThreadingTasksReference = null;
                SystemThreadingTasksExtensionsReference = null;
            }
            else
            {
                Assembly systemThreadingTasks = AppDomain.CurrentDomain.GetAssemblies().Single(x => x.GetName().Name == "System.Threading.Tasks");
                SystemThreadingTasksReference = MetadataReference.CreateFromFile(systemThreadingTasks.Location);

                SystemThreadingTasksExtensionsReference = MetadataReference.CreateFromFile(typeof(ValueTask<>).Assembly.Location);
            }
        }
    }
}
