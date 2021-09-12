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

type Card = {
    Editions: CardEdition list
}
with
    member this.Name = this.Editions.Head.Name
    member this.Count = this.Editions |> Seq.sumBy (fun e -> e.Count)
    member this.Sets = this.Editions |> Seq.map (fun e -> e.Set)

let private readCsv path =
    use reader = File.OpenText path :> TextReader

    let config = CsvConfiguration(CultureInfo.InvariantCulture)
    config.HasHeaderRecord <- false

    use csv = new CsvReader(reader, config)
    csv.Read() |> ignore

    csv.GetRecords<CardEdition>()
    |> Seq.toList

let load path : Card list =
    readCsv path
    |> List.groupBy (fun e -> e.Name)
    |> List.map (fun (_, editions) -> { Editions = editions })