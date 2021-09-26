module GamesFaix.MtgTools.Archivist.Cli.RefreshDb

//let downloadScryfallData (ctx: Context) : JobResult =
//    async {
//        ctx.Log.Information "Downloading data from Scryfall..."
//        use client = new HttpClient()

//        let bulkDataInfoUrl = "https://api.scryfall.com/bulk-data"
//        let! bulkDataInfoResponse = client.GetAsync bulkDataInfoUrl |> Async.AwaitTask
//        match bulkDataInfoResponse.IsSuccessStatusCode with
//        | true ->
//            ctx.Log.Information "Saving data to file..."
//            let! json = bulkDataInfoResponse.Content.ReadAsStringAsync() |> Async.AwaitTask
//            let path = "./scryfall-data.json"
//            do! FileSystem.saveFileText json path
//            ctx.Log.Information "Download complete."

//            let obj = JObject.Parse





//            return Ok ()
//        | _ ->
//            return Error ""





//        let url = "https://c2.scryfall.com/file/scryfall-bulk/unique-artwork/unique-artwork.json"
//        use client = new HttpClient()
//        let! response = client.GetAsync url |> Async.AwaitTask
//        match response.IsSuccessStatusCode with
//        | true ->
//            ctx.Log.Information "Saving data to file..."
//            let! json = response.Content.ReadAsStringAsync() |> Async.AwaitTask
//            let path = "./scryfall-data.json"
//            do! FileSystem.saveFileText json path
//            ctx.Log.Information "Download complete."
//            return Ok ()
//        | _ ->
//            return Error ""
//    }

//type X = JsonProvider<"./bin/Debug/net5.0/scryfall-data.json">
