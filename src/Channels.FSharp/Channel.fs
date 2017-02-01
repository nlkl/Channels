﻿namespace Channels.FSharp

open Channels
open System.Threading

module Channel = 
    let createSynchronous () = SynchronousChannel<'T>() :> IChannel<'T>
    let createUnbounded () = UnboundedChannel<'T>() :> IChannel<'T>
    let createBuffered capacity = BufferedChannel<'T>(capacity) :> IChannel<'T>

    let tryInspect (channel : IInspectableChannel<'T>) =
        channel.TryInspect() |> PotentialValue.toOption

    let inspect (channel : IInspectableChannel<'T>) =
        channel.Inspect()

    let inspectOrCancel (channel : IInspectableChannel<'T>) (cancellationToken : CancellationToken) =
        channel.Inspect(cancellationToken)

    let inspectAsync (channel : IInspectableChannel<'T>) =
        channel.InspectAsync() |> Async.AwaitTask

    let inspectOrCancelAsync (channel : IInspectableChannel<'T>) (cancellationToken : CancellationToken) =
        channel.InspectAsync(cancellationToken) |> Async.AwaitTask

    let inspectDeferredAsync (channel : IInspectableChannel<'T>) =
        Async.runDeferred (inspectOrCancelAsync channel)

    let tryRead (channel : IReadableChannel<'T>) =
        channel.TryRead() |> PotentialValue.toOption

    let read (channel : IReadableChannel<'T>) =
        channel.Read()

    let readOrCancel (channel : IReadableChannel<'T>) (cancellationToken : CancellationToken) =
        channel.Read(cancellationToken)

    let readAsync (channel : IReadableChannel<'T>) =
        channel.ReadAsync() |> Async.AwaitTask

    let readOrCancelAsync (channel : IReadableChannel<'T>) (cancellationToken : CancellationToken) =
        channel.ReadAsync(cancellationToken) |> Async.AwaitTask

    let readDeferredAsync (channel : IReadableChannel<'T>) =
        Async.runDeferred (readOrCancelAsync channel)

    let tryWrite (channel : IWritableChannel<'T>) value =
        channel.TryWrite(value)

    let write (channel : IWritableChannel<'T>) value =
        channel.Write(value)

    let writeOrCancel (channel : IWritableChannel<'T>) value (cancellationToken : CancellationToken) =
        channel.Write(value, cancellationToken)

    let writeAsync (channel : IWritableChannel<'T>) value =
        channel.WriteAsync(value) |> Async.AwaitTask

    let writeOrCancelAsync (channel : IWritableChannel<'T>) value (cancellationToken : CancellationToken) =
        channel.WriteAsync(value, cancellationToken) |> Async.AwaitTask

    let writeDeferredAsync (channel : IWritableChannel<'T>) value =
        Async.runDeferred (writeOrCancelAsync channel value)
