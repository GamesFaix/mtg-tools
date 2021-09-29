module GamesFaix.MtgTools.Scry.Cli.Main

open Argu
open GamesFaix.MtgTools
open GamesFaix.MtgTools.Scry
open GamesFaix.MtgTools.Shared

type Args =
    | [<CliPrefix(CliPrefix.None)>] Workspace of Shared.Cli.Workspace.Args ParseResults
    | [<CliPrefix(CliPrefix.None)>] Query of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Workspace _ -> "Gets or sets the workspace directory"
            | Query _ -> "Queries inventory, using scryfall query syntax."

let command (args: Args ParseResults) (ctx: Context.Context) : CommandResult =
    async {
        match args.GetAllResults().Head with
        | Workspace args ->
            return! Shared.Cli.Workspace.command    
                        Shared.Context.getWorkspace
                        Shared.Context.setWorkspace
                        args
                        ctx
        | Query query -> 
            match ctx with
            | Context.Workspace ctx ->
                return! QueryWithUnownedCards.command query ctx
            | _ ->
                return Error "This operation requires a workspace."
    }