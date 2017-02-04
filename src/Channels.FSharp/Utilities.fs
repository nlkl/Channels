namespace Channels.FSharp

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
