module GamesFaix.MtgTools.Archivist.TransactionLoader

open System.Globalization
open System.IO
open CsvHelper
open CsvHelper.Configuration
open Serilog
open Model

let private loadManifest (dir: Workspace.TransactionDirectory) : TransactionManifest option Async =
    FileSystem.loadFromJson<TransactionManifest> dir.Manifest

let private mapCard (c: DragonShieldCard) : CardCount =
    c.Count,
    {
        Name = c.Name
        Set = c.Set
        Version = c.Version
        Language = c.Language
    }

let private loadCardFile (name: string) (dir: Workspace.TransactionDirectory) (log: ILogger): CardCount list Async =
    async {
        let path = dir.GetCardFile name
        log.Information $"\tLoading cards from {path}..."

        use reader = File.OpenText path :> TextReader

        let config = CsvConfiguration(CultureInfo.InvariantCulture)
        config.HasHeaderRecord <- false

        use csv = new CsvReader(reader, config)
        csv.Read() |> ignore

        let cards =
            csv.GetRecords<DragonShieldCard>()
            |> Seq.map mapCard
            |> Seq.toList

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
        match! loadManifest dir with
        | None -> return Error "Transaction manifest not found."
        | Some manifest ->
            let! add = manifest.AddFiles |> collectAsync (fun f -> loadCardFile f dir log)
            let! subtract = manifest.SubtractFiles |> collectAsync (fun f -> loadCardFile f dir log)
            log.Information $"Adding {add.Length} and subtracting {subtract.Length} cards."
            return Ok {
                Info = manifest.Info
                Add = add
                Subtract = subtract
            }
    }