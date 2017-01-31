namespace Channels.FSharp

open Channels

module internal PotentialValue =

    let toOption (potentialValue : PotentialValue<'T>) =
        let (success, value) = potentialValue.TryGetValue()
        if success then Some value else None

