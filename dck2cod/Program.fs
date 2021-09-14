module GamesFaix.MtgTools.Dck2Cod.Program

open System
open System.IO

let title = """
____    ___  _  _    ___      ___  _____  ____
(  _ \  / __)( )/ )  (__ \    / __)(  _  )(  _ \
 )(_) )( (__  ) (     / _/   ( (__  )(_)(  )(_) )
(____/  \___)(_)\_)  (____)   \___)(_____)(____/ """

let sourceDir =
    "%PROGRAMFILES(x86)%/MagicTG/Decks"
    |> Environment.ExpandEnvironmentVariables
    |> Path.GetFullPath

let targetDir =
    "%USERPROFILE%/Desktop/ShandalarDecks"
    |> Environment.ExpandEnvironmentVariables
    |> Path.GetFullPath

let writeLine (x: string) = Console.WriteLine x

let processFile (file: string): string list =
    printfn $"Parsing {file}..."

    let deck =
        file
        |> FileSystem.readText
        |> Dck.parse
        |> Model.Deck.fromShandalar

    printfn $"  ({deck.Name})"

    let cod = Cod.fromDeck deck
    let targetPath = Path.Combine(targetDir, $"{deck.Name}.cod")

    printfn $"  Writing to {targetPath}..."
    FileSystem.writeCod targetPath cod

    Validator.validate deck

[<EntryPoint>]
let main _ =
    writeLine title
    let files = Directory.GetFiles sourceDir |> Seq.toList

    printfn $"Found {files.Length} deck files in {sourceDir}..."

    let issues = files |> List.collect processFile

    for issue in issues do
        writeLine issue

    printfn "Done"

    Console.Read () |> ignore
    0 // return an integer exit code
