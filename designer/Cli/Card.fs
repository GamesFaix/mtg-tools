module GamesFaix.MtgTools.Designer.Cli.Card

open Argu
open GamesFaix.MtgTools.Designer
open GamesFaix.MtgTools.Designer.Context

type private SaveMode = MtgdWriter.SaveMode

let private copyOrMove (name: string) (fromSet: string) (toSet: string) (mode: SaveMode) =
    fun ctx -> async {
        let action = if mode = SaveMode.Create then "Copying" else "Moving"
        ctx.Log.Information $"{action} card {name} from {fromSet} to {toSet}..."
        let! cardInfos = MtgdReader.getSetCardInfos fromSet ctx
        let card = cardInfos |> Seq.find (fun c -> c.Name = name)
        let! details = MtgdReader.getCardDetails card ctx
        let! details = CardProcessor.processCard details ctx
        let details = { details with Set = toSet }
        do! MtgdWriter.saveCards mode [details] ctx
        ctx.Log.Information "Done."
        return Ok ()
    }

module Copy =
    type Args =
        | [<Mandatory; AltCommandLine("-f")>] FromSet of string
        | [<Mandatory; AltCommandLine("-t")>] ToSet of string
        | [<Mandatory; AltCommandLine("-n")>] Name of string

        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | FromSet _ -> "The card's set abbreviation."
                | ToSet _ -> "The copy's set abbreviation."
                | Name _ -> "The card's name."

    let getJob (results: Args ParseResults) =
        let fromSet = results.GetResult FromSet
        let toSet = results.GetResult ToSet
        let name = results.GetResult Name
        copyOrMove name fromSet toSet SaveMode.Create

module Delete =
    type Args =
        | [<Mandatory; AltCommandLine("-s")>] Set of string
        | [<Mandatory; AltCommandLine("-n")>] Name of string

        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Set _ -> "The card's set abbreviation."
                | Name _ -> "The card's name."

    let getJob (results: Args ParseResults) =
        fun ctx -> async {
            let set = results.GetResult Set
            let name = results.GetResult Name
            ctx.Log.Information $"Deleting card {set} - {name}..."
            let! cardInfos = MtgdReader.getSetCardInfos set ctx
            let card = cardInfos |> Seq.find (fun c -> c.Name = name)
            do! MtgdWriter.deleteCard card ctx
            ctx.Log.Information "Done."
            return Ok ()
        }

module Move =
    type Args =
        | [<Mandatory; AltCommandLine("-f")>] FromSet of string
        | [<Mandatory; AltCommandLine("-t")>] ToSet of string
        | [<Mandatory; AltCommandLine("-n")>] Name of string

        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | FromSet _ -> "The card's set abbreviation."
                | ToSet _ -> "The card's new set abbreviation."
                | Name _ -> "The card's name."

    let getJob (results: Args ParseResults) =
        let fromSet = results.GetResult FromSet
        let toSet = results.GetResult ToSet
        let name = results.GetResult Name
        copyOrMove name fromSet toSet SaveMode.Edit

type Args =
    | [<CliPrefix(CliPrefix.None)>] Copy of Copy.Args ParseResults
    | [<CliPrefix(CliPrefix.None)>] Delete of Delete.Args ParseResults
    | [<CliPrefix(CliPrefix.None)>] Move of Move.Args ParseResults

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Copy _ -> "Copies a card."
            | Delete _ -> "Deletes a card."
            | Move _ -> "Moves a card."

let getJob (results: Args ParseResults) =
    fun ctx ->
        match ctx, (results.GetAllResults().Head) with
        | Empty _, _
        | Workspace _, _ -> Error "This operation requires a logged in user." |> async.Return
        | User ctx, Copy results -> Copy.getJob results ctx
        | User ctx, Delete results -> Delete.getJob results ctx
        | User ctx, Move results -> Move.getJob results ctx
