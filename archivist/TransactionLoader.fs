module GamesFaix.MtgTools.Archivist.TransactionLoader

open System
open System.Globalization
open System.IO
open CsvHelper
open CsvHelper.Configuration
open Serilog

// DragonShield exports don't have headers, so the property order matters
type DragonShieldCard = {
    Count : int
    Name : string
    Set : string
    Condition : string
    Price : decimal
    Version : string
    Language : string
    Date : DateTime
}

// Structure of manifest.json file
type TransactionInfo = {
    Title : string
    Date : DateTime
    Price : decimal option
    Notes : string option
    AddFiles : string list
    SubtractFiles : string list
}

// Hydrated manifest file
type TransactionDetails = {
    Title : string
    Date : DateTime
    Price : decimal option
    Notes : string option
    Add : DragonShieldCard list
    Subtract : DragonShieldCard list
}

let private loadTransactionInfo (dir: Workspace.TransactionDirectory) : TransactionInfo option Async =
    FileSystem.loadFromJson<TransactionInfo> dir.Manifest

let private loadCardFile (name: string) (dir: Workspace.TransactionDirectory) (log: ILogger): DragonShieldCard list Async =
    async {
        let path = dir.GetCardFile name
        log.Information $"\tLoading cards from {path}..."

        use reader = File.OpenText path :> TextReader

        let config = CsvConfiguration(CultureInfo.InvariantCulture)
        config.HasHeaderRecord <- false

        use csv = new CsvReader(reader, config)
        csv.Read() |> ignore

        let cards = csv.GetRecords<DragonShieldCard>() |> Seq.toList

        log.Information $"\tFound {cards.Length} cards."
        return cards
    }

let private collectAsync<'a, 'b> (projection: 'a -> 'b list Async) (source: 'a list) : 'b list Async =
    async {
        let results = ResizeArray()
        for x in source do
            let! ys = projection x
            results.AddRange ys
        return results |> Seq.toList
    }

let loadTransactionDetails (dir: Workspace.TransactionDirectory) (log: ILogger) : Result<TransactionDetails, string> Async =
    async {
        log.Information $"Loading transaction {dir.Path}..."
        match! loadTransactionInfo dir with
        | None -> return Error "Transaction manifest not found."
        | Some info ->
            let! add = info.AddFiles |> collectAsync (fun f -> loadCardFile f dir log)
            let! subtract = info.SubtractFiles |> collectAsync (fun f -> loadCardFile f dir log)
            log.Information $"Adding {add.Length} and subtracting {subtract.Length} cards."
            return Ok {
                Title = info.Title
                Date = info.Date
                Price = info.Price
                Notes = info.Notes
                Add = add
                Subtract = subtract
            }
    }