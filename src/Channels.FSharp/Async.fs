namespace Channels.FSharp

module internal Async =

    let runDeferred run = 
        async {
            let! cancellationToken = Async.CancellationToken
            return! run(cancellationToken)
        }
