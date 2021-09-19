module GamesFaix.MtgTools.Designer.Cli

open Argu
open Auth
open Context

type LoginArgs =
    | [<AltCommandLine("-e")>] Email of string option
    | [<AltCommandLine("-p")>] Pass of string option
    | [<AltCommandLine("-s")>] SaveCreds of bool option

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Email _ -> "Email address to use. If blank, tries to use saved credentials."
            | Pass _ -> "Password to use. If blank, tries to use saved credentials."
            | SaveCreds _ -> "If true, saves credentials to disc. Defaults to false."

//type LogoutArgs =
//    interface IArgParserTemplate with
//        member this.Usage =
//            ""

type WorkspaceArgs =
    | [<AltCommandLine("-d")>] Dir of string option

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Dir _ -> "Directory to set as workspace."

type MainArguments =
    | [<CliPrefix(CliPrefix.None)>] Login of LoginArgs ParseResults
    //| [<CliPrefix(CliPrefix.None)>] Logout of LogoutArgs ParseResults
    | [<CliPrefix(CliPrefix.None)>] Workspace of WorkspaceArgs ParseResults

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Login _ -> "Authenticates and saves cookie for later requests"
            //| Logout _ -> "Logs out"
            | Workspace _ -> "Gets or sets workspace directory for later requests"

let private getLoginJob (ctx: Context) (results: LoginArgs ParseResults) =
    let args = results.GetAllResults()
    let email = args |> Seq.choose (fun a -> match a with Email x -> Some x | _ -> None) |> Seq.tryHead |> Option.defaultValue None
    let pass = args |> Seq.choose (fun a -> match a with Pass x -> Some x | _ -> None) |> Seq.tryHead |> Option.defaultValue None
    let saveCreds = args |> Seq.choose (fun a -> match a with SaveCreds x -> Some x | _ -> None) |> Seq.tryHead |> Option.defaultValue None |> Option.defaultValue false

    let login workspace =
        let creds : Credentials option =
            match email, pass with
            | Some e, Some p -> Some { Email = e; Password = p }
            | _ -> None
        Auth.login workspace creds saveCreds

    match ctx with
    | Context.Empty _ ->
        Error "No workspace directory is set. Please set one before logging in." |> Async.fromValue
    | Context.Workspace ctx -> login ctx.Workspace
    | Context.User ctx -> login ctx.Workspace

let private getWorkspaceJob (ctx: Context) (results: WorkspaceArgs ParseResults) = async {
    let args = results.GetAllResults()
    let dir = args |> Seq.choose (fun a -> match a with Dir x -> Some x | _ -> None) |> Seq.tryHead |> Option.defaultValue None

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

let getJob (ctx: Context) (results: MainArguments ParseResults) : Async<Result<unit, string>> =
    match results.GetAllResults().Head with
    | Login results -> getLoginJob ctx results
    //| Logout results -> failwith "Not implemented"
    | Workspace results -> getWorkspaceJob ctx results
