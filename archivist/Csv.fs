module GamesFaix.MtgTools.Archivist.Csv

open System.Globalization
open System.IO
open CsvHelper
open CsvHelper.Configuration
open Model

type private InventoryCsvCard = {
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