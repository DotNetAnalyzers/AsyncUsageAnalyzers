// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace AsyncUsageAnalyzers.Test
{
    using Xunit;

    public class AnalyzerConstantsUnitTests
    {
        [Fact]
        public void TestEnabledByDefault()
        {
            Assert.True(AnalyzerConstants.EnabledByDefault);
        }

        [Fact]
        public void TestDisabledByDefault()
        {
            Assert.False(AnalyzerConstants.DisabledByDefault);
        }

        [Fact]
        public void TestDisabledAlternative()
        {
            Assert.False(AnalyzerConstants.DisabledAlternative);
        }

        [Fact]
        public void TestDisabledNoTests()
        {
#if DEBUG
            Assert.True(AnalyzerConstants.DisabledNoTests);
#else
            Assert.False(AnalyzerConstants.DisabledNoTests);
#endif
        }
    }
}
