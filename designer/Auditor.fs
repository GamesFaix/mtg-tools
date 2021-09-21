module GamesFaix.MtgTools.Designer.Auditor

open System
open Serilog
open Model

type Issue = {
    Description : string
    CardName : string
}

let private findCardIssues (card : CardDetails) : Issue seq =
    let hasArt (c : CardDetails) =
        try
            Uri card.ArtworkUrl |> ignore
            true
        with _ -> false

    let hasArtist (c : CardDetails) =
        not <| String.IsNullOrWhiteSpace c.Artist &&
        c.Artist <> "No artist credit"

    let xs = ResizeArray()
    let add desc = xs.Add { CardName = card.Name; Description = desc }

    if hasArt card then ()
    else add "Missing artwork"

    if hasArtist card then ()
    else add "Missing artist credit"

    xs :> Issue seq

let findIssues (cards : CardDetails list) : Issue list =
    cards
    |> Seq.collect findCardIssues
    |> Seq.sortBy (fun iss -> sprintf "%s - %s" iss.Description iss.CardName)
    |> Seq.toList

let logIssues (log: ILogger) (issues: Issue list) : unit =
    let groupByDesc = issues |> Seq.groupBy (fun iss -> iss.Description)
    for (key, xs) in groupByDesc do
        log.Information $"\t{key}"
        for issue in xs do
            log.Information $"\t\t{issue.CardName}"