module GamesFaix.MtgTools.Designer.Program

open GamesFaix.MtgTools

[<EntryPoint>]
let main args =
    async {
        let! ctx = Context.loadContext ()
        return! 
            Shared.Program.main
                "mtgd"
                Cli.Main.getJob
                args
                ctx
    }
    |> Async.RunSynchronously