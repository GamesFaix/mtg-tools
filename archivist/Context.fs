module GamesFaix.MtgTools.Archivist.Context

open Serilog
open GamesFaix.MtgTools
open GamesFaix.MtgTools.Shared
open GamesFaix.MtgTools.Shared.Context
open Workspace

(*
    Flow:
    workspace None // displays the workspace
    workspace {dir} // saves config file setting workspace to dir
*)

type WorkspaceContext = {
    Log : ILogger
    Workspace : WorkspaceDirectory
}

type Context =
    | Empty of Shared.Context.EmptyContext
    | Workspace of WorkspaceContext
with    
    interface IContext with
        member this.Log =
            match this with
            | Empty ctx -> ctx.Log
            | Workspace ctx -> ctx.Log

let loadContext () : Context Async = async {
    match! Shared.Context.getWorkspace () with
    | None ->
        return Context.Empty { Log = Log.logger }
    | Some dir ->
        return Context.Workspace {
            Log = Log.logger
            Workspace = Workspace.WorkspaceDirectory.create dir
        }
}