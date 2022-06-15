module GamesFaix.MtgTools.Scry.Collect

open System
open System.Linq
open System.Text
open System.Text.RegularExpressions
open GamesFaix.MtgTools.Shared
open GamesFaix.MtgTools.Shared.Context
open GamesFaix.MtgTools.Shared.Inventory

type ScryfallCard = ScryfallApi.Client.Models.Card
type ScryfallSet = ScryfallApi.Client.Models.Set
type CardWithSet = int * Card * ScryfallSet option
type ResultCard = ScryfallCard * int * Card option

let mergeSetData
    (sets: ScryfallSet list)
    (rawInventory: CardCount list) 
    : CardWithSet list =
    let index = sets.ToDictionary((fun x -> x.Code), StringComparer.InvariantCultureIgnoreCase)
    rawInventory
    |> List.map (fun (ct, c) ->
        match index.TryGetValue(c.Set) with
        | (true, set) -> (ct, c, Some set)
        | _ -> (ct, c, None)
    )

let joinResults
    (scryfallResults: ScryfallCard list)
    (inventory: CardWithSet list)
    : ResultCard list =

    Enumerable.GroupJoin(
        scryfallResults,
        inventory,
        (fun sc -> (sc.Name.ToLowerInvariant(), sc.Set.ToLowerInvariant())),
        (fun (_, c, _) -> (c.Name.ToLowerInvariant(), c.Set.ToLowerInvariant())),
        (fun sc cs -> (sc, cs))
    )
    |> Seq.collect (fun (sc, cs) -> 
        let cs = cs |> Seq.toList
        match cs with
        | [] -> [(sc, 0, None)]
        | _ -> cs |> List.map (fun (ct, c, set) -> (sc, ct, Some c))
    )
    |> Seq.toList

let formatCardOutput (result: ResultCard) : string =
    let (scryfallCard, count, card) = result
    let name = scryfallCard.Name.PadRight(30)
    let typeline = scryfallCard.TypeLine.PadRight(25)
    let cost = scryfallCard |> Scryfall.Card.getManaCost
    let count = $"(x{count})".PadLeft(5)

    let sb = StringBuilder()

    let checkbox = if card.IsNone then "[ ]" else "[x]"
    let set = scryfallCard.Set.ToUpperInvariant()
    let collectorsNo = scryfallCard.CollectorNumber.ToString().PadLeft(3, '0')

    sb.AppendLine $"{checkbox} {name} {set} (#{collectorsNo}) {typeline} {cost} {count}" |> ignore

    let str = sb.ToString()
    str.Substring(0, str.Length - 1) // Remove trailing line

let command (query: string) (ctx: WorkspaceContext<Workspace.WorkspaceDirectory>) : CommandResult =
    async {
        ctx.Log.Information "Loading set information from Scryfall..."
        let! sets = Scryfall.getSets ()
        ctx.Log.Information $"  Found {sets.Length} results"

        ctx.Log.Information $"Searching Scryfall for \"{query}\"..."
        let query = $"{query} unique:printings"
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

        ctx.Log.Information "Joining search results with inventory..."
        let results = joinResults scryfallResults inventory
        ctx.Log.Information
            (sprintf 
                "  Found %i distinct cards, and %i total cards."
                results.Length
                (results |> Seq.sumBy (fun (_, ct, _) -> ct))
            )

        ctx.Log.Information ""

        for c in results do
            ctx.Log.Information (formatCardOutput c)

        let owned = results |> Seq.filter (fun (_, _, c) -> c.IsSome) |> Seq.length
        let total = results |> Seq.length
        let percent = (float owned) / (float total) * 100.0
        let percent = percent.ToString("f2")

        ctx.Log.Information ""
        ctx.Log.Information $"{percent}%% owned ({owned} of {total})"

        return Ok ()
    }