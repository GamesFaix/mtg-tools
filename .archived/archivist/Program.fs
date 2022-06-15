module GamesFaix.MtgTools.Archivist.Program

open GamesFaix.MtgTools

[<EntryPoint>]
let main args =
    async {
        let! ctx = Context.loadContext ()
        return! 
            Shared.Program.main
                "archivist"
                Cli.Main.command
                args
                ctx
    }
    |> Async.RunSynchronously