module GamesFaix.MtgInventorySearch.Program

open System
open System.Linq
open System.Text
open GamesFaix.MtgInventorySearch.Inventory
type ScryfallCard = ScryfallApi.Client.Models.Card

// https://scryfall.com/docs/syntax
let scryfallQuery = "t:cat c:white"

let inventoryPath = sprintf "%s/inventory1 - main.csv" (Environment.GetFolderPath(Environment.SpecialFolder.Desktop))

let joinResults (scryfallResults: ScryfallCard list) (fullInventory: Inventory.Card list) =
    Enumerable.Join(
        scryfallResults,
        fullInventory,
        (fun x -> x.Name.ToLowerInvariant()),
        (fun y -> y.Name.ToLowerInvariant()),
        (fun x y -> (x, y))
    )
    |> Seq.toList

let formatCardOutput (scryfallCard: ScryfallCard, inventoryCard: Inventory.Card) : string =
    let sb = StringBuilder()
    sb.AppendLine $"{scryfallCard.Name}" |> ignore
    sb.AppendLine $"  {scryfallCard.ManaCost} {scryfallCard.TypeLine}" |> ignore

    for e in inventoryCard.Editions do
        sb.AppendLine $"  {e.Count} {e.Set}" |> ignore

    sb.Remove(sb.Length-1, 1) |> ignore // remove last newline
    sb.ToString()

[<EntryPoint>]
let main _ =
    async {
        printfn $"Searching Scryfall for \"{scryfallQuery}\"..."
        let! scryfallResults = Scryfall.search scryfallQuery
        printfn $"  Found {scryfallResults.Length} results"

        printfn "Loading inventory..."
        let fullInventory = Inventory.load inventoryPath
        printfn "  Found %i distinct cards, %i editions, and %i total cards."
            fullInventory.Length
            (fullInventory |> Seq.sumBy (fun c -> c.Editions.Length))
            (fullInventory |> Seq.sumBy (fun c -> c.Count))

        printfn "Joining search results with inventory..."
        let joined = joinResults scryfallResults fullInventory
        printfn "  Found %i distinct cards, %i editions, and %i total cards."
            joined.Length
            (joined |> Seq.sumBy (fun (_, c) -> c.Editions.Length))
            (joined |> Seq.sumBy (fun (_, c) -> c.Count))

        printfn ""

        for c in joined do
            printfn "%s" (formatCardOutput c)

        Console.Read() |> ignore
        return 0 // return an integer exit code
    } |> Async.RunSynchronously

