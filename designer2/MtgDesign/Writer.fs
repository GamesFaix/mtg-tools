module GamesFaix.MtgTools.Designer.MtgDesign.Writer

open System
open System.Net
open System.Net.Http
open System.Web
open GamesFaix.MtgTools.Designer.Model

type SaveMode = Create | Edit

let private baseUrl = "https://mtg.design"

let private buildRenderQuery (mode: SaveMode) (card: CardDetails) : string =
    let query = HttpUtility.ParseQueryString ""
    query.Add("card-number", card.Number)
    query.Add("card-total", card.Total)
    query.Add("card-set", card.Set)
    query.Add("language", card.Lang)
    query.Add("card-title", card.Name)
    query.Add("mana-cost", card.ManaCost)
    if not <| String.IsNullOrEmpty card.SuperType then query.Add("super-type", card.SuperType) else ()
    if not <| String.IsNullOrEmpty card.SubType then query.Add("sub-type", card.SubType) else ()
    if not <| String.IsNullOrEmpty card.Center then query.Add("centered", "true") else ()
    query.Add("type", card.Type)
    query.Add("text-size", card.TextSize)
    query.Add("rarity", card.Rarity)
    query.Add("artist", card.Artist)
    query.Add("power", card.Power)
    query.Add("toughness", card.Toughness)
    if not <| String.IsNullOrEmpty card.ArtworkUrl then query.Add("artwork", card.ArtworkUrl) else ()
    query.Add("designer", card.Designer)
    query.Add("card-border", card.Border)
    query.Add("watermark", card.WatermarkUrl)
    query.Add("card-layout", card.SpecialFrames)
    query.Add("set-symbol", card.CustomSetSymbolUrl)
    query.Add("rules-text", card.RulesText)
    query.Add("flavor-text", card.FlavorText)
    query.Add("card-template", card.Template)
    query.Add("card-accent", card.Accent)
    if card.SpecialFrames = "token" || card.Type.Contains "Land"
    then query.Add("land-overlay", card.LandOverlay) else ()
    query.Add("stars", "0") // ???
    query.Add("edit", if mode = SaveMode.Create then "false" else card.Id)
    if not <| String.IsNullOrEmpty card.ColorIndicator then query.Add("color-indicator", card.ColorIndicator) else ()
    if not <| String.IsNullOrEmpty card.PlaneswalkerSize then query.Add("pw-size", card.PlaneswalkerSize) else ()
    if not <| String.IsNullOrEmpty card.Rules2 then query.Add("pw-text2", card.Rules2) else ()
    if not <| String.IsNullOrEmpty card.Rules3 then query.Add("pw-text3", card.Rules3) else ()
    if not <| String.IsNullOrEmpty card.Rules4 then query.Add("pw-text4", card.Rules4) else ()
    // Not yet supporting loyalty costs for planeswalkers

    query.ToString()

let private renderCard (ctx: Context) (mode: SaveMode) (card: CardDetails) : unit Async =
    async {
        let query = buildRenderQuery mode card
        let url =
            $"{baseUrl}/render?{query}"
                .Replace("+", "%20")
                .Replace("%26rsquo%3b", "%E2%80%99")
                .Replace("%26rsquo%253", "%E2%80%99")

        use request = new HttpRequestMessage()
        request.RequestUri <- Uri url
        request.Method <- HttpMethod.Get
        request.Headers.Add ("Cookie", ctx.Cookie)

        let! response = ctx.Http.SendAsync request |> Async.AwaitTask
        if response.StatusCode >= HttpStatusCode.BadRequest
        then failwith "render error" else ()

        return ()
    }

let private shareCard (ctx: Context) (mode: SaveMode) (card: CardDetails) : unit Async =
    async {
        let query = HttpUtility.ParseQueryString ""
        query.Add("edit", if mode = SaveMode.Create then "false" else card.Id)
        query.Add("name", card.Name)

        let url = $"{baseUrl}/shared?{query.ToString()}"

        use request = new HttpRequestMessage ()
        request.RequestUri <- Uri url
        request.Method <- HttpMethod.Get
        request.Headers.Add("Cookie", ctx.Cookie)

        let! response = ctx.Http.SendAsync request |> Async.AwaitTask
        if response.StatusCode >= HttpStatusCode.BadRequest
        then failwith "share error" else ()

        return ()
    }

let saveCard (ctx: Context) (mode: SaveMode) (card: CardDetails) : unit Async =
    async {
        ctx.Log $"\tRendering ({card.Number}/{card.Total}) {card.Name}..."
        let! _ = renderCard ctx mode card
        ctx.Log $"\tSharing ({card.Number}/{card.Total}) {card.Name}..."
        let! _ = shareCard ctx mode card
        ctx.Log $"\tFinished ({card.Number}/{card.Total}) {card.Name}."
        return ()
    }

let saveCards (ctx: Context) (mode: SaveMode) (cards: CardDetails list) : unit Async =
    ctx.Log "Saving cards..."
    cards
    |> List.map (saveCard ctx mode)
    |> Async.Sequential
    |> Async.Ignore

let deleteCard (ctx: Context) (card: CardInfo) : unit Async =
    async {
        ctx.Log $"\tDeleting {card.Set} - {card.Name}..."

        let url = $"{baseUrl}/set/{card.Set}/i/{card.Id}/delete"
        let! response = ctx.Http.GetAsync url |> Async.AwaitTask
        if response.StatusCode >= HttpStatusCode.BadRequest
        then failwith "delete error" else ()

        ctx.Log $"\tDeleted {card.Set} - {card.Name}."
        return ()
    }

let deleteCards (ctx: Context) (cards: CardInfo list) : unit Async =
    ctx.Log "Deleting cards..."
    cards
    |> List.map (deleteCard ctx)
    |> Async.Parallel
    |> Async.Ignore
