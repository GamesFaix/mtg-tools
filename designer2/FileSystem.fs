module GamesFaix.MtgTools.Designer.FileSystem

open System.IO
open System.Text.RegularExpressions
open Newtonsoft.Json
open Model

let private createDirectoryIfMissing (path: string) : unit =
    if Directory.Exists path then ()
    else Directory.CreateDirectory path |> ignore

let private saveFileBytes (bytes: byte[]) (path: string): unit Async =
    async {
        createDirectoryIfMissing (Path.GetDirectoryName path)
        return! File.WriteAllBytesAsync(path, bytes) |> Async.AwaitTask
    }

let private saveFileText (text: string) (path: string): unit Async =
    async {
        createDirectoryIfMissing (Path.GetDirectoryName path)
        return! File.WriteAllTextAsync(path, text) |> Async.AwaitTask
    }

let getSetDir (rootDir: string) (setAbbrev: string) : string =
    $"{rootDir}/{setAbbrev}"

let getCardFileName (card: CardInfo): string =
    let name = card.Name.Replace(" ", "-").Replace("?", "-")
    $"{name}.jpg"

let getCardImagePath (rootDir: string) (card: CardInfo) : string =
    let dir = getSetDir rootDir card.Set
    let file = getCardFileName card
    $"{dir}/{file}"

let getHtmlLayoutPath (rootDir: string) (setAbbrev: string) : string =
    let dir = getSetDir rootDir setAbbrev
    $"{dir}/layout.html"

let getPdfLayoutPath (rootDir: string) (setAbbrev: string) : string =
    let dir = getSetDir rootDir setAbbrev
    $"{dir}/layout.pdf"

let getJsonDetailsPath (rootDir: string) (setAbbrev: string) : string =
    let dir = getSetDir rootDir setAbbrev
    $"{dir}/details.json"

let getCenterFixesPath (rootDir: string) (setAbbrev: string) : string =
    let dir = getSetDir rootDir setAbbrev
    $"{dir}/center-fixes.txt"

let getCookiePath (rootDir: string) : string =
    $"{rootDir}/cookie.txt"

let getCredentialsPath (rootDir: string) : string =
    $"{rootDir}/credentials.json"

let saveCardImage (rootDir: string) (bytes: byte[]) (card: CardInfo) : unit Async =
    let path = getCardImagePath rootDir card
    saveFileBytes bytes path

let saveHtmlLayout (rootDir: string) (html: string) (setAbbrev: string) : unit Async =
    let path = getHtmlLayoutPath rootDir setAbbrev
    saveFileText html path

let savePdfLayout (rootDir: string) (bytes: byte[]) (setAbbrev: string) : unit Async =
    let path = getPdfLayoutPath rootDir setAbbrev
    saveFileBytes bytes path

let saveJsonDetails (rootDir: string) (cards: CardDetails list) (setAbbrev: string) : unit Async =
    let options = JsonSerializerSettings()
    options.Formatting <- Formatting.Indented
    let json = JsonConvert.SerializeObject(cards, options)
    let path = getJsonDetailsPath rootDir setAbbrev
    saveFileText json path

let private loadFromJson<'a> (path: string) : 'a option Async =
    async {
        try
            let! json = File.ReadAllTextAsync path |> Async.AwaitTask
            let result = JsonConvert.DeserializeObject<'a> json
            return Some result
        with
        | _ ->
            return None
    }

let loadJsonDetails (rootDir: string) (setAbbrev: string) : CardDetails list option Async =
    let path = getJsonDetailsPath rootDir setAbbrev
    loadFromJson path

let private deleteFolderIfExists (path: string) : unit =
    if Directory.Exists path
    then Directory.Delete(path, true)
    else ()

let deleteSetFolder (rootDir: string) (setAbbrev: string) : unit =
    let path = getSetDir rootDir setAbbrev
    deleteFolderIfExists path

let private parseCenterFixes (text: string) : string list =
    (* File format is like this:
     ----------------------------------
       # Comments
       Card Title
       Card Title # Comments

       Card Title
     ----------------------------------
     One card title per line
     Anything after # is a comment
     Blank lines ignored
     *)

    let commentPattern = Regex("#.*")

    text.Split("\n")
    |> Seq.choose (fun line ->
        let cleanLine = commentPattern.Replace(line, "").Trim()
        match cleanLine with
        | "" -> None
        | x -> Some x
    )
    |> Seq.toList

/// <summary> This loads the center-fix file for a set. This is used to
/// compensate for a bug in mtg.design, where the IsCentered property is
/// returned as false for all cards, even those that have been set to true.
/// </summary>
let loadCenterFixes (rootDir: string) (setAbbrev: string) : string list =
    let path = getCenterFixesPath rootDir setAbbrev
    match File.Exists path with
    | false -> []
    | true ->
        File.ReadAllText path
        |> parseCenterFixes

let saveCookie (rootDir: string) (cookie: string) : unit =
    let path = getCookiePath rootDir
    File.WriteAllText(path, cookie)

let loadCredentials (rootDir: string) : Credentials option Async =
    let path = getCredentialsPath rootDir
    loadFromJson path