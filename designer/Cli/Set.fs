module GamesFaix.MtgTools.Designer.Cli.Set

open Argu
open GamesFaix.MtgTools.Designer
open GamesFaix.MtgTools.Designer.Context
open GamesFaix.MtgTools.Designer.Model
open GamesFaix.MtgTools.Shared

type private SaveMode = MtgdWriter.SaveMode

let private loadCards (set: string) ctx =
    async {
        let! cards = MtgdReader.getSetCardDetails set ctx
        return! CardProcessor.processSet set cards ctx
    }

let private copyOrRename (fromSet: string) (toSet: string) (mode: SaveMode) ctx =
    async {
        let action = if mode = SaveMode.Create then "Copying" else "Renaming"
        ctx.Log.Information $"{action} set {fromSet} to {toSet}..."
        let! cards = loadCards fromSet ctx
        let cards = cards |> List.map (fun c -> { c with Set = toSet })
        do! MtgdWriter.saveCards mode cards ctx
        ctx.Log.Information "Done."
        return Ok ()
    }

module Audit =
    type Args =
        | [<MainCommand; ExactlyOnce; Last>] Set of string

        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Set _ -> "The set abbreviation."

    let getJob (results: Args ParseResults) ctx =
        async {
            let set = results.GetResult Set
            ctx.Log.Information $"Auditing set {set}..."
            let! cards = loadCards set ctx
            Auditor.findIssues cards
            |> Auditor.logIssues ctx.Log
            ctx.Log.Information "Done."
            return Ok ()
        }

module Copy =
    type Args =
        | [<Mandatory; AltCommandLine("-f")>] From of string
        | [<Mandatory; AltCommandLine("-t")>] To of string

        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | From _ -> "The set abbreviation."
                | To _ -> "The copy's set abbreviation."

    let getJob (results: Args ParseResults) =
        let fromSet = results.GetResult From
        let toSet = results.GetResult To
        copyOrRename fromSet toSet SaveMode.Create

module Delete =
    type Args =
        | [<MainCommand; ExactlyOnce; Last>] Set of string

        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Set _ -> "The set abbreviation."

    let getJob (results: Args ParseResults) ctx =
        async {
            let set = results.GetResult Set
            ctx.Log.Information $"Deleting set {set}..."
            let! cardInfos = MtgdReader.getSetCardInfos set ctx
            do! MtgdWriter.deleteCards cardInfos ctx
            ctx.Log.Information "Done."
            return Ok ()
        }

module Layout =
    type Args =
        | [<MainCommand; ExactlyOnce; Last>] Set of string

        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Set _ -> "The set abbreviation."

    let getJob (results: Args ParseResults) ctx =
        async {
            let set = results.GetResult Set
            ctx.Log.Information $"Creating HTML layout for set {set}..."
            let! cardInfos = MtgdReader.getSetCardInfos set ctx
            let html = Layout.createHtmlLayout cardInfos
            do! FileSystem.saveFileText html (ctx.Workspace.Set(set).HtmlLayout)
            ctx.Log.Information "Done."
            return Ok ()
        }

module Pull =
    type Args =
        | [<MainCommand; ExactlyOnce; Last>] Set of string

        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Set _ -> "The set abbreviation."

    let getJob (results: Args ParseResults) ctx =
        let set = results.GetResult Set
        let setDir = ctx.Workspace.Set(set)

        let downloadImage (card: CardDetails) =
            async {
                ctx.Log.Information $"\tDownloading image for card {card.Name}..."
                let! bytes = MtgdReader.getCardImage (card |> CardDetails.toInfo)
                let path = setDir.CardImage(card.Name)
                return! FileSystem.saveFileBytes bytes path
            }

        async {
            ctx.Log.Information $"Pulling latest for set {set}..."
            let! details = MtgdReader.getSetCardDetails set ctx

            ctx.Log.Information $"\tSaving data file..."
            do! FileSystem.saveToJson details setDir.JsonDetails

            // Clear old images
            do! FileSystem.deleteFilesInFolderMatching setDir.Path (fun f -> f.EndsWith ".jpg")

            // Download images
            do! details
                |> List.map downloadImage
                |> Async.Parallel
                |> Async.Ignore

            ctx.Log.Information "Done."
            return Ok ()
        }

module Rename =
    type Args =
        | [<Mandatory; AltCommandLine("-f")>] From of string
        | [<Mandatory; AltCommandLine("-t")>] To of string

        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | From _ -> "The old set abbreviation."
                | To _ -> "The new set abbreviation."

    let getJob (results: Args ParseResults) =
        let fromSet = results.GetResult From
        let toSet = results.GetResult To
        copyOrRename fromSet toSet SaveMode.Edit

module Scrub =
    type Args =
        | [<MainCommand; ExactlyOnce; Last>] Set of string

        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Set _ -> "The set abbreviation."

    let getJob (results: Args ParseResults) ctx =
        async {
            let set = results.GetResult Set
            ctx.Log.Information $"Scrubbing set {set}..."
            let! cards = loadCards set ctx
            let! _ = MtgdWriter.saveCards SaveMode.Edit cards ctx
            ctx.Log.Information "Done."
            return Ok ()
        }

type Args =
    | [<CliPrefix(CliPrefix.None)>] Audit of Audit.Args ParseResults
    | [<CliPrefix(CliPrefix.None)>] Copy of Copy.Args ParseResults
    | [<CliPrefix(CliPrefix.None)>] Delete of Delete.Args ParseResults
    | [<CliPrefix(CliPrefix.None)>] Layout of Layout.Args ParseResults
    | [<CliPrefix(CliPrefix.None)>] Pull of Pull.Args ParseResults
    | [<CliPrefix(CliPrefix.None)>] Rename of Rename.Args ParseResults
    | [<CliPrefix(CliPrefix.None)>] Scrub of Scrub.Args ParseResults

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Audit _ -> "Audits a set."
            | Copy _ -> "Copies a set."
            | Delete _ -> "Deletes a set."
            | Layout _ -> "Creates and HTML layout for printing a set."
            | Pull _ -> "Downloads images and data for a set."
            | Rename _ -> "Renames a set."
            | Scrub _ -> "Downloads cards, processes them, then posts updates. Fixes things like collectors numbers."

let getJob (results: Args ParseResults) = function
    | Empty _
    | Workspace _ -> Error "This operation requires a logged in user." |> async.Return
    | User ctx ->
        match results.GetAllResults().Head with
        | Audit results -> Audit.getJob results ctx
        | Copy results -> Copy.getJob results ctx
        | Delete results -> Delete.getJob results ctx
        | Layout results -> Layout.getJob results ctx
        | Pull results -> Pull.getJob results ctx
        | Rename results -> Rename.getJob results ctx
        | Scrub results -> Scrub.getJob results ctx
