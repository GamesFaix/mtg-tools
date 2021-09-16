module GamesFaix.MtgTools.Designer.MtgDesign.Reader

open System
open System.IO
open System.Net.Http
open System.Text.RegularExpressions
open System.Xml
open System.Xml.Linq
open Sgml
open GamesFaix.MtgTools.Designer.Model

let private baseUrl = "https://mtg.design"

let private getXDoc (ctx: Context) (url: string) : XDocument Async =
    async {
        use request = new HttpRequestMessage ()
        request.RequestUri <- Uri url
        request.Method <- HttpMethod.Get
        request.Headers.Add ("Cookie", ctx.Cookie)

        let! response = ctx.Http.SendAsync request |> Async.AwaitTask
        let! stream = response.Content.ReadAsStreamAsync () |> Async.AwaitTask

        use sgmlReader = new SgmlReader ()
        sgmlReader.DocType <- "HTML"
        sgmlReader.WhitespaceHandling <- WhitespaceHandling.All
        sgmlReader.CaseFolding <- CaseFolding.ToLower
        sgmlReader.InputStream <- new StreamReader(stream)

        let doc = XDocument.Load sgmlReader
        return doc
    }

let private getCardInfosFromSetPage (setName: string) (doc: XDocument) : CardInfo list =

    let listElements =
        doc.Descendants ()
        |> Seq.filter (fun el -> el.Name.LocalName = "li")
        |> Seq.toList

    let withParagraphs =
        listElements
        |> Seq.collect (fun li ->
            li.Descendants ()
            |> Seq.filter(fun el -> el.Name.LocalName = "p")
            |> Seq.map(fun p -> li, p))

    let withLinks =
        withParagraphs
        |> Seq.map (fun (li, p) -> (li, p, p.Descendants() |> Seq.tryHead))
        |> Seq.filter (fun (li, p, maybeDesc) ->
            match maybeDesc with
            | Some child -> child.Name.LocalName = "a"
            | _ -> false)
        |> Seq.map (fun (li, p, maybeDesc) -> (li, p, maybeDesc.Value))
        |> Seq.toList

    let hrefName = XName.op_Implicit("href")

    let cards =
        withLinks
        |> Seq.map (fun (li, p, a) ->
            let url = a.Attribute(hrefName).Value
            let m = Regex.Match(url, "https://mtg.design/i/(\w+)/edit")
            {
                Id = m.Groups.[1].Value
                Name = a.Value
                Set = setName
            }
        )
        |> Seq.toList

    cards

let getSetCardInfos (ctx: Context) (setAbbrev: string) : CardInfo list Async =
    async {
        ctx.Log $"Loading list of cards in {setAbbrev}..."

        let url = $"{baseUrl}/set/{setAbbrev}"
        let! page = getXDoc ctx url
        let cards = getCardInfosFromSetPage setAbbrev page

        ctx.Log $"Found {cards.Length} cards."
        for c in cards do
            ctx.Log $"\t{c.Name}"

        return cards
    }

let private getElementById (doc: XDocument) (id: string): XElement =
    doc.Descendants()
    |> Seq.find (fun el ->
        let attr = el.Attribute (XName.Get "id")
        attr <> null
        && attr.Value = id
    )

let private getCardDetailsFromCardPage (doc: XDocument) : CardDetails =
    let getValue (id: string): string =
        let el = getElementById doc id
        let attr = el.Attribute (XName.Get "value")
        match attr with
        | null -> el.Value.Trim ()
        | _ -> attr.Value.Trim ()

    let card = {
        Id =                 ""
        Number =             getValue "card-number"
        Total =              getValue "card-total"
        Set =                getValue "card-set"
        Lang =               getValue "language"
        Designer =           getValue "designer"
        Name =               getValue "card-title"
        ManaCost =           getValue "mana-cost"
        SuperType =          getValue "super-type"
        Type =               getValue "type"
        SubType =            getValue "sub-type"
        SpecialFrames =      getValue "card-layout"
        ColorIndicator =     getValue "color-indicator"
        Rarity =             getValue "rarity"
        RulesText =          getValue "rules-text"
        FlavorText =         getValue "flavor-text"
        TextSize =           getValue "text-size"
        Center =             getValue "centered"
        Foil =               getValue "foil"
        Border =             getValue "card-border"
        ArtworkUrl =         getValue "artwork"
        CustomSetSymbolUrl = getValue "set-symbol"
        WatermarkUrl =       getValue "watermark"
        LightenWatermark =   getValue "lighten"
        Artist =             getValue "artist"
        Power =              getValue "power"
        Toughness =          getValue "toughness"
        LandOverlay =        getValue "land-overlay"
        Template =           "" // calculated in Processor
        Accent =             "" // calculated in Processor
        PlaneswalkerSize =   getValue "pw-size"
        Rules2 =             getValue "planeswalker-text-2"
        Rules3 =             getValue "planeswalker-text-3"
        Rules4 =             getValue "planeswalker-text-4"
        LoyaltyCost1 =       getValue "loyalty-ability-1"
        LoyaltyCost2 =       getValue "loyalty-ability-2"
        LoyaltyCost3 =       getValue "loyalty-ability-3"
        LoyaltyCost4 =       getValue "loyalty-ability-4"
    }
    card

let getCardDetails (ctx: Context) (cardInfo: CardInfo) : CardDetails Async =
    async {
        ctx.Log $"\tParsing details for {cardInfo.Name}..."

        let url = $"{baseUrl}/i/{cardInfo.Id}/edit"
        let! page = getXDoc ctx url
        let cardDetails = getCardDetailsFromCardPage page

        ctx.Log $"\tParsed {cardInfo.Name}."
        return { cardDetails with Id = cardInfo.Id }
    }

let getSetCardDetails (ctx: Context) (setAbbrev: string) : CardDetails list Async =
    async {
        ctx.Log "Parsing card details..."
        let! cardInfos = getSetCardInfos ctx setAbbrev

        let! cardDetails =
            cardInfos
            |> List.map (getCardDetails ctx)
            |> Async.Parallel

        ctx.Log "Card details parsed."
        return cardDetails |> List.ofArray
    }

let getCardImage (ctx: Context) (card: CardInfo) : byte[] Async =
    async {
        let url = $"{baseUrl}/i/{card.Id}.jpg"
        let! response = ctx.Http.GetAsync url |> Async.AwaitTask
        let! bytes = response.Content.ReadAsByteArrayAsync () |> Async.AwaitTask
        return bytes
    }