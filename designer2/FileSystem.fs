module GamesFaix.MtgTools.Designer.FileSystem

open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions
open Newtonsoft.Json
open GamesFaix.MtgTools.Designer.Model

let private createDirectoryIfMissing (path: string) : unit =
    if Directory.Exists path then ()
    else Directory.CreateDirectory path |> ignore

let private deleteFolderIfExists (path: string) : unit =
    if Directory.Exists path
    then Directory.Delete(path, true)
    else ()

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

let private saveToJson<'a> (data: 'a) (path: string) : unit Async =
    let options = JsonSerializerSettings()
    options.Formatting <- Formatting.Indented
    let json = JsonConvert.SerializeObject(data, options)
    saveFileText json path

let saveCardImage (workspace: WorkspaceDirectory) (bytes: byte[]) (card: CardInfo) : unit Async =
    workspace.Set(card.Set).CardImage(card.Name)
    |> saveFileBytes bytes

let saveHtmlLayout (workspace: WorkspaceDirectory) (html: string) (setAbbrev: string) : unit Async =
    workspace.Set(setAbbrev).HtmlLayout
    |> saveFileText html

let saveJsonDetails (workspace: WorkspaceDirectory) (cards: CardDetails list) (setAbbrev: string) : unit Async =
    workspace.Set(setAbbrev).JsonDetails
    |> saveToJson cards

let loadJsonDetails (workspace: WorkspaceDirectory) (setAbbrev: string) : CardDetails list option Async =
    workspace.Set(setAbbrev).JsonDetails
    |> loadFromJson

let deleteSetFolder (workspace: WorkspaceDirectory) (setAbbrev: string) : unit =
    workspace.Set(setAbbrev).Path
    |> deleteFolderIfExists

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
let loadCenterFixes (workspace: WorkspaceDirectory) (setAbbrev: string) : string list =
    let path = workspace.Set(setAbbrev).CenterFixes
    match File.Exists path with
    | false -> []
    | true ->
        File.ReadAllText path
        |> parseCenterFixes

let saveCookie (workspace: WorkspaceDirectory) (cookie: KeyValuePair<string, string>) : unit Async =
    workspace.Cookie |> saveToJson cookie

let loadCredentials (workspace: WorkspaceDirectory) : Credentials option Async =
    workspace.Credentials |> loadFromJson

let loadCookie (workspace: WorkspaceDirectory) : KeyValuePair<string, string> option Async =
    workspace.Cookie |> loadFromJson