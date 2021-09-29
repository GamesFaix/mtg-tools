module GamesFaix.MtgTools.Scry.Workspace

open System
open System.IO

type WorkspaceDirectory = {
    Path : string
    Cards : string
}
module WorkspaceDirectory =
    let create (rootDir: string) : WorkspaceDirectory =
        let rootDir =
            rootDir
            |> Environment.ExpandEnvironmentVariables
            |> Path.GetFullPath

        {
            Path = rootDir
            Cards = Path.Combine(rootDir, "cards.csv")
        }
