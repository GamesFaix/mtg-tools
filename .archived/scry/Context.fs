module GamesFaix.MtgTools.Scry.Context

open Serilog
open GamesFaix.MtgTools
open GamesFaix.MtgTools.Shared
open GamesFaix.MtgTools.Shared.Context

type Context =
    | Empty of Shared.Context.EmptyContext
    | Workspace of WorkspaceContext<Workspace.WorkspaceDirectory>
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