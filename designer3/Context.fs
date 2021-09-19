module GamesFaix.MtgTools.Designer.Context

open System.Net.Http
open Serilog
open Serilog.Events
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

let getWorkspace () =
    loadConfig ()
    |> Async.map (Option.map (fun c -> c.Workspace))

let setWorkspace (directory: string) : unit Async =
    loadConfig ()
    |> Async.map (function Some cfg -> { cfg with Workspace = directory }
                         | None -> { Workspace = directory })
    |> Async.bind saveConfig

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
        .WriteTo.Console()
        .CreateLogger()

let loadContext () : Context Async = async {
    match! getWorkspace () with
    | None ->
        return Context.Empty { Log = logger }
    | Some dir ->
        let workspace = Workspace.WorkspaceDirectory.create dir
        match! Auth.getCookie workspace with
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