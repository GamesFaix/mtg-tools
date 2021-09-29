module GamesFaix.MtgTools.Scry.Cli.Main

open Argu
open GamesFaix.MtgTools.Scry
open GamesFaix.MtgTools.Shared
open GamesFaix.MtgTools.Shared.Context

type Args =
    | [<CliPrefix(CliPrefix.None)>] Query of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Query _ -> "Queries inventory, using scryfall query syntax."

let command (args: Args ParseResults) (ctx: IContext) : CommandResult =
    async {
        match args.GetAllResults().Head with
        | Query query -> 
            return! Query.command query "inventory-path" ctx
    }