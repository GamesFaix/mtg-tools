module GamesFaix.MtgTools.Shared.Context

open Serilog

type IContext = 
    abstract member Log : ILogger

type EmptyContext = {
    Log : ILogger
}

type WorkspaceContext<'workspace> = {
    Log : ILogger
    Workspace : 'workspace
}

// Must be public for Newtonsoft to serialize it
[<CLIMutable>]
type Configuration = {
    Workspace: string
}

let private loadConfig () =
    FileSystem.loadFromJson<Configuration> "./configuration.json"

let private saveConfig (cfg: Configuration) =
    FileSystem.saveToJson cfg "./configuration.json"

let getWorkspace () = async {
    let! cfg = loadConfig ()
    return cfg |> Option.map (fun c -> c.Workspace)
}

let setWorkspace (directory: string) : unit Async = async {
    let! maybeConfig = loadConfig ()
    let cfg = match maybeConfig with
              | Some cfg -> { cfg with Workspace = directory }
              | None -> { Workspace = directory }
    return! saveConfig cfg
}