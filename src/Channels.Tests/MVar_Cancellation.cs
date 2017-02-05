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
    public class MVar_Cancellation
    {
        [Fact(DisplayName = "MVar: Can cancel inspects")]
        public void CanCancelInspects()
        {
            var mvar = new MVar<int>();
            var timeout = 100;

            var cts = new CancellationTokenSource(timeout);
            mvar.Invoking(m => m.Inspect(cts.Token)).ShouldThrow<OperationCanceledException>();

            cts = new CancellationTokenSource(timeout);
            mvar.Invoking(m => m.TryInspect(Timeout.Infinite, cts.Token)).ShouldThrow<OperationCanceledException>();

            cts = new CancellationTokenSource(timeout);
            mvar.Awaiting(m => m.InspectAsync(cts.Token)).ShouldThrow<OperationCanceledException>();

            cts = new CancellationTokenSource(timeout);
            mvar.Awaiting(m => m.TryInspectAsync(Timeout.Infinite, cts.Token)).ShouldThrow<OperationCanceledException>();
        }

        [Fact(DisplayName = "MVar: Can cancel reads")]
        public void CanCancelReads()
        {
            var mvar = new MVar<int>();
            var timeout = 100;

            var cts = new CancellationTokenSource(timeout);
            mvar.Invoking(m => m.Read(cts.Token)).ShouldThrow<OperationCanceledException>();

            cts = new CancellationTokenSource(timeout);
            mvar.Invoking(m => m.TryRead(Timeout.Infinite, cts.Token)).ShouldThrow<OperationCanceledException>();

            cts = new CancellationTokenSource(timeout);
            mvar.Awaiting(m => m.ReadAsync(cts.Token)).ShouldThrow<OperationCanceledException>();

            cts = new CancellationTokenSource(timeout);
            mvar.Awaiting(m => m.TryReadAsync(Timeout.Infinite, cts.Token)).ShouldThrow<OperationCanceledException>();
        }

        [Fact(DisplayName = "MVar: Can cancel writes")]
        public void CanCancelWrites()
        {
            var mvar = new MVar<int>(1);
            var timeout = 100;

            var cts = new CancellationTokenSource(timeout);
            mvar.Invoking(m => m.Write(1, cts.Token)).ShouldThrow<OperationCanceledException>();

            cts = new CancellationTokenSource(timeout);
            mvar.Invoking(m => m.TryWrite(1, Timeout.Infinite, cts.Token)).ShouldThrow<OperationCanceledException>();

            cts = new CancellationTokenSource(timeout);
            mvar.Awaiting(m => m.WriteAsync(1, cts.Token)).ShouldThrow<OperationCanceledException>();

            cts = new CancellationTokenSource(timeout);
            mvar.Awaiting(m => m.TryWriteAsync(1, Timeout.Infinite, cts.Token)).ShouldThrow<OperationCanceledException>();
        }

        [Fact(DisplayName = "MVar: Can timeout inspects")]
        public async Task CanTimeoutInspects()
        {
            var mvar = new MVar<int>();
            var timeout = 100;

            var value = mvar.TryInspect(timeout);
            value.ShouldNotHaveValue();

            value = mvar.TryInspect(timeout, new CancellationToken());
            value.ShouldNotHaveValue();

            value = await mvar.TryInspectAsync(timeout);
            value.ShouldNotHaveValue();

            value = await mvar.TryInspectAsync(timeout, new CancellationToken());
            value.ShouldNotHaveValue();
        }

        [Fact(DisplayName = "MVar: Can timeout reads")]
        public async Task CanTimeoutReads()
        {
            var mvar = new MVar<int>();
            var timeout = 100;

            var value = mvar.TryRead(timeout);
            value.ShouldNotHaveValue();

            value = mvar.TryRead(timeout, new CancellationToken());
            value.ShouldNotHaveValue();

            value = await mvar.TryReadAsync(timeout);
            value.ShouldNotHaveValue();

            value = await mvar.TryReadAsync(timeout, new CancellationToken());
            value.ShouldNotHaveValue();
        }

        [Fact(DisplayName = "MVar: Can timeout writes")]
        public async Task CanTimeoutWrites()
        {
            var mvar = new MVar<int>(1);
            var timeout = 100;

            var success = mvar.TryWrite(1, timeout);
            success.Should().BeFalse();

            success = mvar.TryWrite(1, timeout, new CancellationToken());
            success.Should().BeFalse();

            success = await mvar.TryWriteAsync(1, timeout);
            success.Should().BeFalse();

            success = await mvar.TryWriteAsync(1, timeout, new CancellationToken());
            success.Should().BeFalse();
        }

        [Fact(DisplayName = "MVar: Cancelling inspects keeps integrity")]
        public async Task CancellingInspectsKeepsIntegrity()
        {
            var mvar = new MVar<int>();

            for (int i = 0; i <= 10; i++)
            {
                var timeout = i * 25;
                var cts = new CancellationTokenSource(timeout);
                await Task.WhenAll(
                    TaskStarter.RunAndCatch(() => mvar.Inspect(cts.Token)),
                    TaskStarter.RunAndCatch(() => mvar.TryInspect(Timeout.Infinite, cts.Token)),
                    TaskStarter.RunAndCatch(() => mvar.TryInspect(timeout, new CancellationToken())),
                    TaskStarter.RunAndCatch(() => mvar.InspectAsync(cts.Token)),
                    TaskStarter.RunAndCatch(() => mvar.TryInspectAsync(Timeout.Infinite, cts.Token)),
                    TaskStarter.RunAndCatch(() => mvar.TryInspectAsync(timeout, new CancellationToken()))
                );
            }

            var value = 2318;
            mvar.Write(value);
            mvar.TryInspect().ShouldHaveValue(value);
            mvar.TryRead().ShouldHaveValue(value);
            mvar.TryInspect().ShouldNotHaveValue();
        }

        [Fact(DisplayName = "MVar: Cancelling reads keeps integrity")]
        public async Task CancellingReadsKeepsIntegrity()
        {
            var mvar = new MVar<int>();

            for (int i = 0; i <= 10; i++)
            {
                var timeout = i * 25;
                var cts = new CancellationTokenSource(timeout);
                await Task.WhenAll(
                    TaskStarter.RunAndCatch(() => mvar.Read(cts.Token)),
                    TaskStarter.RunAndCatch(() => mvar.TryRead(Timeout.Infinite, cts.Token)),
                    TaskStarter.RunAndCatch(() => mvar.TryRead(timeout, new CancellationToken())),
                    TaskStarter.RunAndCatch(() => mvar.ReadAsync(cts.Token)),
                    TaskStarter.RunAndCatch(() => mvar.TryReadAsync(Timeout.Infinite, cts.Token)),
                    TaskStarter.RunAndCatch(() => mvar.TryReadAsync(timeout, new CancellationToken()))
                );
            }

            var value = 2318;
            mvar.Write(value);
            mvar.TryInspect().ShouldHaveValue(value);
            mvar.TryRead().ShouldHaveValue(value);
            mvar.TryInspect().ShouldNotHaveValue();
        }

        [Fact(DisplayName = "MVar: Cancelling writes keeps integrity")]
        public async Task CancellingWritesKeepsIntegrity()
        {
            var value = 2318;
            var mvar = new MVar<int>(value);

            for (int i = 0; i <= 10; i++)
            {
                var timeout = i * 25;
                var cts = new CancellationTokenSource(timeout);
                await Task.WhenAll(
                    TaskStarter.RunAndCatch(() => mvar.Write(1, cts.Token)),
                    TaskStarter.RunAndCatch(() => mvar.TryWrite(1, Timeout.Infinite, cts.Token)),
                    TaskStarter.RunAndCatch(() => mvar.TryWrite(1, timeout, new CancellationToken())),
                    TaskStarter.RunAndCatch(() => mvar.WriteAsync(1, cts.Token)),
                    TaskStarter.RunAndCatch(() => mvar.TryWriteAsync(1, Timeout.Infinite, cts.Token)),
                    TaskStarter.RunAndCatch(() => mvar.TryWriteAsync(1, timeout, new CancellationToken()))
                );
            }

            mvar.TryInspect().ShouldHaveValue(value);
            mvar.TryRead().ShouldHaveValue(value);
            mvar.TryInspect().ShouldNotHaveValue();

            var otherValue = 885;
            mvar.Write(otherValue);
            mvar.TryInspect().ShouldHaveValue(otherValue);
        }
    }
}
