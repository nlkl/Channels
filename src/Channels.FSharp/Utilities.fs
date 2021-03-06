﻿namespace Channels.FSharp

open Channels

module internal Async =

    let defer run = 
        async {
            let! cancellationToken = Async.CancellationToken
            return! run(cancellationToken)
        }

module internal Option =

    let fromCSharpOption (potentialValue : PotentialValue<'T>) =
        let (success, value) = potentialValue.TryGetValue()
        if success then Some value else None

module internal Seq =

    let nullIgnoringCast source : seq<'TResult> =
        if isNull source then Unchecked.defaultof<seq<'TResult>> else Seq.cast source