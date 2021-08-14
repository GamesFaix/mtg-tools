open System
open ScryfallApi.Client.Models
open System.Net.Http
open System.IO
open CsvHelper
open CsvHelper.Configuration
open System.Globalization
open System.Linq

// https://scryfall.com/docs/syntax
let scryfallQuery = "o:'each player draw'"

let searchScryfall query = 
    async {
        printfn "Searching for %s" query 

        use client = new HttpClient()
        client.BaseAddress <- new Uri("https://api.scryfall.com/")
        let scryfall = new ScryfallApi.Client.ScryfallApiClient(client)

        let options = SearchOptions()

        let results = ResizeArray()

        let mutable hasMore = true
        let mutable i = 1
        while hasMore do
            printfn "Requesting page %i" i
            let! result = scryfall.Cards.Search(query, i, options) |> Async.AwaitTask
            results.AddRange(result.Data)
            hasMore <- result.HasMore
            i <- i+1
            do! Async.Sleep(100) // courtesy throttling

        return results
    }

type InventoryCard = {
    count: int
    name: string
    set: string    
}

let loadInventory () =
    let path = sprintf "%s/inventory1 - main.csv" (Environment.GetFolderPath(Environment.SpecialFolder.Desktop))
    use reader = File.OpenText path :> TextReader
    let config = CsvConfiguration(CultureInfo.InvariantCulture)
    config.HasHeaderRecord <- false
    use csv = new CsvReader(reader, config)
    csv.Read() |> ignore
    csv.GetRecords<InventoryCard>()
    |> Seq.toList

[<EntryPoint>]
let main _ =
    async {
        let! results = searchScryfall scryfallQuery
        let inventory = loadInventory ()
        let joined = 
            Enumerable.Join(
                results,
                inventory,
                (fun x -> (x.Name.ToLowerInvariant(), x.Set.ToLowerInvariant())),
                (fun y -> (y.name.ToLowerInvariant(), y.set.ToLowerInvariant())),
                (fun x y -> (x, y))
            )

        //printfn "Found %i cards" result.TotalCards

        //for c in result.Data do
        //    printfn "\t%s" c.Name

        //for c in inventory |> Seq.take 10 do
        //    printfn "\t%s %s" c.set c.name

            
        for (x,y) in joined do
            printfn "\t%s %s %s" x.Set x.Name x.ManaCost
        
        Console.Read() |> ignore
        return 0 // return an integer exit code
    } |> Async.RunSynchronously
    
