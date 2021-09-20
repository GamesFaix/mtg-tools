module GamesFaix.MtgTools.Designer.Cli.Card

open Argu
open GamesFaix.MtgTools.Designer
open GamesFaix.MtgTools.Designer.Context

type private SaveMode = MtgDesign.Writer.SaveMode

let private copyOrRename (ctx: UserContext) (name: string) (fromSet: string) (toSet: string) (mode: SaveMode) =
    async {
        let action = if mode = SaveMode.Create then "Copying" else "Renaming"
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
        | [<AltCommandLine("-f")>] FromSet of string
        | [<AltCommandLine("-t")>] ToSet of string
        | [<AltCommandLine("-n")>] Name of string

        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | FromSet _ -> "The card's set abbreviation."
                | ToSet _ -> "The copy's set abbreviation."
                | Name _ -> "The card's name."

    let getJob (ctx: UserContext) (results: Args ParseResults) : JobResult =
        let args = results.GetAllResults()
        let fromSet = args |> Seq.choose (fun a -> match a with FromSet x -> Some x | _ -> None) |> Seq.tryHead
        let toSet = args |> Seq.choose (fun a -> match a with ToSet x -> Some x | _ -> None) |> Seq.tryHead
        let name = args |> Seq.choose (fun a -> match a with Name x -> Some x | _ -> None) |> Seq.tryHead

        match fromSet, toSet, name with
        | Some fromSet, Some toSet, Some name ->
            copyOrRename ctx name fromSet toSet SaveMode.Create
        | _ -> Error "Invalid arguments." |> Async.fromValue

module Delete =
    type Args =
        | [<CliPrefix(CliPrefix.None)>] Set of string
        | [<CliPrefix(CliPrefix.None)>] Name of string

        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Set _ -> "The card's set abbreviation."
                | Name _ -> "The card's name."

    let getJob (ctx: UserContext) (results: Args ParseResults) : JobResult =
        let args = results.GetAllResults()
        let set = args |> Seq.choose (fun a -> match a with Set x -> Some x | _ -> None) |> Seq.tryHead
        let name = args |> Seq.choose (fun a -> match a with Name x -> Some x | _ -> None) |> Seq.tryHead

        match set, name with
        | Some set, Some name ->
            async {
                ctx.Log.Information $"Deleting card {set} - {name}..."
                let! cardInfos = MtgDesign.Reader.getSetCardInfos ctx set
                let card = cardInfos |> Seq.find (fun c -> c.Name = name)
                do! MtgDesign.Writer.deleteCard ctx card
                ctx.Log.Information "Done."
                return Ok ()
            }
        | _ -> Error "Invalid arguments." |> Async.fromValue

module Rename =
    type Args =
        | [<AltCommandLine("-f")>] FromSet of string
        | [<AltCommandLine("-t")>] ToSet of string
        | [<AltCommandLine("-n")>] Name of string

        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | FromSet _ -> "The card's set abbreviation."
                | ToSet _ -> "The card's new set abbreviation."
                | Name _ -> "The card's name."

    let getJob (ctx: UserContext) (results: Args ParseResults) : JobResult =
        let args = results.GetAllResults()
        let fromSet = args |> Seq.choose (fun a -> match a with FromSet x -> Some x | _ -> None) |> Seq.tryHead
        let toSet = args |> Seq.choose (fun a -> match a with ToSet x -> Some x | _ -> None) |> Seq.tryHead
        let name = args |> Seq.choose (fun a -> match a with Name x -> Some x | _ -> None) |> Seq.tryHead

        match fromSet, toSet, name with
        | Some fromSet, Some toSet, Some name ->
            copyOrRename ctx name fromSet toSet SaveMode.Edit
        | _ -> Error "Invalid arguments." |> Async.fromValue

type Args =
    | [<CliPrefix(CliPrefix.None)>] Copy of Copy.Args ParseResults
    | [<CliPrefix(CliPrefix.None)>] Delete of Delete.Args ParseResults
    | [<CliPrefix(CliPrefix.None)>] Rename of Rename.Args ParseResults

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Copy _ -> "Copies a card."
            | Delete _ -> "Deletes a card."
            | Rename _ -> "Renames a card."

let getJob (ctx: Context) (results: Args ParseResults) : JobResult =
    match ctx, (results.GetAllResults().Head) with
    | Empty _, _
    | Workspace _, _ -> Error "This operation requires a logged in user." |> Async.fromValue
    | User ctx, Copy results -> Copy.getJob ctx results
    | User ctx, Delete results -> Delete.getJob ctx results
    | User ctx, Rename results -> Rename.getJob ctx results
