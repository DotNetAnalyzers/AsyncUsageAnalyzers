// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace AsyncUsageAnalyzers.Test
{
    using Xunit;

    public class AttributesUnitTests
    {
        [Fact]
        public void TestNoCodeFixAttribute()
        {
            var attribute = new NoCodeFixAttribute("Message");
            Assert.Equal("Message", attribute.Reason);
        }
    }
}
