module GamesFaix.MtgTools.Designer.Watermarks.Scryfall

open System.Net.Http
open ScryfallApi.Client
open System
open ScryfallApi.Client.Models
open System.IO
open System.Text.Json

let private httpClient = new HttpClient()
httpClient.BaseAddress <- Uri "https://api.scryfall.com"

let mutable private config = ScryfallApiClientConfig ()
config.ScryfallApiBaseAddress <- Uri "https://api.scryfall.com"

let private scryfall = ScryfallApiClient(httpClient, config)

let private getCard (name: string) = async {
    printfn "Searching for card %s..." name

    // https://scryfall.com/docs/syntax
    let query = $"!\"{name}\""
    
    let mutable options = SearchOptions()
    options.Mode <- SearchOptions.RollupMode.Prints
    options.Sort <- SearchOptions.CardSort.Released
    options.Direction <- SearchOptions.SortDirection.Asc
    options.IncludeExtras <- true

    let! results = scryfall.Cards.Search(query, 1, options) |> Async.AwaitTask

    if results.TotalCards = 0 then
        return failwith $"No card found named \"{name}\"."
    else 
        let result = 
            results.Data 
            |> Seq.filter (fun c -> not <| c.Name.Contains(" // ")) // No art cards
            |> Seq.head
        printfn "%s first printing is %s" name result.Set
        return result            
}

let getAllSets () = async {
    let path = FileSystem.scryfallSetsDataPath()
    
    let options = JsonSerializerOptions()
    options.WriteIndented <- true

    let inner () = async {
        printfn "Downloading sets data from Scryfall..."
        let! results = scryfall.Sets.Get() |> Async.AwaitTask
        let data = results.Data |> Seq.toList 
        let json = JsonSerializer.Serialize(data, options)
        do! File.WriteAllTextAsync(path, json) |> Async.AwaitTask
        return data
    }

    if File.Exists path then
        let! json = File.ReadAllTextAsync path |> Async.AwaitTask
        let data = JsonSerializer.Deserialize<Set list>(json)
        return data
    else
        return! inner()
}

let getCards (names: string seq) = async {
    let path = FileSystem.scryfallCardsDataPath()

    let options = JsonSerializerOptions()
    options.WriteIndented <- true

    let inner () = async {
        printfn "Searching for each card on Scryfall..."
        let! data =
            names
            |> Seq.map getCard
            |> Async.Parallel

        let json = JsonSerializer.Serialize(data, options)
        do! File.WriteAllTextAsync(path, json) |> Async.AwaitTask
        return data |> Array.toList
    }

    if File.Exists path then
        let! json = File.ReadAllTextAsync path |> Async.AwaitTask
        let data = JsonSerializer.Deserialize<Card list>(json)
        return data
    else
        return! inner()
}

let private downloadSetSymbolSvg (set: Set) = async {
    let inner () = async { 
        use request = new HttpRequestMessage(HttpMethod.Get, set.IconSvgUri)
        let! response = httpClient.SendAsync request |> Async.AwaitTask
        let! contentStream = response.Content.ReadAsStreamAsync () |> Async.AwaitTask
        use stream = new FileStream(FileSystem.svgPath set.Code, FileMode.Create)
        do! contentStream.CopyToAsync stream |> Async.AwaitTask
    }

    if File.Exists (FileSystem.svgPath set.Code) 
    then 
        printfn "Found downloaded SVG for %s" set.Code
        return ()
    else 
        printfn "Downloading SVG for %s..." set.Code
        return! inner ()
}

let downloadSetSymbolSvgs (cards: Card seq) = async {
    let codes = cards |> Seq.map (fun c -> c.Set) |> Seq.distinct
    let! sets = getAllSets()
    let sets = sets |> Seq.filter (fun s -> codes |> Seq.contains s.Code)
    let! _ = sets |> Seq.map downloadSetSymbolSvg |> Async.Parallel
    return ()
}