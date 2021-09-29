module GamesFaix.MtgTools.Scry.Query

open System
open System.Linq
open System.Text
open System.Text.RegularExpressions
open GamesFaix.MtgTools.Scry.Inventory
open GamesFaix.MtgTools.Shared
open GamesFaix.MtgTools.Shared.Context

type ScryfallCard = ScryfallApi.Client.Models.Card
type ScryfallSet = ScryfallApi.Client.Models.Set

let mergeSetData
    (sets: ScryfallSet list)
    (rawInventory: Inventory.CardEdition list) =
    let index = sets.ToDictionary((fun x -> x.Code), StringComparer.InvariantCultureIgnoreCase)
    rawInventory
    |> List.map (fun x ->
        match index.TryGetValue(x.Set) with
        | (true, set) -> (x, Some set)
        | _ -> (x, None)
    )

type InventoryCard = string * CardEdition list

let groupEditions (inventoryWithSets: (Inventory.CardEdition * ScryfallSet option) list) : InventoryCard list =
    inventoryWithSets
    |> List.groupBy (fun (edition, set) -> edition.Name)
    |> List.map (fun (name, editions) ->
        let editions =
            editions
            |> List.sortBy (fun (_, set) ->
                set
                |> Option.bind (fun x -> x.ReleaseDate |> Option.ofNullable)
                |> Option.defaultValue DateTime.MaxValue)
            |> List.map (fun (edition, set) ->
                match set with
                | Some s -> { edition with Set = $"{s.Name} ({s.Code.ToUpperInvariant()})" }
                | None -> edition
            )
        (name, editions)
    )

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

let command (query: string) (inventoryPath: string) (ctx: IContext) : CommandResult =
    async {
        ctx.Log.Information "Loading set information from Scryfall..."
        let! sets = Scryfall.getSets ()
        ctx.Log.Information $"  Found {sets.Length} results"

        ctx.Log.Information $"Searching Scryfall for \"{query}\"..."
        let! scryfallResults = Scryfall.search query
        ctx.Log.Information $"  Found {scryfallResults.Length} results"

        ctx.Log.Information $"Loading inventory from {inventoryPath}..."
        let rawInventory = Inventory.load inventoryPath
        ctx.Log.Information 
            (sprintf
                "  Found %i editions, and %i total cards."
                rawInventory.Length
                (rawInventory |> Seq.sumBy (fun c -> c.Count))
            )
        let inventory =
            rawInventory
            |> mergeSetData sets
            |> groupEditions

        ctx.Log.Information "Joining search results with inventory..."
        let joined = joinResults scryfallResults inventory
        ctx.Log.Information
            (sprintf 
                "  Found %i distinct cards, %i editions, and %i total cards."
                joined.Length
                (joined |> Seq.sumBy (fun (_, editions) -> editions.Length))
                (joined |> Seq.sumBy (fun (_, editions) -> editions |> Seq.sumBy (fun e -> e.Count)))
            )

        ctx.Log.Information ""

        for c in joined do
            ctx.Log.Information (formatCardOutput c)

        return Ok ()
    }