module GamesFaix.MtgTools.Cockatrize.Model

open System
open System.Linq
open System.Text

type DeckItem = {
    Name: string
    Count: int
}
with
    member this.Key =
        this.Name.ToLowerInvariant()

type Deck = {
    Name: string
    Comments: string
    Cards: DeckItem list
    Sideboard: DeckItem list
}

module DeckItem =
    let getKey (item: DeckItem) = item.Name.ToLowerInvariant()

    let private basicLandNames = [
        "plains"
        "island"
        "swamp"
        "mountain"
        "forest"
        "snow-covered plains"
        "snow-covered island"
        "snow-covered swamp"
        "snow-covered mountain"
        "snow-covered forest"
        "wastes"
    ]
    let isBasicLand (item: DeckItem) : bool =
        basicLandNames |> List.contains item.Key

    let private astralCardNames = [
        "aswan jaguar"
        "call from the grave"
        "faerie dragon"
        "gem bazaar"
        "goblin polka band"
        "necropolis of azar"
        "orcish catapult"
        "pandora's box"
        "power struggle"
        "prismatic dragon"
        "rainbow knights"
        "whimsy"
    ]
    let isAstral (item: DeckItem) : bool =
        astralCardNames |> List.contains item.Key

    let private anteCardNames = [
        "amulet of quoz"
        "bronze tablet"
        "contract from below"
        "darkpact"
        "demonic attorney"
        "jeweled bird"
        "rebirth"
        "tempest efreet"
        "timmerian fiends"
    ]
    let isAnte (item: DeckItem) : bool =
        anteCardNames |> List.contains item.Key

    // --- Set/List operations ---

    let toListString (items: DeckItem list) =
        let names = items |> List.map (fun x -> x.Name)
        String.Join(", ", names)

    let private consolidateDuplicates
        (aggregate: (DeckItem -> int) -> DeckItem list -> int)
        (items: DeckItem list)
        : DeckItem list =

        items
        |> List.groupBy getKey
        |> List.map (fun (_, items) -> {
            Name = items.Head.Name
            Count = items |> aggregate (fun item -> item.Count)
        })

    let sumDuplicates = consolidateDuplicates List.sumBy
    let unionDuplicates = consolidateDuplicates (fun getKey items -> (items |> List.maxBy getKey).Count)

    let subtract (toSubtract: DeckItem list) (items: DeckItem list) =
        let toSubtractCache = toSubtract.ToDictionary (fun x -> x.Key)

        items
        |> List.map (fun item ->
            let count =
                match toSubtractCache.TryGetValue item.Key with
                | (true, toSubtractItem) -> item.Count - toSubtractItem.Count
                | _ -> item.Count

            {
                Name = item.Name
                Count = count
            }
        )
        |> List.filter (fun x -> x.Count > 0)

module Deck =
    let private shandalarTypos : Map<string, string> =
        [
            "will-o-the-wisp", "Will-o'-the-Wisp"
            "manaflare",       "Mana Flare"
            "Yotian Soldiers", "Yotian Soldier"
        ]
        |> Map.ofList

    let private fixShandalarTypos (items: DeckItem list) : DeckItem list =
        items
        |> List.map (fun x ->
            match shandalarTypos |> Map.tryFind x.Name with
            | Some rightName -> { x with Name = rightName }
            | _ -> x
        )

    let private getComments (cards: DeckItem list): string =
        let sb = StringBuilder()

        let astralCards = cards |> List.filter DeckItem.isAstral
        if List.any astralCards then
            let cardList = astralCards |> DeckItem.toListString
            sb.Append $"Contains these Astral cards: {cardList}. " |> ignore

        let anteCards = cards |> List.filter DeckItem.isAnte
        if List.any anteCards then
            let cardList = anteCards |> DeckItem.toListString
            sb.Append $"Contains these ante cards: {cardList}. " |> ignore
        sb.ToString()

    let fromDck (dck: Dck.DckDeck) : Deck =
        let toDeckItem (dckCard: Dck.DckCard) : DeckItem =
            {
                Name = dckCard.Name
                Count = dckCard.Count
            }

        let cards = dck.Cards |> List.map toDeckItem

        let defaultExtension =
            dck.Extensions
            |> List.tryFind (fun e -> e.Name = "None")
            |> Option.map (fun e -> e.Cards)
            |> Option.defaultValue []
            |> List.map toDeckItem

        let colorExtensions =
            dck.Extensions
            |> List.filter (fun e -> e.Name <> "None")
            |> List.collect (fun e -> e.Cards)
            |> List.map toDeckItem

        let cards =
            cards @ defaultExtension
            |> DeckItem.unionDuplicates
            |> fixShandalarTypos

        let sideboard =
            colorExtensions
            |> DeckItem.unionDuplicates
            |> DeckItem.subtract defaultExtension
            |> fixShandalarTypos

        let comments = getComments (cards @ sideboard)

        {
            Name = dck.Name
            Comments = comments
            Cards = cards
            Sideboard = sideboard
        }