open System
open ScryfallApi.Client.Models
open System.Net.Http

[<EntryPoint>]
let main _ =
    async {
        use client = new HttpClient()
        client.BaseAddress <- new Uri("https://api.scryfall.com/")
        let scryfall = new ScryfallApi.Client.ScryfallApiClient(client)

        let options = SearchOptions()

        let! result = scryfall.Cards.Search("o:flip", 1, options) |> Async.AwaitTask
        

        printfn "Found %i cards" result.TotalCards

        for c in result.Data do
            printfn "\t%s" c.Name
        
        Console.Read() |> ignore
        return 0 // return an integer exit code
    } |> Async.RunSynchronously
    
