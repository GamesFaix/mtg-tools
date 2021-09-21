module GamesFaix.MtgTools.Designer.Layout

open System.Xml.Linq
open Model
open Workspace

// 1/64th less than 3.5 x 2.5
let private cardHeightInches = 3.46875
let private cardWidthInches = 2.46875

let private style =
    [
        "body { margin: 0; }"
        "img {"
        $"  height: {cardHeightInches}in;"
        $"  width: {cardWidthInches}in;"
        "  padding-right: 0.15625in;"
        "  padding-bottom: 0.53125in;"
        "}"
    ] |> String.concat "\n"

let private name = XName.Get

let private getImageTag (card: CardInfo) =
    let file = SetDirectory.getCardFileName card.Name
    XElement(name "img",
        XAttribute(name "src", file)
    )

let createDoc (cards: CardInfo list) =
   XDocument(
        XElement(name "html",
            XElement(name "head",
                XElement(name "style",
                    style
                )
            ),
            XElement(name "body",
                cards |> List.map getImageTag
            )
        )
    )

let createHtmlLayout (cards: CardInfo list) : string =
    let doc = createDoc cards
    doc.ToString()
