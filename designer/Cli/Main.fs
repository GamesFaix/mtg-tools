module GamesFaix.MtgTools.Designer.Cli.Main

open Argu
open FSharpx.Reader
open GamesFaix.MtgTools
open GamesFaix.MtgTools.Designer
open GamesFaix.MtgTools.Designer.Cli

type Args =
    | [<CliPrefix(CliPrefix.None)>] Workspace of Shared.Cli.Workspace.Args ParseResults
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

let getJob (results: Args ParseResults) : Reader<Context.Context, Shared.Types.CommandResult> =
    match results.GetAllResults().Head with
    | Workspace args -> 
        Shared.Cli.Workspace.command
            Context.getWorkspace
            Context.setWorkspace
            args

    | Login results -> Login.getJob results
    //| Logout results -> failwith "Not implemented"
    | Set results -> Set.getJob results
    | Card results -> Card.getJob results
