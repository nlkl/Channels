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
    public class MVar_BasicOperations
    {
        [Fact(DisplayName = "MVar: Can create empty mvar")]
        public void CanCreateEmptyMVar()
        {
            var mvar = new MVar<int>();
            mvar.TryInspect().ShouldNotHaveValue();
        }

        [Fact(DisplayName = "MVar: Can create full mvar")]
        public void CanCreateFullMVar()
        {
            var mvar = new MVar<int>(1);
            mvar.TryInspect().ShouldHaveValue(1);
        }

        [Fact(DisplayName = "MVar: Can inspect full mvar")]
        public async Task CanInspectFullMVar()
        {
            var expectedValue = 212;

            var mvar = new MVar<int>(expectedValue);
            var potentialValue = mvar.TryInspect();
            potentialValue.ShouldHaveValue(expectedValue);

            mvar = new MVar<int>(expectedValue);
            potentialValue = mvar.TryInspect(1000);
            potentialValue.ShouldHaveValue(expectedValue);

            mvar = new MVar<int>(expectedValue);
            potentialValue = mvar.TryInspect(1000, new CancellationToken());
            potentialValue.ShouldHaveValue(expectedValue);

            mvar = new MVar<int>(expectedValue);
            var value = mvar.Inspect();
            value.Should().Be(expectedValue);

            mvar = new MVar<int>(expectedValue);
            value = mvar.Inspect(new CancellationToken());
            value.Should().Be(expectedValue);

            mvar = new MVar<int>(expectedValue);
            potentialValue = await mvar.TryInspectAsync(1000);
            potentialValue.ShouldHaveValue(expectedValue);

            mvar = new MVar<int>(expectedValue);
            potentialValue = await mvar.TryInspectAsync(1000, new CancellationToken());
            potentialValue.ShouldHaveValue(expectedValue);

            mvar = new MVar<int>(expectedValue);
            value = await mvar.InspectAsync();
            value.Should().Be(expectedValue);

            mvar = new MVar<int>(expectedValue);
            value = await mvar.InspectAsync(new CancellationToken());
            value.Should().Be(expectedValue);
        }

        [Fact(DisplayName = "MVar: Can read from full mvar")]
        public async Task CanReadFromFullMvar()
        {
            var expectedValue = 212;

            var mvar = new MVar<int>(expectedValue);
            var potentialValue = mvar.TryRead();
            potentialValue.ShouldHaveValue(expectedValue);

            mvar = new MVar<int>(expectedValue);
            potentialValue = mvar.TryRead(1000);
            potentialValue.ShouldHaveValue(expectedValue);

            mvar = new MVar<int>(expectedValue);
            potentialValue = mvar.TryRead(1000, new CancellationToken());
            potentialValue.ShouldHaveValue(expectedValue);

            mvar = new MVar<int>(expectedValue);
            var value = mvar.Read();
            value.Should().Be(expectedValue);

            mvar = new MVar<int>(expectedValue);
            value = mvar.Read(new CancellationToken());
            value.Should().Be(expectedValue);

            mvar = new MVar<int>(expectedValue);
            potentialValue = await mvar.TryReadAsync(1000);
            potentialValue.ShouldHaveValue(expectedValue);

            mvar = new MVar<int>(expectedValue);
            potentialValue = await mvar.TryReadAsync(1000, new CancellationToken());
            potentialValue.ShouldHaveValue(expectedValue);

            mvar = new MVar<int>(expectedValue);
            value = await mvar.ReadAsync();
            value.Should().Be(expectedValue);

            mvar = new MVar<int>(expectedValue);
            value = await mvar.ReadAsync(new CancellationToken());
            value.Should().Be(expectedValue);
        }

        [Fact(DisplayName = "MVar: Can write to empty mvar")]
        public async Task CanWriteToEmptyMVar()
        {
            var value = 212;

            var mvar = new MVar<int>();
            var success = mvar.TryWrite(value);
            success.Should().BeTrue();
            mvar.TryInspect().ShouldHaveValue(value);

            mvar = new MVar<int>();
            success = mvar.TryWrite(value, 1000);
            success.Should().BeTrue();
            mvar.TryInspect().ShouldHaveValue(value);

            mvar = new MVar<int>();
            success = mvar.TryWrite(value, 1000, new CancellationToken());
            success.Should().BeTrue();
            mvar.TryInspect().ShouldHaveValue(value);

            mvar = new MVar<int>();
            mvar.Write(value);
            mvar.TryInspect().ShouldHaveValue(value);

            mvar = new MVar<int>();
            mvar.Write(value, new CancellationToken());
            mvar.TryInspect().ShouldHaveValue(value);

            mvar = new MVar<int>();
            success = await mvar.TryWriteAsync(value, 1000);
            success.Should().BeTrue();
            mvar.TryInspect().ShouldHaveValue(value);

            mvar = new MVar<int>();
            success = await mvar.TryWriteAsync(value, 1000, new CancellationToken());
            success.Should().BeTrue();
            mvar.TryInspect().ShouldHaveValue(value);

            mvar = new MVar<int>();
            await mvar.WriteAsync(value);
            mvar.TryInspect().ShouldHaveValue(value);

            mvar = new MVar<int>();
            await mvar.WriteAsync(value, new CancellationToken());
            mvar.TryInspect().ShouldHaveValue(value);
        }

        [Fact(DisplayName = "MVar: Cannot write to full mvar")]
        public void CannotWriteToFullMVar()
        {
            var mvar = new MVar<int>(1);
            mvar.TryWrite(1).Should().BeFalse();
        }

        [Fact(DisplayName = "MVar: Cannot read from empty mvar")]
        public void CannotReadFromEmptyMVar()
        {
            var mvar = new MVar<int>();
            mvar.TryRead().ShouldNotHaveValue();
        }

        [Fact(DisplayName = "MVar: Cannot inspect empty mvar")]
        public void CannotInspectEmptyMVar()
        {
            var mvar = new MVar<int>();
            mvar.TryInspect().ShouldNotHaveValue();
        }

        [Fact(DisplayName = "MVar: Write fills mvar")]
        public void WriteFillsMVar()
        {
            var mvar = new MVar<int>();
            mvar.TryWrite(1).Should().BeTrue();
            mvar.TryWrite(1).Should().BeFalse();
        }

        [Fact(DisplayName = "MVar: Read empties mvar")]
        public void ReadEmptiesMVar()
        {
            var mvar = new MVar<int>(1);
            mvar.TryRead().ShouldHaveValue(1);
            mvar.TryRead().ShouldNotHaveValue();
        }

        [Fact(DisplayName = "MVar: Inspect does not empty mvar")]
        public void InspectDoesNotEmptyMVar()
        {
            var mvar = new MVar<int>(1);
            mvar.TryInspect().ShouldHaveValue(1);
            mvar.TryInspect().ShouldHaveValue(1);
        }
    }
}
