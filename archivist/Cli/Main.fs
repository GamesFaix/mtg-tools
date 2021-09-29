module GamesFaix.MtgTools.Archivist.Cli.Main

open Argu
open GamesFaix.MtgTools
open GamesFaix.MtgTools.Archivist
open GamesFaix.MtgTools.Shared

type Args =
    | [<CliPrefix(CliPrefix.None)>] Workspace of Shared.Cli.Workspace.Args ParseResults
    | [<CliPrefix(CliPrefix.None)>] CreateInventory

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Workspace _ -> "Gets or sets the workspace directory"
            | CreateInventory -> "Creates a new current inventory file from all transactions in the workspace."

let command (args: Args ParseResults) (ctx: Context.Context) : CommandResult =
    async {
        match args.GetAllResults().Head with
        | Workspace args ->
            return! Shared.Cli.Workspace.command    
                        Context.getWorkspace
                        (fun dir -> async {
                            do! Context.setWorkspace dir
                            let workspace = Workspace.WorkspaceDirectory.create dir
                            FileSystem.createDirectoryIfMissing workspace.Inventory.Path
                            FileSystem.createDirectoryIfMissing workspace.Transactions.Path
                        })
                        args
                        ctx
        | CreateInventory ->
            match ctx with
            | Context.Workspace ctx -> return! InventoryGenerator.generate ctx
            | _ -> return Error "This operation requires a workspace."
    }
