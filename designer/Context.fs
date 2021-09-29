module GamesFaix.MtgTools.Designer.Context

open Serilog
open GamesFaix.MtgTools
open GamesFaix.MtgTools.Shared
open GamesFaix.MtgTools.Shared.Context
open Workspace

(*
    Flow:
    workspace None // displays the workspace
    workspace {dir} // saves config file setting workspace to dir
    login {user} {pass} // logs in a {user} {pass} and saves cookie to a file for later use
    login {user} {pass} --save // logs in and saves credentials for later use
    logout //log out and delete credentials file
*)

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
    interface IContext with
        member this.Log =
            match this with
            | Empty ctx -> ctx.Log
            | Workspace ctx -> ctx.Log
            | User ctx -> ctx.Log

let loadContext () : Context Async = async {
    match! Shared.Context.getWorkspace () with
    | None ->
        return Context.Empty { Log = Log.logger }
    | Some dir ->
        let workspace = Workspace.WorkspaceDirectory.create dir
        match! Auth.loadCookieFile workspace with
        | None ->
            return Context.Workspace {
                Log = Log.logger
                Workspace = workspace
            }
        | Some cookie ->
            return Context.User {
                Log = Log.logger
                Workspace = workspace
                Cookie = cookie
            }
}