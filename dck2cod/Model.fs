module GamesFaix.MtgTools.Dck2Cod.Model

open System

type DeckItem = {
    Name: string
    Count: int
}
with
    member this.Key =
        this.Name.ToLowerInvariant()

module DeckItem =
    let getKey (item: DeckItem) = item.Key

    let toListString (items: DeckItem list) =
        let names = items |> List.map (fun x -> x.Name)
        String.Join(", ", names)

type Deck = {
    Name: string
    Comments: string
    Cards: DeckItem list
    Sideboard: DeckItem list
}

type ShandalarDeckModel = {
    Name: string
    Core: DeckItem list
    DefaultExtension: DeckItem list
    BlackExtension: DeckItem list
    BlueExtension: DeckItem list
    GreenExtension: DeckItem list
    RedExtension: DeckItem list
    WhiteExtension: DeckItem list
}
