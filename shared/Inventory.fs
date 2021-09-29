module GamesFaix.MtgTools.Shared.Inventory

open System.Globalization
open System.IO
open CsvHelper
open CsvHelper.Configuration

type Card = {
    Name : string
    Set : string
    Version : string
    Language : string
}

type CardCount = int * Card

// Property order must reflect CSV column order; parsing headers is not working
// Must be public for CsvHelper
type InventoryCsvCard = {
    Count : int
    Name : string
    Set : string
    Version : string
    Language : string
}

let private toInventoryCsv ((ct, c): CardCount) : InventoryCsvCard =
    {
        Count = ct
        Name = c.Name
        Set = c.Set
        Version = c.Version
        Language = c.Language
    }
    
let private fromInventoryCsv (card: InventoryCsvCard) : CardCount =
    let c =
        {
            Name = card.Name
            Set = card.Set
            Version = card.Version
            Language = card.Language
        }
    (card.Count, c)

let saveInventoryFile (path: string) (cards: CardCount list) : unit Async =
    async {
        let config = CsvConfiguration(CultureInfo.InvariantCulture)
        config.HasHeaderRecord <- true

        let cards = cards |> Seq.map toInventoryCsv |> Seq.sortBy (fun c -> c.Name)
        
        use writer = new StreamWriter(path)
        use csv = new CsvWriter(writer, config)

        csv.WriteRecords cards
        return ()
    }

let loadInventoryFile (path: string) : CardCount list Async =
    async {
        use reader = File.OpenText path :> TextReader

        let config = CsvConfiguration(CultureInfo.InvariantCulture)
        config.HasHeaderRecord <- false

        use csv = new CsvReader(reader, config)
        csv.Read() |> ignore

        return
            csv.GetRecords<InventoryCsvCard>()
            |> Seq.map fromInventoryCsv
            |> Seq.toList
    }