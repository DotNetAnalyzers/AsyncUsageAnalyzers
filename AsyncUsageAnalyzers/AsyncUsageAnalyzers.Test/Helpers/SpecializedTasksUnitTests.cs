// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace AsyncUsageAnalyzers.Test.Helpers
{
    using System.Threading.Tasks;
    using AsyncUsageAnalyzers.Helpers;
    using Xunit;

    public class SpecializedTasksUnitTests
    {
        [Fact]
        public void TestCompletedTask()
        {
            Assert.NotNull(SpecializedTasks.CompletedTask);
            Assert.Equal(TaskStatus.RanToCompletion, SpecializedTasks.CompletedTask.Status);
            Assert.Same(SpecializedTasks.CompletedTask, SpecializedTasks.CompletedTask);
        }
    }
}
