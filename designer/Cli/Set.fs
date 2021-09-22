module GamesFaix.MtgTools.Designer.Cli.Set

open Argu
open GamesFaix.MtgTools.Designer
open GamesFaix.MtgTools.Designer.Context
open GamesFaix.MtgTools.Designer.Model

type private SaveMode = MtgDesign.Writer.SaveMode

let private loadCards (ctx: UserContext) (set: string) = async {
    let! cards = MtgDesign.Reader.getSetCardDetails ctx set
    return! CardProcessor.processSet ctx set cards
}

let private copyOrRename (ctx: UserContext) (fromSet: string) (toSet: string) (mode: SaveMode) =
    async {
        let action = if mode = SaveMode.Create then "Copying" else "Renaming"
        ctx.Log.Information $"{action} set {fromSet} to {toSet}..."
        let! cards = loadCards ctx fromSet
        let cards = cards |> List.map (fun c -> { c with Set = toSet })
        do! MtgDesign.Writer.saveCards ctx mode cards
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

    let getJob (ctx: UserContext) (results: Args ParseResults) : JobResult =
        let set = results.GetResult Set
        async {
            ctx.Log.Information $"Auditing set {set}..."
            let! cards = loadCards ctx set
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

    let getJob (ctx: UserContext) (results: Args ParseResults) : JobResult =
        let fromSet = results.GetResult From
        let toSet = results.GetResult To
        copyOrRename ctx fromSet toSet SaveMode.Create

module Delete =
    type Args =
        | [<MainCommand; ExactlyOnce; Last>] Set of string

        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Set _ -> "The set abbreviation."

    let getJob (ctx: UserContext) (results: Args ParseResults) : JobResult =
        let set = results.GetResult Set
        async {
            ctx.Log.Information $"Deleting set {set}..."
            let! cardInfos = MtgDesign.Reader.getSetCardInfos ctx set
            do! MtgDesign.Writer.deleteCards ctx cardInfos
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

    let getJob (ctx: UserContext) (results: Args ParseResults) : JobResult =
        let set = results.GetResult Set
        async {
            ctx.Log.Information $"Creating HTML layout for set {set}..."
            let! cardInfos = MtgDesign.Reader.getSetCardInfos ctx set
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

    let getJob (ctx: UserContext) (results: Args ParseResults) : JobResult =
        let set = results.GetResult Set
        let setDir = ctx.Workspace.Set(set)

        let downloadImage (card: CardDetails) =
            async {
                ctx.Log.Information $"\tDownloading image for card {card.Name}..."
                let! bytes = MtgDesign.Reader.getCardImage (card |> CardDetails.toInfo)
                let path = setDir.CardImage(card.Name)
                return! FileSystem.saveFileBytes bytes path
            }

        async {
            ctx.Log.Information $"Pulling latest for set {set}..."
            let! details = MtgDesign.Reader.getSetCardDetails ctx set

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

    let getJob (ctx: UserContext) (results: Args ParseResults) : JobResult =
        let fromSet = results.GetResult From
        let toSet = results.GetResult To
        copyOrRename ctx fromSet toSet SaveMode.Edit

module Scrub =
    type Args =
        | [<MainCommand; ExactlyOnce; Last>] Set of string

        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Set _ -> "The set abbreviation."

    let getJob (ctx: UserContext) (results: Args ParseResults) : JobResult =
        let set = results.GetResult Set
        async {
            ctx.Log.Information $"Scrubbing set {set}..."
            let! cards = loadCards ctx set
            let! _ = MtgDesign.Writer.saveCards ctx SaveMode.Edit cards
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

let getJob (ctx: Context) (results: Args ParseResults) : JobResult =
    match ctx with
    | Empty _
    | Workspace _ -> Error "This operation requires a logged in user." |> async.Return
    | User ctx ->
        match results.GetAllResults().Head with
        | Audit results -> Audit.getJob ctx results
        | Copy results -> Copy.getJob ctx results
        | Delete results -> Delete.getJob ctx results
        | Layout results -> Layout.getJob ctx results
        | Pull results -> Pull.getJob ctx results
        | Rename results -> Rename.getJob ctx results
        | Scrub results -> Scrub.getJob ctx results
