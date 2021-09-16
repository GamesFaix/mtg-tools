﻿module GamesFaix.MtgTools.Designer.CardProcessor

open System
open System.Linq
open Serilog
open Model

let getColors (card: CardDetails) : char list =
    card.ManaCost.Intersect ['W';'U';'B';'R';'G'] |> Seq.toList

let private getCollectorNumberGroup (card: CardDetails) : CollectorNumberGroup =
    if card.SuperType.ToLower().Contains "token" then CollectorNumberGroup.Token
    elif card.Type.ToLower().Contains "land" then CollectorNumberGroup.Land
    else
        let colors = getColors card

        match colors.Length with
        | 0 -> if card.Type.Contains "Artifact"
               then CollectorNumberGroup.Artifact
               else CollectorNumberGroup.Colorless
        | 1 -> match colors.Head with
                | 'W' -> CollectorNumberGroup.White
                | 'U' -> CollectorNumberGroup.Blue
                | 'B' -> CollectorNumberGroup.Black
                | 'R' -> CollectorNumberGroup.Red
                | 'G' -> CollectorNumberGroup.Green
                | _ -> failwith "invalid symbol"
        | _ -> if card.ManaCost.Contains "/"
               then CollectorNumberGroup.Hybrid
               else CollectorNumberGroup.Multi

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
            if card.LandOverlay = "A" then "C"
            else card.LandOverlay
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

let processCard (cardsToCenter: string list) (card: CardDetails) : CardDetails =
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

let processCards (log: ILogger) (cardsToCenter: string list) (cards : CardDetails list) : CardDetails list =
    log.Information "Processing cards..."

    log.Information "\tCalculating properies..."
    let cards = cards |> List.map (processCard cardsToCenter)

    log.Information "\tGenerating card numbers..."
    let cards = cards |> generateAndApplyNumbers

    log.Information "\tCards processed."
    cards