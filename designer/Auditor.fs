module Auditor

open System
open Model
type ArrayList<'a> = System.Collections.Generic.List<'a>

type Issue = {
    description: string
    cardName: string
}

let private findCardIssues (card : CardDetails) : Issue seq =
    let hasArt (c : CardDetails) =
        try 
            System.Uri(card.ArtworkUrl) |> ignore
            true
        with _ -> false

    let hasArtist (c : CardDetails) =
        not <| String.IsNullOrWhiteSpace(c.Artist) &&
        c.Artist <> "No artist credit"

    let xs = ArrayList<Issue>()
    let add desc = xs.Add { cardName = card.Name; description = desc }
    
    if hasArt card then ()
    else add "Missing artwork"

    if hasArtist card then ()
    else add "Missing artist credit"

    xs :> Issue seq

let findIssues (cards : CardDetails list) : Issue list =
    cards 
    |> Seq.collect findCardIssues
    |> Seq.sortBy (fun iss -> sprintf "%s - %s" iss.description iss.cardName)
    |> Seq.toList

let printIssues (issues: Issue list) : unit =
    let groupByDesc = issues |> Seq.groupBy (fun iss -> iss.description)
    for (key, xs) in groupByDesc do
        printfn "\t%s" key
        for issue in xs do
            printfn "\t\t%s" issue.cardName