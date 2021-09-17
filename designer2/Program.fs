module GamesFaix.MtgTools.Designer.Program

open System
open System.Net.Http
open Argu
open Microsoft.Extensions.Configuration
open Serilog
open Cli
open Model

type Settings = {
    OutputDirectory : string
}

let configure args =

    let config =
        ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional = false, reloadOnChange = false)
            .Build()

    let settings = {
        OutputDirectory = config.["OutputDirectory"] |> Environment.ExpandEnvironmentVariables
    }

    if String.IsNullOrWhiteSpace settings.OutputDirectory then failwith $"{nameof settings.OutputDirectory} cannot be blank"

    settings

[<EntryPoint>]
let main args =
    async {
        let settings = configure args

        let logger =
            LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger()

        try
            let parser = ArgumentParser.Create<MainArguments>(programName = "designer")
            let results = parser.Parse(inputs = args, raiseOnUsage = true)
            let workspace = WorkspaceDirectory.create settings.OutputDirectory

            let! cookie = FileSystem.loadCookie workspace
            if cookie.IsNone then failwith "Not logged in"

            let ctx : Context = {
                Logger = logger
                Http = new HttpClient()
                Workspace = workspace
                Cookie = cookie.Value
            }

            let job = Cli.getJob ctx results
            job |> Async.RunSynchronously
            return 0
        with e ->
            logger.Error(e, "An unexpected error occurred.")
            return 1
    }
    |> Async.RunSynchronously