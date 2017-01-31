open Channels.FSharp

[<EntryPoint>]
let main argv = 
    let chan = Channel.createBuffered 10
    let write = Channel.write chan
    write 1
    write 2
    write 3
    let read () = Channel.read chan
    printfn "Value: %i" (read ())
    printfn "Value: %i" (read ())
    printfn "Value: %i" (read ())

    let mvar = MVar.createEmpty ()
    MVar.put mvar 1
    printfn "Value: %i" (MVar.take mvar)
    let mvar = MVar.create 2
    printfn "Value: %i" (MVar.take mvar)

    System.Console.ReadKey() |> ignore
    0 
