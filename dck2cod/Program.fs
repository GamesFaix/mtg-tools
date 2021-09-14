module GamesFaix.MtgTools.Dck2Cod.Program

open System
open System.IO

let sourceDir =
    "%PROGRAMFILES(x86)%/MagicTG/Decks"
    |> Environment.ExpandEnvironmentVariables
    |> Path.GetFullPath

let targetDir =
    "%USERPROFILE%/Desktop/ShandalarDecks"
    |> Environment.ExpandEnvironmentVariables
    |> Path.GetFullPath

[<EntryPoint>]
let main _ =
    printfn """
____    ___  _  _    ___      ___  _____  ____
(  _ \  / __)( )/ )  (__ \    / __)(  _  )(  _ \
    )(_) )( (__  )  (    / _/   ( (__  )(_)(  )(_) )
(____/  \___)(_)\_)  (____)   \___)(_____)(____/ """

    let files = Directory.GetFiles sourceDir |> Seq.toList

    printfn $"Found {files.Length} deck files in {sourceDir}..."

    let issues =
        files
        |> List.collect (fun f ->
            printfn $"Processing {f}..."
            let deck = DckParser.parseDeck f |> Model.Deck.fromShandalar
            printfn $"  ({deck.Name})"
            let issues = Validator.validate deck
            let targetPath = Path.Combine(targetDir, $"{deck.Name}.cod")
            CodWriter.writeDeck targetPath deck
            issues
        )

    for issue in issues do
        printfn "%s" issue

    printfn "Done"

    Console.Read () |> ignore
    0 // return an integer exit code
