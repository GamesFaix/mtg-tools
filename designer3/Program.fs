module GamesFaix.MtgTools.Designer.Program

open Argu

[<EntryPoint>]
let main args =
    async {
        let! ctx = Context.loadContext ()
        ctx.Log.Information "mtg.design.cli"

        let parser = ArgumentParser.Create<Cli.Args>(programName = "designer")

        try
            let results = parser.Parse(inputs = args, raiseOnUsage = true)
            ctx.Log.Debug (results.ToString())

            let! job = Cli.getJob ctx results

            match job with
            | Ok () ->
                ctx.Log.Information "Done."
                return 0

            | Error err ->
                ctx.Log.Error err
                return -1
        with
        | ex ->
            ctx.Log.Error(ex, "An unexpected error occurred.")
            ctx.Log.Information (parser.PrintUsage())
            return -1
    }
    |> Async.RunSynchronously