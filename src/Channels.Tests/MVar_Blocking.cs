using Channels.Tests.Framework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Channels.Tests
{
    public class MVar_Blocking
    {
        [Fact(DisplayName = "MVar: Inspect blocks until mvar is full")]
        public async Task InspectBlocksUntilMVarIsFull()
        {
            var expectedValue = 23123;

            // TryInspect
            var mvar = new MVar<int>();
            TaskStarter.RunDelayed(() => mvar.Write(expectedValue)).FireAndForget();
            mvar.TryInspect().ShouldNotHaveValue();
            var potentialValue = mvar.TryInspect(Timeout.Infinite);
            potentialValue.ShouldHaveValue(expectedValue);

            mvar = new MVar<int>();
            TaskStarter.RunDelayed(() => mvar.Write(expectedValue)).FireAndForget();
            mvar.TryInspect().ShouldNotHaveValue();
            potentialValue = mvar.TryInspect(Timeout.Infinite, new CancellationToken());
            potentialValue.ShouldHaveValue(expectedValue);

            // Inspect
            mvar = new MVar<int>();
            TaskStarter.RunDelayed(() => mvar.Write(expectedValue)).FireAndForget();
            mvar.TryInspect().ShouldNotHaveValue();
            var value = mvar.Inspect();
            value.Should().Be(expectedValue);

            mvar = new MVar<int>();
            TaskStarter.RunDelayed(() => mvar.Write(expectedValue)).FireAndForget();
            mvar.TryInspect().ShouldNotHaveValue();
            value = mvar.Inspect(new CancellationToken());
            value.Should().Be(expectedValue);

            // TryInspectAsync
            mvar = new MVar<int>();
            TaskStarter.RunDelayed(() => mvar.Write(expectedValue)).FireAndForget();
            mvar.TryInspect().ShouldNotHaveValue();
            potentialValue = await mvar.TryInspectAsync(Timeout.Infinite);
            potentialValue.ShouldHaveValue(expectedValue);

            mvar = new MVar<int>();
            TaskStarter.RunDelayed(() => mvar.Write(expectedValue)).FireAndForget();
            mvar.TryInspect().ShouldNotHaveValue();
            potentialValue = await mvar.TryInspectAsync(Timeout.Infinite, new CancellationToken());
            potentialValue.ShouldHaveValue(expectedValue);

            // InspectAsync
            mvar = new MVar<int>();
            TaskStarter.RunDelayed(() => mvar.Write(expectedValue)).FireAndForget();
            mvar.TryInspect().ShouldNotHaveValue();
            value = await mvar.InspectAsync();
            value.Should().Be(expectedValue);

            mvar = new MVar<int>();
            TaskStarter.RunDelayed(() => mvar.Write(expectedValue)).FireAndForget();
            mvar.TryInspect().ShouldNotHaveValue();
            value = await mvar.InspectAsync(new CancellationToken());
            value.Should().Be(expectedValue);
        }

        [Fact(DisplayName = "MVar: Read blocks until mvar is full")]
        public async Task ReadBlocksUntilMVarIsFull()
        {
            var expectedValue = 23123;

            // TryRead
            var mvar = new MVar<int>();
            TaskStarter.RunDelayed(() => mvar.Write(expectedValue)).FireAndForget();
            mvar.TryInspect().ShouldNotHaveValue();
            var potentialValue = mvar.TryRead(Timeout.Infinite);
            potentialValue.ShouldHaveValue(expectedValue);

            mvar = new MVar<int>();
            TaskStarter.RunDelayed(() => mvar.Write(expectedValue)).FireAndForget();
            mvar.TryInspect().ShouldNotHaveValue();
            potentialValue = mvar.TryRead(Timeout.Infinite, new CancellationToken());
            potentialValue.ShouldHaveValue(expectedValue);

            // Read
            mvar = new MVar<int>();
            TaskStarter.RunDelayed(() => mvar.Write(expectedValue)).FireAndForget();
            mvar.TryInspect().ShouldNotHaveValue();
            var value = mvar.Read();
            value.Should().Be(expectedValue);

            mvar = new MVar<int>();
            TaskStarter.RunDelayed(() => mvar.Write(expectedValue)).FireAndForget();
            mvar.TryInspect().ShouldNotHaveValue();
            value = mvar.Read(new CancellationToken());
            value.Should().Be(expectedValue);

            // TryReadAsync
            mvar = new MVar<int>();
            TaskStarter.RunDelayed(() => mvar.Write(expectedValue)).FireAndForget();
            mvar.TryInspect().ShouldNotHaveValue();
            potentialValue = await mvar.TryReadAsync(Timeout.Infinite);
            potentialValue.ShouldHaveValue(expectedValue);

            mvar = new MVar<int>();
            TaskStarter.RunDelayed(() => mvar.Write(expectedValue)).FireAndForget();
            mvar.TryInspect().ShouldNotHaveValue();
            potentialValue = await mvar.TryReadAsync(Timeout.Infinite, new CancellationToken());
            potentialValue.ShouldHaveValue(expectedValue);

            // ReadAsync
            mvar = new MVar<int>();
            TaskStarter.RunDelayed(() => mvar.Write(expectedValue)).FireAndForget();
            mvar.TryInspect().ShouldNotHaveValue();
            value = await mvar.ReadAsync();
            value.Should().Be(expectedValue);

            mvar = new MVar<int>();
            TaskStarter.RunDelayed(() => mvar.Write(expectedValue)).FireAndForget();
            mvar.TryInspect().ShouldNotHaveValue();
            value = await mvar.ReadAsync(new CancellationToken());
            value.Should().Be(expectedValue);
        }

        [Fact(DisplayName = "MVar: Write blocks until mvar is empty")]
        public async Task WriteBlocksUntilMVarIsEmpty()
        {
            var value = 23123;

            // TryWrite
            var mvar = new MVar<int>(1);
            TaskStarter.RunDelayed(() => mvar.Read()).FireAndForget();
            mvar.TryWrite(value).Should().BeFalse();
            var success = mvar.TryWrite(value, Timeout.Infinite);
            success.Should().BeTrue();
            mvar.TryInspect().ShouldHaveValue(value);

            mvar = new MVar<int>(1);
            TaskStarter.RunDelayed(() => mvar.Read()).FireAndForget();
            mvar.TryWrite(value).Should().BeFalse();
            success = mvar.TryWrite(value, Timeout.Infinite, new CancellationToken());
            success.Should().BeTrue();
            mvar.TryInspect().ShouldHaveValue(value);

            // Write
            mvar = new MVar<int>(1);
            TaskStarter.RunDelayed(() => mvar.Read()).FireAndForget();
            mvar.TryWrite(value).Should().BeFalse();
            mvar.Write(value);
            mvar.TryInspect().ShouldHaveValue(value);

            mvar = new MVar<int>(1);
            TaskStarter.RunDelayed(() => mvar.Read()).FireAndForget();
            mvar.TryWrite(value).Should().BeFalse();
            mvar.Write(value, new CancellationToken());
            mvar.TryInspect().ShouldHaveValue(value);

            // TryWriteAsync
            mvar = new MVar<int>(1);
            TaskStarter.RunDelayed(() => mvar.Read()).FireAndForget();
            mvar.TryWrite(value).Should().BeFalse();
            success = await mvar.TryWriteAsync(value, Timeout.Infinite);
            success.Should().BeTrue();
            mvar.TryInspect().ShouldHaveValue(value);

            mvar = new MVar<int>(1);
            TaskStarter.RunDelayed(() => mvar.Read()).FireAndForget();
            mvar.TryWrite(value).Should().BeFalse();
            success = await mvar.TryWriteAsync(value, Timeout.Infinite, new CancellationToken());
            success.Should().BeTrue();
            mvar.TryInspect().ShouldHaveValue(value);

            // WriteAsync
            mvar = new MVar<int>(1);
            TaskStarter.RunDelayed(() => mvar.Read()).FireAndForget();
            mvar.TryWrite(value).Should().BeFalse();
            await mvar.WriteAsync(value);
            mvar.TryInspect().ShouldHaveValue(value);

            mvar = new MVar<int>(1);
            TaskStarter.RunDelayed(() => mvar.Read()).FireAndForget();
            mvar.TryWrite(value).Should().BeFalse();
            await mvar.WriteAsync(value, new CancellationToken());
            mvar.TryInspect().ShouldHaveValue(value);
        }
    }
}
