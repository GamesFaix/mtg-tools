module GamesFaix.MtgTools.Dck2Cod.Validator

open System
open GamesFaix.MtgTools.Dck2Cod.Model

let private toListString (grp: (string * DeckItem list) list) : string =
    grp
    |> List.map (fun (_, items) -> items.Head)
    |> DeckItem.toListString

let validate (deck: Deck) : string list =
    if deck.Name |> String.IsNullOrWhiteSpace then failwith "Deck name cannot be blank." else ()

    [
        let duplicates =
            deck.Cards
            |> List.groupBy DeckItem.getKey
            |> List.filter (fun (_, items) -> items.Length > 1)
        if List.any duplicates then
            $"The deck {deck.Name} has duplicate listings for {duplicates |> toListString}"

        let duplicates =
            deck.Sideboard
            |> List.groupBy DeckItem.getKey
            |> List.filter (fun (_, items) -> items.Length > 1)
        if List.any duplicates then
            $"The sideboard of {deck.Name} has duplicate listings for {duplicates |> toListString}"

        let deckWithSideboard =
            deck.Cards @ deck.Sideboard
            |> DeckItem.sumDuplicates

        let lessThan1 = deckWithSideboard |> List.filter (fun x -> x.Count < 1)
        if List.any lessThan1 then
            $"The deck {deck.Name} has less than 1 of {lessThan1 |> DeckItem.toListString}"

        let moreThan4 = deckWithSideboard |> List.filter (fun x -> x.Count > 4 && (not <| DeckItem.isBasicLand x))
        if List.any moreThan4 then
            $"The deck {deck.Name} has more than 4 of {moreThan4 |> DeckItem.toListString}"

        let count = deck.Cards |> List.sumBy (fun x -> x.Count)
        if count < 60 then
            $"The deck {deck.Name} has less than 60 cards."

        let sideboardCount = deck.Sideboard |> List.sumBy (fun x -> x.Count)
        if sideboardCount > 15 then
            $"The sideboard of {deck.Name} has more than 15 cards."
    ]