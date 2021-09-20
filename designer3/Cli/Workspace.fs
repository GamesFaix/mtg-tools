module GamesFaix.MtgTools.Designer.Cli.Workspace

open Argu
open GamesFaix.MtgTools.Designer
open GamesFaix.MtgTools.Designer.Context

type Args =
    | [<CliPrefix(CliPrefix.None)>] Dir of string option

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Dir _ -> "Directory to set as workspace."

let getJob (ctx: Context) (results: Args ParseResults) : JobResult = async {
    let args = results.GetAllResults()
    let dir = args |> Seq.choose (fun a -> match a with Dir x -> x | _ -> None) |> Seq.tryHead

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