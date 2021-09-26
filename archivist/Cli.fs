module GamesFaix.MtgTools.Archivist.Cli

open Argu
open GamesFaix.MtgTools.Archivist.Context
open System.Net.Http
open FSharp.Data

type JobResult = Async<Result<unit, string>>

type Args =
    | [<CliPrefix(CliPrefix.None)>] Echo of string
    | [<CliPrefix(CliPrefix.None)>] RefreshDb

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Echo _ -> "Prints the input string"
            | RefreshDb -> "Downloads card data from Scryfall."

let downloadScryfallData (ctx: Context) : JobResult =
    async {
        ctx.Log.Information "Downloading data from Scryfall..."
        use client = new HttpClient()

        let bulkDataInfoUrl = "https://api.scryfall.com/bulk-data"
        let! bulkDataInfoResponse = client.GetAsync bulkDataInfoUrl |> Async.AwaitTask
        match bulkDataInfoResponse.IsSuccessStatusCode with
        | true ->
            ctx.Log.Information "Saving data to file..."
            let! json = bulkDataInfoResponse.Content.ReadAsStringAsync() |> Async.AwaitTask
            let path = "./scryfall-data.json"
            do! FileSystem.saveFileText json path
            ctx.Log.Information "Download complete."

            let obj = JObject.Parse





            return Ok ()
        | _ ->
            return Error ""





        let url = "https://c2.scryfall.com/file/scryfall-bulk/unique-artwork/unique-artwork.json"
        use client = new HttpClient()
        let! response = client.GetAsync url |> Async.AwaitTask
        match response.IsSuccessStatusCode with
        | true ->
            ctx.Log.Information "Saving data to file..."
            let! json = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            let path = "./scryfall-data.json"
            do! FileSystem.saveFileText json path
            ctx.Log.Information "Download complete."
            return Ok ()
        | _ ->
            return Error ""
    }

type X = JsonProvider<"./bin/Debug/net5.0/scryfall-data.json">

let getJob (results: Args ParseResults) (ctx: Context) : JobResult =
    async {
        match results.GetAllResults().Head with
        | Echo str ->
            ctx.Log.Information str
            return Ok ()
        | RefreshDb ->
            return! downloadScryfallData ctx
    }
