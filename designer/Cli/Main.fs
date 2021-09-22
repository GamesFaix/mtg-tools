module GamesFaix.MtgTools.Designer.Cli.Main

open Argu
open GamesFaix.MtgTools.Designer.Cli
open GamesFaix.MtgTools.Designer.Context

type Args =
    | [<CliPrefix(CliPrefix.None)>] Workspace of Workspace.Args ParseResults
    | [<CliPrefix(CliPrefix.None)>] Login of Login.Args ParseResults
    //| [<CliPrefix(CliPrefix.None)>] Logout of Logout.Args ParseResults
    | [<CliPrefix(CliPrefix.None)>] Set of Set.Args ParseResults
    | [<CliPrefix(CliPrefix.None)>] Card of Card.Args ParseResults

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Workspace _ -> "Gets or sets workspace directory for later requests"
            | Login _ -> "Authenticates and saves cookie for later requests"
            //| Logout _ -> "Logs out"
            | Set _ -> "Performs operations on sets of cards."
            | Card _ -> "Performs operations on individual cards."

let getJob (ctx: Context) (results: Args ParseResults) : JobResult =
    match results.GetAllResults().Head with
    | Workspace results -> Workspace.getJob ctx results
    | Login results -> Login.getJob ctx results
    //| Logout results -> failwith "Not implemented"
    | Set results -> Set.getJob ctx results
    | Card results -> Card.getJob ctx results
