module GamesFaix.MtgTools.Designer.Cli.Login

open Argu
open GamesFaix.MtgTools.Designer
open GamesFaix.MtgTools.Designer.Context

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

let command (args: Args ParseResults) =
    let email = args.GetResult Email
    let pass = args.GetResult Pass
    let saveCreds = args.GetResult SaveCreds |> Option.defaultValue false

    let login =
        let creds : Auth.Credentials option =
            match email, pass with
            | Some e, Some p -> Some { Email = e; Password = p }
            | _ -> None
        Auth.login creds saveCreds

    function
    | Context.Empty _ ->
        Error "No workspace directory is set. Please set one before logging in." |> async.Return
    | Context.Workspace ctx -> login ctx.Workspace
    | Context.User ctx -> login ctx.Workspace
