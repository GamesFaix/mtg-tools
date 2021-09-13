module GamesFaix.MtgTools.Dck2Cod.Program

open System
open System.IO

let sourceDir =
    Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
        "MagicTG/Decks")

let targetDir =
    Path.Combine(
        Directory.GetCurrentDirectory(),
        "../../../../../Microprose")
    |> Path.GetFullPath

[<EntryPoint>]
let main _ =
    let allIssues = ResizeArray()
    let allFiles = Directory.GetFiles sourceDir

    for f in allFiles do
        Console.WriteLine($"Processing {f}")
        let deck = Parser.parseDeck f |> Mapper.mapDeck
        let issues = Validator.validate deck
        allIssues.AddRange issues

        let targetPath = Path.Combine(targetDir, $"{deck.Name}.cod")
        Writer.writeDeck targetPath deck

    for issue in allIssues do
        Console.WriteLine issue

    Console.WriteLine "Done"
    Console.Read () |> ignore
    0 // return an integer exit code
