module GamesFaix.MtgTools.Designer.Workspace

open System.IO
open System
open GamesFaix.MtgTools.Shared.Utils

type SetDirectory = {
    Path : string
    HtmlLayout : string
    JsonDetails : string
    CenterFixes : string
    CardImage : string -> string
}
module SetDirectory =
    let private escapeCardNameForPath (cardName: string) =
        cardName.Replace(" ", "-").Replace("?", "-")

    let getCardFileName (cardName: string) =
        "cards" /- (escapeCardNameForPath(cardName) + ".jpg")

    let getMpcCardFileName (cardName: string) =
        "mpc" /- (escapeCardNameForPath(cardName) + ".jpg")
        
    let create (rootDir: string) (name: string) : SetDirectory =
        let path = rootDir /- name
        {
            Path = path
            HtmlLayout = path /- "layout.html"
            JsonDetails = path /- "details.json"
            CenterFixes = path /- "center-fixes.json"
            CardImage = (fun name -> path /- getCardFileName name)
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
            Cookie = rootDir /- "cookie.json"
            Credentials = rootDir /- "credentials.json"
            Set = SetDirectory.create rootDir
        }
