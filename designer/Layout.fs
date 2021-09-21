module GamesFaix.MtgTools.Designer.Layout

open Model
open Workspace

// 1/64th less than 3.5 x 2.5
let private cardHeightInches = 3.46875
let private cardWidthInches = 2.46875

let private styleTag =
    [
        "<style>"
        "  body { margin: 0; }"
        "  img {"
        $"    height: {cardHeightInches}in;"
        $"    width: {cardWidthInches}in;"
        "    padding-right: 0.15625in;"
        "    padding-bottom: 0.53125in;"
        "  }"
        "</style>"
    ] |> String.concat "\n"

let private getImageTag (card: CardInfo) : string =
    let file = SetDirectory.getCardFileName card.Name
    $"<img src=\"{file}\"/>"

let createHtmlLayout (cards: CardInfo list) : string =
    let cardTags = cards |> List.map getImageTag

    [
        "<html>"
        "<head>"
        styleTag
        "</head>"
        "<body>"
        cardTags |> String.concat ""
        "</body>"
        "</html>"
    ] |> String.concat "\n"
