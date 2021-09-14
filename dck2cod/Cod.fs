module GamesFaix.MtgTools.Dck2Cod.Cod

open System.Xml.Linq
open GamesFaix.MtgTools.Dck2Cod.Model

let private name = XName.Get

let private toElement (item: DeckItem) : XElement =
    XElement(name "card",
        XAttribute(name "number", item.Count),
        XAttribute(name "name", item.Name)
    )

let fromDeck (deck: Deck): XDocument =
    let children = ResizeArray()

    children.AddRange([
        XElement(name "deckname", deck.Name)
        XElement(name "comments", deck.Comments)
        XElement(name "zone",
            XAttribute(name "name", "main"),
            deck.Cards |> Seq.map toElement
        )
    ])

    if deck.Sideboard.Length > 0 then
        children.Add <|
            XElement(name "zone",
                XAttribute(name "name", "side"),
                deck.Sideboard |> Seq.map toElement
            )

    XDocument(
        XElement(name "cockatrice_deck",
            XAttribute(name "version", "1"),
            children
        )
    )
