open Channels.FSharp

[<EntryPoint>]
let main argv = 
    let chan = Channel.createBuffered 10

    [1 .. 10] |> List.iter (Channel.write chan)

    for i in 1 .. 10 do
        let value = Channel.read chan
        printfn "Value: %i" value

    let mvar = MVar.createEmpty ()
    MVar.put mvar 1
    printfn "Value: %i" (MVar.take mvar)
    let mvar = MVar.create 2
    printfn "Value: %i" (MVar.take mvar)

    let chan = Channel.createBuffered 5

    let writeTask = async {
        for i in 0 .. 100 do
            do! Channel.asyncWrite chan i
            printfn "Wrote: %i" i
    }

    let readTask = async {
        for i in 0 .. 100 do
            let! value = Channel.asyncRead chan
            printfn "Read: %i" value
    }

    let task = Async.Parallel([writeTask; readTask])
    Async.RunSynchronously(task) |> ignore

    System.Console.ReadKey() |> ignore
    0 
