// Modifies cards before saving.
module Processor

open System
open System.Linq
open Model

let cardsToCenter = [
    "Time Walk"
    "Demonic Tutor"
    "Wheel of Fortune"
    "Jokulhaups"
    "Bazaar of Baghdad"
    "Shahrazad"
    "Buried in Time"
    "Martyr Drill"
    "Krark's Thumb"
    "Krark's Trampoline"
    "Thirst for Power"
    "Unnatural Gravity"
    "Master of None"
    "Never-Ending Story"
    "Black Lotus"
    "Food"
    "Treasure"
    "Boar"
    "Tutor Booster"
    "Falling Star"
    "Chaos Orb"
]

let getColors (card: CardDetails) : char list =
    card.ManaCost.Intersect(['W';'U';'B';'R';'G']) |> Seq.toList

let private getColorGroup (card: CardDetails) : ColorGroup = 
    if card.SuperType.ToLower().Contains("token") then ColorGroup.Token
    elif card.Type.ToLower().Contains("land") then ColorGroup.Land
    else
        let colors = getColors card

        match colors.Length with
        | 0 -> if card.Type.Contains("Artifact") then ColorGroup.Artifact else ColorGroup.Colorless
        | 1 -> match colors.Head with
                | 'W' -> ColorGroup.White
                | 'U' -> ColorGroup.Blue
                | 'B' -> ColorGroup.Black
                | 'R' -> ColorGroup.Red
                | 'G' -> ColorGroup.Green
                | _ -> failwith "invalid symbol"
        | _ -> if card.ManaCost.Contains("/") then ColorGroup.Hybrid else ColorGroup.Multi

let private generateNumbers (cards: CardDetails seq) : (int * CardDetails) seq =
    cards
    |> Seq.groupBy getColorGroup
    |> Seq.sortBy (fun (grp, _) -> grp)
    |> Seq.collect (fun (_, cs) -> cs |> Seq.sortBy (fun c -> c.Name))
    |> Seq.indexed
    |> Seq.map (fun (n, c) -> (n+1, c))

let private generateAndApplyNumbers (cards: CardDetails list) : CardDetails list =
    let count = cards.Length
    generateNumbers cards 
    |> Seq.map (fun (n, c) -> 
        { c with 
            Number = n.ToString().PadLeft(count.ToString().Length, '0'); 
            Total = count.ToString() 
        })
    |> Seq.toList

let private getCardTemplate (card: CardDetails) : string =
    let colors = getColors card
    match colors.Length with
    | 0 -> "C"
    | 1 -> colors.Head.ToString()
    | 2 -> 
        if not <| card.ManaCost.Contains('/') then "Gld"
        else String(colors |> Seq.toArray)
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
            if colors.Contains('W') && colors.Contains('U') then "WU"
            elif colors.Contains('U') && colors.Contains('B') then "UB"
            elif colors.Contains('B') && colors.Contains('R') then "BR"
            elif colors.Contains('R') && colors.Contains('G') then "RG"
            elif colors.Contains('G') && colors.Contains('W') then "GW"
            elif colors.Contains('W') && colors.Contains('B') then "WB"
            elif colors.Contains('W') && colors.Contains('R') then "RW"
            elif colors.Contains('U') && colors.Contains('R') then "UR"
            elif colors.Contains('U') && colors.Contains('G') then "GU"
            elif colors.Contains('B') && colors.Contains('G') then "BG"
            else failwith "invalid colors"
        | _ -> "Gld"

let processCard (card: CardDetails) : CardDetails =
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

let processCards (cards : CardDetails list) : CardDetails list =
    printfn "Processing cards..."

    printfn "\tCalculating properties..."
    let cards = cards |> List.map processCard

    printfn "\tGenerating card numbers..."
    let cards = generateAndApplyNumbers cards

    cards