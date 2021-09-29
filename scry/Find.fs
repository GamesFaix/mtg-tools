module GamesFaix.MtgTools.Scry.Find

open System
open System.Linq
open System.Text
open System.Text.RegularExpressions
open GamesFaix.MtgTools.Shared
open GamesFaix.MtgTools.Shared.Context
open GamesFaix.MtgTools.Shared.Inventory

type ScryfallCard = ScryfallApi.Client.Models.Card
type ScryfallSet = ScryfallApi.Client.Models.Set

let mergeSetData
    (sets: ScryfallSet list)
    (rawInventory: CardCount list) =
    let index = sets.ToDictionary((fun x -> x.Code), StringComparer.InvariantCultureIgnoreCase)
    rawInventory
    |> List.map (fun (ct, c) ->
        match index.TryGetValue(c.Set) with
        | (true, set) -> (ct, c, Some set)
        | _ -> (ct, c, None)
    )

type InventoryCard = string * CardCount list

let groupEditions (inventoryWithSets: (int * Card * ScryfallSet option) list) : InventoryCard list =
    inventoryWithSets
    |> List.groupBy (fun (ct, c, set) -> c.Name)
    |> List.map (fun (name, printings) ->
        let printings =
            printings
            |> List.sortBy (fun (_, _, set) ->
                set
                |> Option.bind (fun x -> x.ReleaseDate |> Option.ofNullable)
                |> Option.defaultValue DateTime.MaxValue)
            |> List.map (fun (ct, c, set) ->
                match set with
                | Some s -> (ct, { c with Set = $"{s.Name} ({s.Code.ToUpperInvariant()})" })
                | None -> (ct, c)
            )
        (name, printings)
    )

let joinResults
    (scryfallResults: ScryfallCard list)
    (inventory: InventoryCard list)
    : (ScryfallCard * list<CardCount>) list =
    Enumerable.Join(
        scryfallResults,
        inventory,
        (fun sc -> sc.Name.ToLowerInvariant()),
        (fun (name, _) -> name.ToLowerInvariant()),
        (fun sc (_, editions) -> (sc, editions))
    )
    |> Seq.toList

let formatCardOutput (scryfallCard: ScryfallCard, inventoryEditions: CardCount list) : string =
    let name = scryfallCard.Name.PadRight(30)
    let typeline = scryfallCard.TypeLine.PadRight(25)
    let cost =
        Regex.Replace(scryfallCard.ManaCost, "{(\\d|W|U|B|R|G)}", "$1")
             .PadLeft(6)
    let count = inventoryEditions |> Seq.sumBy fst
    let count = $"(x{count})".PadLeft(5)

    let sb = StringBuilder()
    sb.AppendLine $"{name} {typeline} {cost} {count}" |> ignore

    for (ct, c) in inventoryEditions do
        let set = c.Set
        let count = $"(x{ct})".PadLeft(5)
        sb.AppendLine $"  {count} {set}" |> ignore

    sb.ToString()

let command (query: string) (ctx: WorkspaceContext<Workspace.WorkspaceDirectory>) : CommandResult =
    async {
        ctx.Log.Information "Loading set information from Scryfall..."
        let! sets = Scryfall.getSets ()
        ctx.Log.Information $"  Found {sets.Length} results"

        ctx.Log.Information $"Searching Scryfall for \"{query}\"..."
        let! scryfallResults = Scryfall.search query
        ctx.Log.Information $"  Found {scryfallResults.Length} results"

        ctx.Log.Information $"Loading inventory from {ctx.Workspace.Path}..."
        let! rawInventory = Inventory.loadInventoryFile ctx.Workspace.Cards
        ctx.Log.Information 
            (sprintf
                "  Found %i editions, and %i total cards."
                rawInventory.Length
                (rawInventory |> Seq.sumBy fst)
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
                (joined |> Seq.sumBy (fun (_, editions) -> editions |> Seq.sumBy fst))
            )

        ctx.Log.Information ""

        for c in joined do
            ctx.Log.Information (formatCardOutput c)

        return Ok ()
    }