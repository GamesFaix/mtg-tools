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
    let settings = configure args

    let logger =
        LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger()

    let ctx : Context = {
        Logger = logger
        Http = new HttpClient()
        RootDir = settings.OutputDirectory
        Cookie = ""
    }

    try
        let parser = ArgumentParser.Create<MainArguments>(programName = "designer")
        let results = parser.Parse(inputs = args, raiseOnUsage = true)
        let job = Cli.getJob ctx results
        job |> Async.RunSynchronously
        0 // return an integer exit code
    with e ->
        logger.Error(e, "An unexpected error occurred.")
        1
