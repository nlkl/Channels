namespace Channels.FSharp

open Channels
open System.Threading

module MVar = 
    let createEmpty () = MVar<'T>() :> IMVar<'T>
    let create value = MVar<'T>(value) :> IMVar<'T>

    let tryPeek (mvar : IMVar<'T>) =
        mvar.TryInspect() |> PotentialValue.toOption

    let peek (mvar : IMVar<'T>) =
        mvar.Inspect()

    let peekOrCancel (mvar : IMVar<'T>) (cancellationToken : CancellationToken) =
        mvar.Inspect(cancellationToken)

    let asyncPeek (mvar : IMVar<'T>) =
        mvar.InspectAsync() |> Async.AwaitTask

    let asyncPeekOrCancel (mvar : IMVar<'T>) (cancellationToken : CancellationToken) =
        mvar.InspectAsync(cancellationToken) |> Async.AwaitTask

    let tryTake (mvar : IMVar<'T>) =
        mvar.TryRead() |> PotentialValue.toOption

    let take (mvar : IMVar<'T>) =
        mvar.Read()

    let takeOrCancel (mvar : IMVar<'T>) (cancellationToken : CancellationToken) =
        mvar.Read(cancellationToken)

    let asyncTake (mvar : IMVar<'T>) =
        mvar.ReadAsync() |> Async.AwaitTask

    let asyncTakeOrCancel (mvar : IMVar<'T>) (cancellationToken : CancellationToken) =
        mvar.ReadAsync(cancellationToken) |> Async.AwaitTask

    let tryPut (mvar : IMVar<'T>) value =
        mvar.TryWrite(value)

    let put (mvar : IMVar<'T>) value =
        mvar.Write(value)

    let putOrCancel (mvar : IMVar<'T>) value (cancellationToken : CancellationToken) =
        mvar.Write(value, cancellationToken)

    let asyncPut (mvar : IMVar<'T>) value =
        mvar.WriteAsync(value) |> Async.AwaitTask

    let asyncPutOrCancel (mvar : IMVar<'T>) value (cancellationToken : CancellationToken) =
        mvar.WriteAsync(value, cancellationToken) |> Async.AwaitTask

