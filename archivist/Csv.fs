module GamesFaix.MtgTools.Archivist.Csv

open System.Globalization
open System.IO
open CsvHelper
open CsvHelper.Configuration
open Model

(* 
    Note: DragonShield does not output properly formatted CSV files. 
      * It uses ', ' as a delimiter, so strings must be trimmed.
      * Strings are not quoted, so card names with commas screw up the columns.
 *)

let loadCardFile (path: string) : CardCount list Async =
    async {
        let config = CsvConfiguration(CultureInfo.InvariantCulture)
        config.HasHeaderRecord <- false
        
        use reader = File.OpenText path :> TextReader
        use csv = new CsvReader(reader, config)

        csv.Read() |> ignore

        let cards =
            csv.GetRecords<DragonShieldCsvCard>()
            |> Seq.map Card.fromDragonShieldCsv // Trims strings
            |> Seq.toList

        return cards
    }

let saveCardFile (path: string) (cards: CardCount list) : unit Async =
    async {
        let config = CsvConfiguration(CultureInfo.InvariantCulture)
        config.HasHeaderRecord <- false

        let cards = cards |> Seq.map Card.toInventoryCsv
        
        use writer = new StreamWriter(path)
        use csv = new CsvWriter(writer, config)

        csv.WriteRecords cards
        return ()
    }