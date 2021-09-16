module GamesFaix.MtgTools.Designer.FileSystem

open System.IO
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

let loadJsonDetails (rootDir: string) (setAbbrev: string) : CardDetails list option Async =
    async {
        try
            let path = getJsonDetailsPath rootDir setAbbrev
            let! json = File.ReadAllTextAsync path |> Async.AwaitTask
            let cards = JsonConvert.DeserializeObject<CardDetails list> json
            return Some cards
        with
        | _ ->
            return None
    }

let private deleteFolderIfExists (path: string) : unit =
    if Directory.Exists path
    then Directory.Delete(path, true)
    else ()

let deleteSetFolder (rootDir: string) (setAbbrev: string) : unit =
    let path = getSetDir rootDir setAbbrev
    deleteFolderIfExists path