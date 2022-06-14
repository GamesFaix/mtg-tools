module GamesFaix.MtgTools.Designer.Workspace

open System.IO
open System

type SetDirectory = {
    Path : string
    HtmlLayout : string
    JsonDetails : string
    CenterFixes : string
    CardImage : string -> string
}
module SetDirectory =
    let getCardFileName (cardName: string) =
        let file = cardName.Replace(" ", "-").Replace("?", "-") + ".jpg"
        Path.Combine("cards", file)

    let create (rootDir: string) (name: string) : SetDirectory =
        let path = Path.Combine(rootDir, name)
        {
            Path = path
            HtmlLayout = Path.Combine(path, "layout.html")
            JsonDetails = Path.Combine(path, "details.json")
            CenterFixes = Path.Combine(path, "center-fixes.json")
            CardImage = (fun name -> Path.Combine(path, getCardFileName name))
        }

type WorkspaceDirectory = {
    Path : string
    Cookie : string
    Credentials : string
    Set : string -> SetDirectory
}
module WorkspaceDirectory =
    let create (rootDir: string) : WorkspaceDirectory =
        let rootDir =
            rootDir
            |> Environment.ExpandEnvironmentVariables
            |> Path.GetFullPath

        {
            Path = rootDir
            Cookie = Path.Combine(rootDir, "cookie.json")
            Credentials = Path.Combine(rootDir, "credentials.json")
            Set = SetDirectory.create rootDir
        }
