module GamesFaix.MtgTools.Shared.Cli.Workspace

open Argu
open GamesFaix.MtgTools.Shared
open GamesFaix.MtgTools.Shared.Context

type Args =
    | [<AltCommandLine("-d")>] Dir of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Dir _ -> "Directory to set as workspace."

let command 
    (getWorkspace: unit -> string option Async)
    (setWorkspace: string -> unit Async)
    (args: Args ParseResults)
    (ctx: IContext) 
    : CommandResult =
    
    async {
        match args.TryGetResult Args.Dir with
        | Some dir ->
            ctx.Log.Information $"Setting workspace to {dir}..."
            do! setWorkspace dir

        | None ->
            match! getWorkspace () with
            | Some dir ->
                ctx.Log.Information $"Workspace is currently set to {dir}."
            | None ->
                ctx.Log.Information "Workspace not currently set."

        return Ok ()
    }
