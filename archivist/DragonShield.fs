module GamesFaix.MtgTools.Archivist.DragonShield

open System
open System.IO
open GamesFaix.MtgTools.Shared.Inventory

(*
    DragonShield's .txt files are almost CSV files, except they don't quote values,
    so card names with commas break CSV parsing. This is a custom parser that will
    work around the issue.
 *)

type private DragonShieldCsvCard = {
    Count : int
    Name : string
    Set : string
    Condition : string
    Price : decimal
    Version : string
    Language : string
    Date : DateTime
}
 
let private toCardCount (c: DragonShieldCsvCard) : CardCount =
    c.Count,
    {
        Name = c.Name.Trim()
        Set = c.Set.Trim()
        Version = c.Version.Trim()
        Language = c.Language.Trim()
    }

let private parseDragonShieldCard (line: string) : DragonShieldCsvCard =
    let words = line.Split(',') |> List.ofArray
    
    // Get count from first cell
    let count = int words.Head
    let words = words.Tail

    // Flip the list
    let words = words |> List.rev

    // Get date, land, version, price, condition, set from end
    let date = words.Head |> DateTime.Parse
    let words = words.Tail

    let lang = words.Head.Trim()
    let words = words.Tail

    let version = words.Head.Trim()
    let words = words.Tail

    let price = words.Head |> decimal
    let words = words.Tail

    let condition = words.Head.Trim()
    let words = words.Tail

    let set = words.Head.Trim()
    let words = words.Tail

    // Flip back
    let words = words |> List.rev
    let name = String.Join("", words)

    {
        Count = count
        Name = name
        Set = set
        Condition = condition
        Price = price
        Version = version
        Language = lang
        Date = date
    }

let loadCardFile (path: string) : CardCount list Async =
    async {
        use reader = File.OpenText path
        reader.ReadLine() |> ignore // Title line
        reader.ReadLine() |> ignore // Blank line

        return
            seq {
                while not reader.EndOfStream do
                    yield reader.ReadLine() |> parseDragonShieldCard |> toCardCount
            } |> Seq.toList
    }