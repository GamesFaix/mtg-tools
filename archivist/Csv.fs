module GamesFaix.MtgTools.Archivist.Csv

open System.Globalization
open System.IO
open CsvHelper
open CsvHelper.Configuration
open Model

let saveCardFile (path: string) (cards: CardCount list) : unit Async =
    async {
        let config = CsvConfiguration(CultureInfo.InvariantCulture)
        config.HasHeaderRecord <- true

        let cards = cards |> Seq.map Card.toInventoryCsv |> Seq.sortBy (fun c -> c.Name)b  
        
        use writer = new StreamWriter(path)
        use csv = new CsvWriter(writer, config)

        csv.WriteRecords cards
        return ()
    }