namespace Channels.FSharp

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

    let asyncInspect (channel : IInspectableChannel<'T>) =
        channel.InspectAsync() |> Async.AwaitTask

    let asyncInspectOrCancel (channel : IInspectableChannel<'T>) (cancellationToken : CancellationToken) =
        channel.InspectAsync(cancellationToken) |> Async.AwaitTask

    let tryRead (channel : IReadableChannel<'T>) =
        channel.TryRead() |> PotentialValue.toOption

    let read (channel : IReadableChannel<'T>) =
        channel.Read()

    let readOrCancel (channel : IReadableChannel<'T>) (cancellationToken : CancellationToken) =
        channel.Read(cancellationToken)

    let asyncRead (channel : IReadableChannel<'T>) =
        channel.ReadAsync() |> Async.AwaitTask

    let asyncReadOrCancel (channel : IReadableChannel<'T>) (cancellationToken : CancellationToken) =
        channel.ReadAsync(cancellationToken) |> Async.AwaitTask

    let tryWrite (channel : IWritableChannel<'T>) value =
        channel.TryWrite(value)

    let write (channel : IWritableChannel<'T>) value =
        channel.Write(value)

    let writeOrCancel (channel : IWritableChannel<'T>) value (cancellationToken : CancellationToken) =
        channel.Write(value, cancellationToken)

    let asyncWrite (channel : IWritableChannel<'T>) value =
        channel.WriteAsync(value) |> Async.AwaitTask

    let asyncWriteOrCancel (channel : IWritableChannel<'T>) value (cancellationToken : CancellationToken) =
        channel.WriteAsync(value, cancellationToken) |> Async.AwaitTask

