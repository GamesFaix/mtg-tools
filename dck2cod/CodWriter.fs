module GamesFaix.MtgTools.Dck2Cod.CodWriter

open System.IO
open System.Xml.Linq
open GamesFaix.MtgTools.Dck2Cod.Model

let private name = XName.Get

let private toElement (item: DeckItem) : XElement =
    XElement(name "card",
        XAttribute(name "number", item.Count),
        XAttribute(name "name", item.Name)
    )

let private toDocument (deck: Deck): XDocument =
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

let private createDirIfMissing (path: string) =
    let dir = Path.GetDirectoryName path
    match Directory.Exists dir with
    | false -> Directory.CreateDirectory dir |> ignore
    | _ -> ()

let writeDeck (path: string) (deck: Deck) : unit =
    let doc = deck |> toDocument
    createDirIfMissing path
    use stream = File.Open(path, FileMode.Create)
    doc.Save stream
