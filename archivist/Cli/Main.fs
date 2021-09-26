module GamesFaix.MtgTools.Archivist.Cli.Main

open Argu
open GamesFaix.MtgTools.Archivist
open GamesFaix.MtgTools.Archivist.Context
open GamesFaix.MtgTools.Archivist.Model

type Args =
    | [<CliPrefix(CliPrefix.None)>] Echo of string
    //| [<CliPrefix(CliPrefix.None)>] RefreshDb
    | [<CliPrefix(CliPrefix.None)>] Workspace of Workspace.Args ParseResults
    | [<CliPrefix(CliPrefix.None)>] CreateInventory

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Echo _ -> "Prints the input string"
            //| RefreshDb -> "Downloads card data from Scryfall."
            | Workspace _ -> "Gets or sets the workspace directory"
            | CreateInventory -> "Creates a new current inventory file from all transactions in the workspace."

let getJob (results: Args ParseResults) (ctx: Context) : JobResult =
    async {
        match results.GetAllResults().Head with
        | Echo str ->
            ctx.Log.Information str
            return Ok ()
        | Workspace args ->
            return! Workspace.getJob args ctx
        | CreateInventory ->
            match ctx with
            | Context.Workspace ctx -> return! InventoryGenerator.generate ctx
            | _ -> return Error "This operation requires a workspace."
    }
