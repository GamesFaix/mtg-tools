module GamesFaix.MtgTools.Designer.Cli

open Argu
open Auth
open Context

type JobResult = Async<Result<unit, string>>

module Workspace =
    type Args =
        | [<CliPrefix(CliPrefix.None)>] Dir of string option

        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Dir _ -> "Directory to set as workspace."

    let getJob (ctx: Context) (results: Args ParseResults) : JobResult = async {
        let args = results.GetAllResults()
        let dir = args |> Seq.choose (fun a -> match a with Dir x -> x | _ -> None) |> Seq.tryHead

        match dir with
        | Some d ->
            ctx.Log.Information $"Setting workspace to {d}..."
            do! Context.setWorkspace d
        | None ->
            match! Context.getWorkspace () with
            | Some d ->
                ctx.Log.Information $"Workspace is currently set to {d}."
            | None ->
                ctx.Log.Information "Workspace not currently set."

        return Ok ()
    }

module Login =
    type Args =
        | [<AltCommandLine("-e")>] Email of string option
        | [<AltCommandLine("-p")>] Pass of string option
        | [<AltCommandLine("-s")>] SaveCreds of bool option

        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Email _ -> "Email address to use. If blank, tries to use saved credentials."
                | Pass _ -> "Password to use. If blank, tries to use saved credentials."
                | SaveCreds _ -> "If true, saves credentials to disc. Defaults to false."

    let getJob (context: Context) (results: Args ParseResults) : JobResult =
        let args = results.GetAllResults()
        let email = args |> Seq.choose (fun a -> match a with Email x -> x | _ -> None) |> Seq.tryHead
        let pass = args |> Seq.choose (fun a -> match a with Pass x -> x | _ -> None) |> Seq.tryHead
        let saveCreds = args |> Seq.choose (fun a -> match a with SaveCreds x -> x | _ -> None) |> Seq.tryHead |> Option.defaultValue false

        let login workspace =
            let creds : Credentials option =
                match email, pass with
                | Some e, Some p -> Some { Email = e; Password = p }
                | _ -> None
            Auth.login workspace creds saveCreds

        match context with
        | Context.Empty _ ->
            Error "No workspace directory is set. Please set one before logging in." |> Async.fromValue
        | Context.Workspace ctx -> login ctx.Workspace
        | Context.User ctx -> login ctx.Workspace

//module Logout =

//    type LogoutArgs =
//        interface IArgParserTemplate with
//            member this.Usage =
//                ""

module Card =
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

module Set =
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

type Args =
    | [<CliPrefix(CliPrefix.None)>] Workspace of Workspace.Args ParseResults
    | [<CliPrefix(CliPrefix.None)>] Login of Login.Args ParseResults
    //| [<CliPrefix(CliPrefix.None)>] Logout of Logout.Args ParseResults
    | [<CliPrefix(CliPrefix.None)>] Set of Set.Args ParseResults
    | [<CliPrefix(CliPrefix.None)>] Card of Card.Args ParseResults

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Workspace _ -> "Gets or sets workspace directory for later requests"
            | Login _ -> "Authenticates and saves cookie for later requests"
            //| Logout _ -> "Logs out"
            | Set _ -> "Performs operations on sets of cards."
            | Card _ -> "Performs operations on individual cards."

let getJob (ctx: Context) (results: Args ParseResults) : JobResult =
    match results.GetAllResults().Head with
    | Workspace results -> Workspace.getJob ctx results
    | Login results -> Login.getJob ctx results
    //| Logout results -> failwith "Not implemented"
    | Set results -> Set.getJob ctx results
    | Card results -> Card.getJob ctx results
