module GamesFaix.MtgTools.Designer.Cli.Workspace

open Argu
open GamesFaix.MtgTools.Designer
open GamesFaix.MtgTools.Shared
open GamesFaix.MtgTools.Shared.Context

type Args =
    | [<AltCommandLine("-d")>] Dir of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Dir _ -> "Directory to set as workspace."

let getJob (results: Args ParseResults) (ctx: IContext) : CommandResult =
    async {
        let dir = results.TryGetResult Args.Dir

        match dir with
        | Some d ->
            ctx.Log.Information $"Setting workspace to {d}..."
            do! Context.setWorkspace d
        | None ->
            match! Context.getWorkspace () with
            | Some d ->
                ctx.Log.Information $"Workspace is currently set to {d}."
            | None ->
                ctx.Log.Information "Workspace not currently set."

        return Ok ()
    }