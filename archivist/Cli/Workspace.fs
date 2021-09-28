module GamesFaix.MtgTools.Archivist.Cli.Workspace

open Argu
open GamesFaix.MtgTools.Archivist
open GamesFaix.MtgTools.Archivist.Context
open GamesFaix.MtgTools.Archivist.Model
open GamesFaix.MtgTools.Shared

type Args =
    | [<AltCommandLine("-d")>] Dir of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Dir _ -> "Directory to set as workspace."

let getJob (results: Args ParseResults) (ctx: Context) : JobResult =
    async {
        let dir = results.TryGetResult Args.Dir

        match dir with
        | Some d ->
            ctx.Log.Information $"Setting workspace to {d}..."
            do! Context.setWorkspace d
            let workspace = Workspace.WorkspaceDirectory.create d
            FileSystem.createDirectoryIfMissing workspace.Inventory.Path
            FileSystem.createDirectoryIfMissing workspace.Transactions.Path

        | None ->
            match! Context.getWorkspace () with
            | Some d ->
                ctx.Log.Information $"Workspace is currently set to {d}."
            | None ->
                ctx.Log.Information "Workspace not currently set."

        return Ok ()
    }