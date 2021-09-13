module GamesFaix.MtgTools.Dck2Cod.Parser

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

let parseDeck (path: string) : ShandalarDeckModel =
    let lines = File.ReadAllLines path |> List.ofArray
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

    let mutable skipLength = 1 // vNone header
    let defaultExt =
        extensionLines
        |> List.skip skipLength
        |> List.takeWhile isExtensionLine
        |> List.map parseDeckItem

    skipLength <- skipLength + defaultExt.Length + 1
    let blackExt =
        extensionLines
        |> List.skip skipLength
        |> List.takeWhile isExtensionLine
        |> List.map parseDeckItem

    skipLength <- skipLength + blackExt.Length + 1
    let blueExt =
        extensionLines
        |> List.skip skipLength
        |> List.takeWhile isExtensionLine
        |> List.map parseDeckItem

    skipLength <- skipLength + blueExt.Length + 1
    let greenExt =
        extensionLines
        |> List.skip skipLength
        |> List.takeWhile isExtensionLine
        |> List.map parseDeckItem

    skipLength <- skipLength + greenExt.Length + 1
    let redExt =
        extensionLines
        |> List.skip skipLength
        |> List.takeWhile isExtensionLine
        |> List.map parseDeckItem

    skipLength <- skipLength + redExt.Length + 1
    let whiteExt =
        extensionLines
        |> List.skip skipLength
        |> List.takeWhile isExtensionLine
        |> List.map parseDeckItem

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
