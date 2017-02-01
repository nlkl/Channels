namespace Channels.FSharp

module internal Async =

    let defer run = 
        async {
            let! cancellationToken = Async.CancellationToken
            return! run(cancellationToken)
        }
