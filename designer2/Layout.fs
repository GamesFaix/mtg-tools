module GamesFaix.MtgTools.Designer.Layout

open Model

let private getStyleTag (heightInches: float, widthInches: float) : string =
    [
        "<style>"
        "body { margin: 0; }"
        "img {"
        $"  height: {heightInches}in;"
        $"  width: {widthInches}in;"
        "  padding-right: 0.15625in;"
        "  padding-bottom: 0.53125in;"
        "}"
        "</style>"
    ] |> String.concat "\n"

let private getImageTag (card: CardInfo) : string =
    let file = FileSystem.getCardFileName card
    $"<img src=\"{file}\"/>"

let createHtmlLayout (cards : CardInfo list) : string =
    let styleTag = getStyleTag (3.46875, 2.46875)
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
