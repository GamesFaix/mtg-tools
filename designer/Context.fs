module GamesFaix.MtgTools.Designer.Context

open Serilog
open GamesFaix.MtgTools
open GamesFaix.MtgTools.Shared
open GamesFaix.MtgTools.Shared.Context
open Workspace

type UserContext = {
    Log : ILogger
    Cookie : Auth.Cookie
    Workspace : WorkspaceDirectory
}

type Context =
    | Empty of EmptyContext
    | Workspace of Shared.Context.WorkspaceContext<WorkspaceDirectory>
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