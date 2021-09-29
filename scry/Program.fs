module GamesFaix.MtgTools.Scry.Program

open GamesFaix.MtgTools

[<EntryPoint>]
let main args =
    async {
        let! ctx = Context.loadContext ()
        return! Shared.Program.main
            "scry"
            Cli.Main.command
            args
            ctx
    } 
    |> Async.RunSynchronously