namespace Channels.FSharp

open Channels
open System.Threading

module MVar = 
    let createEmpty () = MVar<'T>() :> IMVar<'T>
    let create value = MVar<'T>(value) :> IMVar<'T>

    let tryPeek (mvar : IMVar<'T>) =
        mvar.TryInspect() |> Option.fromCSharpOption

    let peek (mvar : IMVar<'T>) =
        mvar.Inspect()

    let peekOrCancel (mvar : IMVar<'T>) (cancellationToken : CancellationToken) =
        mvar.Inspect(cancellationToken)

    let peekAsync (mvar : IMVar<'T>) =
        mvar.InspectAsync() |> Async.AwaitTask

    let peekOrCancelAsync (mvar : IMVar<'T>) (cancellationToken : CancellationToken) =
        mvar.InspectAsync(cancellationToken) |> Async.AwaitTask

    let peekDeferredAsync (mvar : IMVar<'T>) =
        peekOrCancelAsync mvar |> Async.defer

    let tryTake (mvar : IMVar<'T>) =
        mvar.TryRead() |> Option.fromCSharpOption

    let take (mvar : IMVar<'T>) =
        mvar.Read()

    let takeOrCancel (mvar : IMVar<'T>) (cancellationToken : CancellationToken) =
        mvar.Read(cancellationToken)

    let takeAsync (mvar : IMVar<'T>) =
        mvar.ReadAsync() |> Async.AwaitTask

    let takeOrCancelAsync (mvar : IMVar<'T>) (cancellationToken : CancellationToken) =
        mvar.ReadAsync(cancellationToken) |> Async.AwaitTask

    let takeDeferredAsync (mvar : IMVar<'T>) =
        takeOrCancelAsync mvar |> Async.defer

    let tryPut (mvar : IMVar<'T>) value =
        mvar.TryWrite(value)

    let put (mvar : IMVar<'T>) value =
        mvar.Write(value)

    let putOrCancel (mvar : IMVar<'T>) value (cancellationToken : CancellationToken) =
        mvar.Write(value, cancellationToken)

    let putAsync (mvar : IMVar<'T>) value =
        mvar.WriteAsync(value) |> Async.AwaitTask

    let putOrCancelAsync (mvar : IMVar<'T>) value (cancellationToken : CancellationToken) =
        mvar.WriteAsync(value, cancellationToken) |> Async.AwaitTask

    let putDeferredAsync (mvar : IMVar<'T>) value =
        putOrCancelAsync mvar value |> Async.defer
