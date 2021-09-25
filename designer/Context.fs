module GamesFaix.MtgTools.Designer.Context

open Serilog
open Workspace

(*
    Flow:
    workspace None // displays the workspace
    workspace {dir} // saves config file setting workspace to dir
    login {user} {pass} // logs in a {user} {pass} and saves cookie to a file for later use
    login {user} {pass} --save // logs in and saves credentials for later use
    logout //log out and delete credentials file
*)

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

type EmptyContext = {
    Log : ILogger
}

type WorkspaceContext = {
    Log : ILogger
    Workspace : WorkspaceDirectory
}

type UserContext = {
    Log : ILogger
    Cookie : Auth.Cookie
    Workspace : WorkspaceDirectory
}

type Context =
    | Empty of EmptyContext
    | Workspace of WorkspaceContext
    | User of UserContext
with
    member this.Log =
        match this with
        | Empty ctx -> ctx.Log
        | Workspace ctx -> ctx.Log
        | User ctx -> ctx.Log

let logger =
    LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .CreateLogger()

let loadContext () : Context Async = async {
    match! getWorkspace () with
    | None ->
        return Context.Empty { Log = logger }
    | Some dir ->
        let workspace = Workspace.WorkspaceDirectory.create dir
        match! Auth.loadCookieFile workspace with
        | None ->
            return Context.Workspace {
                Log = logger
                Workspace = workspace
            }
        | Some cookie ->
            return Context.User {
                Log = logger
                Workspace = workspace
                Cookie = cookie
            }
}