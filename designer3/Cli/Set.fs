module GamesFaix.MtgTools.Designer.Cli.Set

open Argu
open GamesFaix.MtgTools.Designer
open GamesFaix.MtgTools.Designer.Context

type private SaveMode = MtgDesign.Writer.SaveMode

let private copyOrRename (ctx: UserContext) (fromSet: string) (toSet: string) (mode: SaveMode) =
    async {
        let action = if mode = SaveMode.Create then "Copying" else "Renaming"
        ctx.Log.Information $"{action} set {fromSet} to {toSet}..."
        let! cards = MtgDesign.Reader.getSetCardDetails ctx fromSet
        let! cards = CardProcessor.processSet ctx fromSet cards
        let cards = cards |> List.map (fun c -> { c with Set = toSet })
        do! MtgDesign.Writer.saveCards ctx mode cards
        ctx.Log.Information "Done."
        return Ok ()
    }

module Copy =
    type Args =
        | [<AltCommandLine("-f")>] From of string
        | [<AltCommandLine("-t")>] To of string

        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | From _ -> "The set abbreviation."
                | To _ -> "The copy's set abbreviation."

    let getJob (ctx: UserContext) (results: Args ParseResults) : JobResult =
        let args = results.GetAllResults()
        let fromSet = args |> Seq.choose (fun a -> match a with From x -> Some x | _ -> None) |> Seq.tryHead
        let toSet = args |> Seq.choose (fun a -> match a with To x -> Some x | _ -> None) |> Seq.tryHead

        match fromSet, toSet with
        | Some fromSet, Some toSet ->
            copyOrRename ctx fromSet toSet SaveMode.Create
        | _ -> Error "Invalid arguments." |> Async.fromValue

module Delete =
    type Args =
        | [<CliPrefix(CliPrefix.None)>] Set of string

        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Set _ -> "The set abbreviation."

    let getJob (ctx: UserContext) (results: Args ParseResults) : JobResult =
        let args = results.GetAllResults()
        let set = args |> Seq.choose (fun a -> match a with Set x -> Some x | _ -> None) |> Seq.tryHead

        match set with
        | Some set ->
            async {
                ctx.Log.Information $"Deleting set {set}..."
                let! cardInfos = MtgDesign.Reader.getSetCardInfos ctx set
                do! MtgDesign.Writer.deleteCards ctx cardInfos
                ctx.Log.Information "Done."
                return Ok ()
            }
        | _ -> Error "Invalid arguments." |> Async.fromValue

module Rename =
    type Args =
        | [<AltCommandLine("-f")>] From of string
        | [<AltCommandLine("-t")>] To of string

        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | From _ -> "The old set abbreviation."
                | To _ -> "The new set abbreviation."

    let getJob (ctx: UserContext) (results: Args ParseResults) : JobResult =
        let args = results.GetAllResults()
        let fromSet = args |> Seq.choose (fun a -> match a with From x -> Some x | _ -> None) |> Seq.tryHead
        let toSet = args |> Seq.choose (fun a -> match a with To x -> Some x | _ -> None) |> Seq.tryHead

        match fromSet, toSet with
        | Some fromSet, Some toSet ->
            copyOrRename ctx fromSet toSet SaveMode.Edit
        | _ -> Error "Invalid arguments." |> Async.fromValue

type Args =
    | [<CliPrefix(CliPrefix.None)>] Copy of Copy.Args ParseResults
    | [<CliPrefix(CliPrefix.None)>] Delete of Delete.Args ParseResults
    | [<CliPrefix(CliPrefix.None)>] Rename of Rename.Args ParseResults

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Copy _ -> "Copies a set."
            | Delete _ -> "Deletes a set."
            | Rename _ -> "Renames a set."

let getJob (ctx: Context) (results: Args ParseResults) : JobResult =
    match ctx, (results.GetAllResults().Head) with
    | Empty _, _
    | Workspace _, _ -> Error "This operation requires a logged in user." |> Async.fromValue
    | User ctx, Copy results -> Copy.getJob ctx results
    | User ctx, Delete results -> Delete.getJob ctx results
    | User ctx, Rename results -> Rename.getJob ctx results
