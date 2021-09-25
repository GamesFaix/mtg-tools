module GamesFaix.MtgTools.Archivist.Workspace

open System.IO
open System

type WorkspaceDirectory = {
    Path : string
}
module WorkspaceDirectory =
    let create (rootDir: string) : WorkspaceDirectory =
        let rootDir =
            rootDir
            |> Environment.ExpandEnvironmentVariables
            |> Path.GetFullPath

        {
            Path = rootDir
        }
