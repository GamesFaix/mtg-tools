module GamesFaix.MtgInventorySearch.Program

open System
open System.Collections.Generic
open System.Linq
open System.Text
open Microsoft.Extensions.Configuration
open GamesFaix.MtgInventorySearch.Inventory
open System.Text.RegularExpressions
type ScryfallCard = ScryfallApi.Client.Models.Card
type ScryfallSet = ScryfallApi.Client.Models.Set

type Settings = {
    Query: string // https://scryfall.com/docs/syntax
    InventoryPath: string
}

let configure args =
    let argMap = Dictionary<string, string>()
    argMap.Add("-i", "InventoryPath")
    argMap.Add("--inventory", "InventoryPath")
    argMap.Add("-q", "Query")
    argMap.Add("--query", "Query")

    let config =
        ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional = false, reloadOnChange = false)
            .AddCommandLine(args, argMap)
            .Build()

    let settings = {
        Query = config.["Query"]
        InventoryPath = Environment.ExpandEnvironmentVariables(config.["InventoryPath"])
    }

    if String.IsNullOrWhiteSpace(settings.InventoryPath) then failwith $"{nameof(settings.InventoryPath)} cannot be blank"
    if String.IsNullOrWhiteSpace(settings.Query) then failwith $"{nameof(settings.Query)} cannot be blank"

    settings

let mergeSetData
    (sets: ScryfallSet list)
    (rawInventory: Inventory.CardEdition list) =
    let index = sets.ToDictionary((fun x -> x.Code), StringComparer.InvariantCultureIgnoreCase)
    rawInventory
    |> List.map (fun x ->
        match index.TryGetValue(x.Set) with
        | (true, set) -> { x with Set = $"{set.Name} ({set.Code.ToUpperInvariant()})" }
        | _ -> x
    )

type InventoryCard = string * CardEdition list

let groupEditions (rawInventory: Inventory.CardEdition list) : InventoryCard list =
    rawInventory
    |> List.groupBy (fun x -> x.Name)

let joinResults
    (scryfallResults: ScryfallCard list)
    (inventory: InventoryCard list)
    : (ScryfallCard * list<CardEdition>) list =
    Enumerable.Join(
        scryfallResults,
        inventory,
        (fun sc -> sc.Name.ToLowerInvariant()),
        (fun (name, _) -> name.ToLowerInvariant()),
        (fun sc (_, editions) -> (sc, editions))
    )
    |> Seq.toList

let formatCardOutput (scryfallCard: ScryfallCard, inventoryEditions: CardEdition list) : string =
    let name = scryfallCard.Name.PadRight(30)
    let typeline = scryfallCard.TypeLine.PadRight(25)
    let cost =
        Regex.Replace(scryfallCard.ManaCost, "{(\\d|W|U|B|R|G)}", "$1")
             .PadLeft(6)
    let count = inventoryEditions |> Seq.sumBy (fun x -> x.Count)
    let count = $"(x{count})".PadLeft(5)

    let sb = StringBuilder()
    sb.AppendLine $"{name} {typeline} {cost} {count}" |> ignore

    for e in inventoryEditions do
        let set = e.Set
        let count = $"(x{e.Count})".PadLeft(5)
        sb.AppendLine $"  {count} {set}" |> ignore

    sb.ToString()

[<EntryPoint>]
let main args =
    async {
        let settings = configure args

        printfn "Loading set information from Scryfall..."
        let! sets = Scryfall.getSets ()
        printfn $"  Found {sets.Length} results"

        printfn $"Searching Scryfall for \"{settings.Query}\"..."
        let! scryfallResults = Scryfall.search settings.Query
        printfn $"  Found {scryfallResults.Length} results"

        printfn $"Loading inventory from {settings.InventoryPath}..."
        let rawInventory = Inventory.load settings.InventoryPath
        printfn "  Found %i editions, and %i total cards."
            rawInventory.Length
            (rawInventory |> Seq.sumBy (fun c -> c.Count))

        let inventory =
            rawInventory
            |> mergeSetData sets
            |> groupEditions

        printfn "Joining search results with inventory..."
        let joined = joinResults scryfallResults inventory
        printfn "  Found %i distinct cards, %i editions, and %i total cards."
            joined.Length
            (joined |> Seq.sumBy (fun (_, editions) -> editions.Length))
            (joined |> Seq.sumBy (fun (_, editions) -> editions |> Seq.sumBy (fun e -> e.Count)))

        printfn ""

        for c in joined do
            printfn "%s" (formatCardOutput c)

        return 0 // return an integer exit code
    } |> Async.RunSynchronously

