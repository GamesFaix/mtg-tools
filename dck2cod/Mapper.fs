module GamesFaix.MtgTools.Dck2Cod.Mapper

open System.Linq
open GamesFaix.MtgTools.Dck2Cod.Model

let consolidateDuplicates (aggregate: (DeckItem -> int) -> DeckItem list -> int) (items: DeckItem list) : DeckItem list =
    items
    |> List.groupBy DeckItem.getKey
    |> List.map (fun (key, items) -> {
        Name = key
        Count = items |> aggregate (fun item -> item.Count)
    })

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

let fixBugs (items: DeckItem list) : DeckItem list =
    items
    |> List.map (fun x ->
        match x.Key with
        | "will-o-the-wisp" -> { x with Name = "Will-o'-the-Wisp" }
        | "manaflare" -> { x with Name = "Mana Flare" }
        | _ -> x
    )

let mapDeck (shandalarDeck: ShandalarDeckModel) : Deck =
    let cards =
        shandalarDeck.Core
        @ shandalarDeck.DefaultExtension
        |> consolidateDuplicates List.sumBy // Sum count of cards in core and default extension
        |> fixBugs

    let sideboard =
        shandalarDeck.BlackExtension
        @ shandalarDeck.BlueExtension
        @ shandalarDeck.GreenExtension
        @ shandalarDeck.RedExtension
        @ shandalarDeck.WhiteExtension
        |> consolidateDuplicates (fun getKey items -> (items |> List.maxBy getKey).Count) // Take max count of each card used in any color extension
        |> subtract shandalarDeck.DefaultExtension // Subtract count from default extension
        |> fixBugs


    let mutable comments = ""

    let astralCards = cards @ sideboard |> List.filter CardInfo.isAstral

    if astralCards |> List.isEmpty |> not then
        let cardList = astralCards |> DeckItem.toListString
        comments <- comments + $"Contains these Astral cards: {cardList}. "

    let anteCards = cards @ sideboard |> List.filter CardInfo.isAnte

    if anteCards |> List.isEmpty |> not then
        let cardList = anteCards |> DeckItem.toListString
        comments <- comments + $"Contains these ante cards: {cardList}. "

    {
        Name = shandalarDeck.Name
        Comments = comments
        Cards = cards
        Sideboard = sideboard
    }
