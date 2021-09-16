module GamesFaix.MtgTools.Designer.Program

open System
open Microsoft.Extensions.Configuration

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

    printfn "Hello World from F#!"
    0 // return an integer exit code
