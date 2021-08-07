open System
open ScryfallApi.Client.Models
open System.Net.Http
open System.IO
open CsvHelper
open CsvHelper.Configuration
open System.Globalization
open System.Linq

let searchScryfall query = 
    async {
        use client = new HttpClient()
        client.BaseAddress <- new Uri("https://api.scryfall.com/")
        let scryfall = new ScryfallApi.Client.ScryfallApiClient(client)

        let options = SearchOptions()

        return! scryfall.Cards.Search(query, 1, options) |> Async.AwaitTask
    }

type InventoryCard = {
    count: int
    name: string
    set: string    
}

let loadInventory () : InventoryCard list =
    let path = sprintf "%s/inventory1 - main.csv" (Environment.GetFolderPath(Environment.SpecialFolder.Desktop))
    use reader = File.OpenText path :> TextReader
    let config = CsvConfiguration(CultureInfo.InvariantCulture)
    config.HasHeaderRecord <- false
    use csv = new CsvReader(reader, config)
    csv.Read() |> ignore
    let cards = csv.GetRecords<InventoryCard>()
    cards |> Seq.toList

[<EntryPoint>]
let main _ =
    async {
        let! result = searchScryfall "o:proliferate"
        let inventory = loadInventory ()
        let joined = 
            Enumerable.Join(
                result.Data,
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

            
        for (x,y) in joined |> Seq.take 10 do
            printfn "\t%s %s" x.Set x.Name
        
        Console.Read() |> ignore
        return 0 // return an integer exit code
    } |> Async.RunSynchronously
    
