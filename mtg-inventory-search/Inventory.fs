module GamesFaix.MtgInventorySearch.Inventory

open System.Globalization
open System.IO
open CsvHelper
open CsvHelper.Configuration

// Property order must reflect CSV column order
type CardEdition = {
    Count: int
    Name: string
    Set: string
}

let load path =
    use reader = File.OpenText path :> TextReader

    let config = CsvConfiguration(CultureInfo.InvariantCulture)
    config.HasHeaderRecord <- false

    use csv = new CsvReader(reader, config)
    csv.Read() |> ignore

    csv.GetRecords<CardEdition>()
    |> Seq.toList
