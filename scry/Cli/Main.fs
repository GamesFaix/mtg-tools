module GamesFaix.MtgTools.Scry.Cli.Main

open Argu
open GamesFaix.MtgTools
open GamesFaix.MtgTools.Scry
open GamesFaix.MtgTools.Shared

type Args =
    | [<CliPrefix(CliPrefix.None)>] Workspace of Shared.Cli.Workspace.Args ParseResults
    | [<CliPrefix(CliPrefix.None)>] Find of string
    | [<CliPrefix(CliPrefix.None)>] Collect of string
    | [<CliPrefix(CliPrefix.None)>] Report

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Workspace _ -> "Gets or sets the workspace directory"
            | Find _ -> "Finds all cards in inventory matching the given query, counting all printings of each card as equal."
            | Collect _ -> "Finds all cards matching the given query and shows which are owned, counting each printing as unique."
            | Report -> "Generates a report."

let command (args: Args ParseResults) (ctx: Context.Context) : CommandResult =
    async {
        match ctx, args.GetAllResults().Head with
        | _, Workspace args ->
            return! Shared.Cli.Workspace.command    
                        Shared.Context.getWorkspace
                        Shared.Context.setWorkspace
                        args
                        ctx
        | Context.Workspace ctx, Find query -> 
            return! Find.command query ctx
        | Context.Workspace ctx, Collect query ->
            return! Collect.command query ctx
        | Context.Workspace ctx, Report ->
            return! Report.command ctx
        | _ ->
            return Error "This operation requires a workspace."    
    }