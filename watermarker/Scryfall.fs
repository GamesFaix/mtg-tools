module Scryfall

open System.Net.Http
open ScryfallApi.Client
open System
open FSharp.Control.Tasks
open ScryfallApi.Client.Models
open System.IO
open System.Text.Json
open System.Threading.Tasks

let private httpClient = new HttpClient()
httpClient.BaseAddress <- Uri "https://api.scryfall.com"

let mutable private config = ScryfallApiClientConfig ()
config.ScryfallApiBaseAddress <- Uri "https://api.scryfall.com"

let private scryfall = ScryfallApiClient(httpClient, config)

let private getCard (name: string) = task {
    printfn "Searching for card %s..." name

    // https://scryfall.com/docs/syntax
    let query = $"!\"{name}\""
    
    let mutable options = SearchOptions()
    options.Mode <- SearchOptions.RollupMode.Prints
    options.Sort <- SearchOptions.CardSort.Released
    options.Direction <- SearchOptions.SortDirection.Asc
    options.IncludeExtras <- true

    let! results = scryfall.Cards.Search(query, 1, options)

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

let getAllSets () = task {
    let path = FileSystem.scryfallSetsDataPath()
    
    let options = JsonSerializerOptions()
    options.WriteIndented <- true

    let inner () = task {
        printfn "Downloading sets data from Scryfall..."
        let! results = scryfall.Sets.Get()
        let data = results.Data |> Seq.toList 
        let json = JsonSerializer.Serialize(data, options)
        do! File.WriteAllTextAsync(path, json)
        return data
    }

    if File.Exists path then
        let! json = File.ReadAllTextAsync path
        let data = JsonSerializer.Deserialize<Set list>(json)
        return data
    else
        return! inner()
}

let getCards (names: string seq) = task {
    let path = FileSystem.scryfallCardsDataPath()

    let options = JsonSerializerOptions()
    options.WriteIndented <- true

    let inner () = task {
        printfn "Searching for each card on Scryfall..."
        let! data =
            names
            |> Seq.map getCard
            |> Task.WhenAll

        let json = JsonSerializer.Serialize(data, options)
        do! File.WriteAllTextAsync(path, json)
        return data |> Array.toList
    }

    if File.Exists path then
        let! json = File.ReadAllTextAsync path
        let data = JsonSerializer.Deserialize<Card list>(json)
        return data
    else
        return! inner()
}

let private downloadSetSymbolSvg (set: Set) = task {
    let inner () = task { 
        use request = new HttpRequestMessage(HttpMethod.Get, set.IconSvgUri)
        let! response = httpClient.SendAsync request
        let! contentStream = response.Content.ReadAsStreamAsync ()
        use stream = new FileStream(FileSystem.svgPath set.Code, FileMode.Create)
        do! contentStream.CopyToAsync stream
    }

    if File.Exists (FileSystem.svgPath set.Code) 
    then 
        printfn "Found downloaded SVG for %s" set.Code
        return ()
    else 
        printfn "Downloading SVG for %s..." set.Code
        return! inner ()
}

let downloadSetSymbolSvgs (cards: Card seq) = task {
    let codes = cards |> Seq.map (fun c -> c.Set) |> Seq.distinct
    let! sets = getAllSets()
    let sets = sets |> Seq.filter (fun s -> codes |> Seq.contains s.Code)
    let! _ = sets |> Seq.map downloadSetSymbolSvg |> Task.WhenAll
    return ()
}