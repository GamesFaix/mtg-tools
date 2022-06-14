module GamesFaix.MtgTools.Designer.CardProcessor

open System
open System.Linq
open Serilog
open GamesFaix.MtgTools.Shared
open Context
open Model

let getColors (card: CardDetails) : char list =
    card.ManaCost.Intersect ['W';'U';'B';'R';'G'] |> Seq.toList

let private getCollectorNumberGroup (card: CardDetails) : CollectorNumberGroup =
    if card.SuperType.ToLower().Contains "token" then CollectorNumberGroup.Token
    elif card.Type.ToLower().Contains "land" then CollectorNumberGroup.Land
    else
        let colors =
            match getColors card with
            | [] -> card.ColorIndicator.ToCharArray() |> Seq.toList
            | x -> x

        match colors with
        | [] -> 
            if card.Type.Contains "Artifact" then 
                CollectorNumberGroup.Artifact
            else CollectorNumberGroup.Colorless
        | ['W'] -> CollectorNumberGroup.White
        | ['U'] -> CollectorNumberGroup.Blue
        | ['B'] -> CollectorNumberGroup.Black
        | ['R'] -> CollectorNumberGroup.Red
        | ['G'] -> CollectorNumberGroup.Green
        | _ -> CollectorNumberGroup.Multi

let private generateNumbers (cards: CardDetails seq) : (int * CardDetails) seq =
    cards
    |> Seq.groupBy getCollectorNumberGroup
    |> Seq.sortBy (fun (grp, _) -> grp)
    |> Seq.collect (fun (_, cs) -> cs |> Seq.sortBy (fun c -> c.Name))
    |> Seq.indexed
    |> Seq.map (fun (n, c) -> (n+1, c))

let private generateAndApplyNumbers (cards: CardDetails list) : CardDetails list =
    let count = cards.Length
    generateNumbers cards
    |> Seq.map (fun (n, c) ->
        { c with
            Number = n.ToString().PadLeft(count.ToString().Length, '0')
            Total = count.ToString()
        })
    |> Seq.toList

let private getCardTemplate (card: CardDetails) : string =
    let colors = getColors card
    match colors.Length with
    | 0 -> "C"
    | 1 -> colors.Head.ToString()
    | 2 ->
        if not <| card.ManaCost.Contains '/' then "Gld"
        else colors |> Seq.toArray |> String
    | _ -> "Gld"

let private getAccent (card: CardDetails) : string =
    if card.SpecialFrames = "token"
    then "C"
    else
        let colors = getColors card
        match colors.Length with
        | 0 ->
            match card.LandOverlay with
            | "" -> ""
            | _ -> "C"
        | 1 -> colors.Head.ToString()
        | 2 ->
            if   colors.Contains 'W' && colors.Contains 'U' then "WU"
            elif colors.Contains 'U' && colors.Contains 'B' then "UB"
            elif colors.Contains 'B' && colors.Contains 'R' then "BR"
            elif colors.Contains 'R' && colors.Contains 'G' then "RG"
            elif colors.Contains 'G' && colors.Contains 'W' then "GW"
            elif colors.Contains 'W' && colors.Contains 'B' then "WB"
            elif colors.Contains 'W' && colors.Contains 'R' then "RW"
            elif colors.Contains 'U' && colors.Contains 'R' then "UR"
            elif colors.Contains 'U' && colors.Contains 'G' then "GU"
            elif colors.Contains 'B' && colors.Contains 'G' then "BG"
            else failwith "invalid colors"
        | _ -> "Gld"


let private processCardInner (cardsToCenter: string list) (card: CardDetails) : CardDetails =
    // Set accent and template
    let card =
        { card with
            Template = getCardTemplate card
            Accent = getAccent card
        }

    // Fix centering bug
    let card =
        if cardsToCenter |> Seq.contains card.Name
        then { card with Center = "true" }
        else card

    card

let private loadCardsToCenter (setAbbrev: string) (ctx: UserContext) =
    async {
        let path = ctx.Workspace.Set(setAbbrev).CenterFixes
        match! FileSystem.loadFromJson<string list> path with
        | Some cards -> return cards
        | None -> return []
    }

let processCard (card: CardDetails) ctx =
    async {
        let! cardsToCenter = loadCardsToCenter card.Set ctx
        return processCardInner cardsToCenter card
    }

let processSet (setAbbrev: string) (cards: CardDetails list) (ctx: UserContext) =
    async {
        ctx.Log.Information "Processing cards..."

        let! cardsToCenter = loadCardsToCenter setAbbrev ctx

        ctx.Log.Information "\tCalculating properies..."
        let cards = cards |> List.map (processCardInner cardsToCenter)

        ctx.Log.Information "\tGenerating card numbers..."
        let cards = cards |> generateAndApplyNumbers

        ctx.Log.Information "\tCards processed."
        return cards
    }