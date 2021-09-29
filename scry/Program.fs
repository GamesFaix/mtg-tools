module GamesFaix.MtgTools.Scry.Program

open GamesFaix.MtgTools
open GamesFaix.MtgTools.Shared.Context

[<EntryPoint>]
let main args =
    async {
        let ctx = { new IContext with member _.Log = failwith "" }

        return! Shared.Program.main
            "scry"
            Cli.Main.command
            args
            ctx
    } 
    |> Async.RunSynchronously