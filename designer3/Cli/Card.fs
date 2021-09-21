module GamesFaix.MtgTools.Designer.Cli.Card

open Argu
open GamesFaix.MtgTools.Designer
open GamesFaix.MtgTools.Designer.Context

type private SaveMode = MtgDesign.Writer.SaveMode

let private copyOrMove (ctx: UserContext) (name: string) (fromSet: string) (toSet: string) (mode: SaveMode) =
    async {
        let action = if mode = SaveMode.Create then "Copying" else "Moving"
        ctx.Log.Information $"{action} card {name} from {fromSet} to {toSet}..."
        let! cardInfos = MtgDesign.Reader.getSetCardInfos ctx fromSet
        let card = cardInfos |> Seq.find (fun c -> c.Name = name)
        let! details = MtgDesign.Reader.getCardDetails ctx card
        let! details = CardProcessor.processCard ctx details
        let details = { details with Set = toSet }
        do! MtgDesign.Writer.saveCards ctx mode [details]
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

    let getJob (ctx: UserContext) (results: Args ParseResults) : JobResult =
        let fromSet = results.GetResult FromSet
        let toSet = results.GetResult ToSet
        let name = results.GetResult Name
        copyOrMove ctx name fromSet toSet SaveMode.Create

module Delete =
    type Args =
        | [<Mandatory; AltCommandLine("-s")>] Set of string
        | [<Mandatory; AltCommandLine("-n")>] Name of string

        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Set _ -> "The card's set abbreviation."
                | Name _ -> "The card's name."

    let getJob (ctx: UserContext) (results: Args ParseResults) : JobResult =
        let set = results.GetResult Set
        let name = results.GetResult Name
        async {
            ctx.Log.Information $"Deleting card {set} - {name}..."
            let! cardInfos = MtgDesign.Reader.getSetCardInfos ctx set
            let card = cardInfos |> Seq.find (fun c -> c.Name = name)
            do! MtgDesign.Writer.deleteCard ctx card
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

    let getJob (ctx: UserContext) (results: Args ParseResults) : JobResult =
        let fromSet = results.GetResult FromSet
        let toSet = results.GetResult ToSet
        let name = results.GetResult Name
        copyOrMove ctx name fromSet toSet SaveMode.Edit

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

let getJob (ctx: Context) (results: Args ParseResults) : JobResult =
    match ctx, (results.GetAllResults().Head) with
    | Empty _, _
    | Workspace _, _ -> Error "This operation requires a logged in user." |> Async.fromValue
    | User ctx, Copy results -> Copy.getJob ctx results
    | User ctx, Delete results -> Delete.getJob ctx results
    | User ctx, Move results -> Move.getJob ctx results
