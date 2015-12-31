// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace AsyncUsageAnalyzers.Helpers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    /// <summary>
    /// Provides helper methods to work with trivia (lists).
    /// </summary>
    internal static class TriviaHelper
    {
        /// <summary>
        /// Returns the index of the first non-whitespace trivia in the given trivia list.
        /// </summary>
        /// <param name="triviaList">The trivia list to process.</param>
        /// <param name="endOfLineIsWhitespace"><see langword="true"/> to treat <see cref="SyntaxKind.EndOfLineTrivia"/>
        /// as whitespace; otherwise, <see langword="false"/>.</param>
        /// <returns>The index where the non-whitespace starts, or -1 if there is no non-whitespace trivia.</returns>
        internal static int IndexOfFirstNonWhitespaceTrivia(SyntaxTriviaList triviaList, bool endOfLineIsWhitespace)
        {
            for (var index = 0; index < triviaList.Count; index++)
            {
                var currentTrivia = triviaList[index];
                switch (currentTrivia.Kind())
                {
                case SyntaxKind.EndOfLineTrivia:
                    if (!endOfLineIsWhitespace)
                    {
                        return index;
                    }

                    break;

                case SyntaxKind.WhitespaceTrivia:
                    break;

                default:
                    // encountered non-whitespace trivia -> the search is done.
                    return index;
                }
            }

            return -1;
        }
    }
}
