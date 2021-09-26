module GamesFaix.MtgTools.Archivist.Csv

open System
open System.Globalization
open System.IO
open CsvHelper
open CsvHelper.Configuration
open Model

let private toDomain (c: DragonShieldCard) : CardCount =
    c.Count,
    {
        Name = c.Name
        Set = c.Set
        Version = c.Version
        Language = c.Language
    }

let private toDragonShield ((ct, c): CardCount) : DragonShieldCard =
    {
        Count = ct
        Name = c.Name
        Set = c.Set
        Version = c.Version
        Language = c.Language
        Price = 0.0m
        Date = DateTime.MinValue
        Condition = ""
    }

let loadCardFile (path: string) : CardCount list Async =
    async {
        let config = CsvConfiguration(CultureInfo.InvariantCulture)
        config.HasHeaderRecord <- false
        
        use reader = File.OpenText path :> TextReader
        use csv = new CsvReader(reader, config)
        csv.Read() |> ignore

        let cards =
            csv.GetRecords<DragonShieldCard>()
            |> Seq.map toDomain
            |> Seq.toList

        return cards
    }

let saveCardFile (path: string) (cards: CardCount list) : unit Async =
    async {
        let config = CsvConfiguration(CultureInfo.InvariantCulture)
        config.HasHeaderRecord <- false

        let cards = cards |> Seq.map toDragonShield
        
        use writer = new StreamWriter(path)
        use csv = new CsvWriter(writer, config)

        csv.WriteRecords cards
        return ()
    }