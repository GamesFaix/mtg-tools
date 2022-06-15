module GamesFaix.MtgTools.Designer.Workspace

open System.IO
open System
open GamesFaix.MtgTools.Shared.Utils

type WorkspaceDirectory = {
    Path : string
    Cookie : string
    Credentials : string
}
module WorkspaceDirectory =
    let create (rootDir: string) : WorkspaceDirectory =
        let rootDir =
            rootDir
            |> Environment.ExpandEnvironmentVariables
            |> Path.GetFullPath

        {
            Path = rootDir
            Cookie = rootDir /- "cookie.json"
            Credentials = rootDir /- "credentials.json"
        }
