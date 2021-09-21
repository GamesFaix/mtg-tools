module GamesFaix.MtgTools.Designer.Cli.Workspace

open Argu
open GamesFaix.MtgTools.Designer
open GamesFaix.MtgTools.Designer.Context

type Args =
    | [<AltCommandLine("-d")>] Dir of string option

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Dir _ -> "Directory to set as workspace."

let getJob (ctx: Context) (results: Args ParseResults) : JobResult = async {
    let dir = results.GetResult Args.Dir

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