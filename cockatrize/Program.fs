module GamesFaix.MtgTools.Cockatrize.Program

open System
open System.IO
open GamesFaix.MtgTools.Shared

let title ="""Cockatrize""".Trim()

let sourceDir =
    "%PROGRAMFILES(x86)%/MagicTG/Decks"
    |> Environment.ExpandEnvironmentVariables
    |> Path.GetFullPath

let targetDir =
    "%USERPROFILE%/Desktop/ShandalarDecks"
    |> Environment.ExpandEnvironmentVariables
    |> Path.GetFullPath

let writeLine (x: string) = Console.WriteLine x

let processFile (file: string): string list Async = async {
    printfn $"Parsing {file}..."

    let! text = FileSystem.loadText file 

    let deck =
        text
        |> Dck.parse
        |> Model.Deck.fromDck

    let cod = Cod.fromDeck deck
    let targetPath = Path.Combine(targetDir, $"{deck.Name}.cod")

    printfn $"  Writing to {targetPath}..."
    do! FileSystem.saveFileText (cod.ToString()) targetPath

    return Validator.validate deck
}

[<EntryPoint>]
let main _ = 
    async {
        writeLine title
        writeLine ""

        let files = Directory.GetFiles sourceDir |> Seq.toList

        printfn $"Found {files.Length} deck files in {sourceDir}..."

        let! issues = files |> List.collectAsync processFile

        printfn ""
        printfn "VALIDATION ISSUES:"
        for issue in issues do
            writeLine issue

        printfn "Done"

        Console.Read () |> ignore
        return 0
    } 
    |> Async.RunSynchronously