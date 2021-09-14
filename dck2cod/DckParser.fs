module GamesFaix.MtgTools.Dck2Cod.DckParser

open System
open System.IO
open System.Text.RegularExpressions
open GamesFaix.MtgTools.Dck2Cod.Model

let private parseName (line: string) : string =
    let pattern = @"([\w ]+).*"
    match Regex.Match(line, pattern) with
    | m when m.Success ->
        m.Groups.[1].Captures.[0].Value.Trim()
    | _ -> raise <| FormatException()

let private parseDeckItem (line: string) : DeckItem =
    let pattern = @"\.\d+\s+(\d+)\s+(.*)"
    match Regex.Match(line, pattern) with
    | m when m.Success ->
        {
            Name = m.Groups.[2].Captures.[0].Value.Trim()
            Count = Int32.Parse(m.Groups.[1].Captures.[0].Value)
        }
    | _ -> raise <| FormatException()

let private parseDeckInner (lines : string list) =
    let titleLine = lines.[0]

    let core =
        lines
        |> List.skip 2 // Title and blank line
        |> List.takeWhile (not << String.IsNullOrWhiteSpace)
        |> List.map parseDeckItem

    let extensionLines =
        lines
        |> List.skip (2 + core.Length + 1) // Title, blank, core, blank
        |> List.takeWhile (not << String.IsNullOrWhiteSpace) // Avoid trailing blank line

    let isExtensionLine line = Regex.IsMatch(line, "\\d")

    let takeLines length =
        extensionLines
        |> List.skip length
        |> List.takeWhile isExtensionLine
        |> List.map parseDeckItem

    let mutable skipLength = 1 // vNone header
    let defaultExt = takeLines skipLength

    skipLength <- skipLength + defaultExt.Length + 1
    let blackExt = takeLines skipLength

    skipLength <- skipLength + blackExt.Length + 1
    let blueExt = takeLines skipLength

    skipLength <- skipLength + blueExt.Length + 1
    let greenExt = takeLines skipLength

    skipLength <- skipLength + greenExt.Length + 1
    let redExt = takeLines skipLength

    skipLength <- skipLength + redExt.Length + 1
    let whiteExt = takeLines skipLength

    {
        Name = parseName titleLine
        Core = core
        DefaultExtension = defaultExt
        BlackExtension = blackExt
        BlueExtension = blueExt
        GreenExtension = greenExt
        RedExtension = redExt
        WhiteExtension = whiteExt
    }

let parseDeck (path: string) : ShandalarDeck =
    File.ReadAllLines path
    |> List.ofArray
    |> parseDeckInner