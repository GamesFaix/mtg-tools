module GamesFaix.MtgTools.Archivist.TransactionLoader

open Serilog
open Model
open System.Text.RegularExpressions
open System.IO
open System
open System.Linq

let private loadCardFile (name: string) (dir: Workspace.TransactionDirectory) (log: ILogger): CardCount list Async =
    async {
        let path = dir.GetCardFile name
        log.Information $"\tLoading cards from {path}..."
        let! cards = Csv.loadCardFile path
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
    
(*
    Allow more implicitly defined transactions.
    * All .txt files are assumed to be part of AddFiles, only specifying SubtractFiles is required
    * Allow omitting the manifest file. If no manifest, parse transaction directory name for title and date
*)

let private loadManfiest (dir: Workspace.TransactionDirectory) : TransactionManifest Async = 
    async {
        let pattern = Regex("(?<y>\d{4})-(?<m>\d{2})-(?<d>\d{2})-(?<title>.*)")
        let name = DirectoryInfo(dir.Path).Name
        let m = pattern.Match name

        let! maybeManifest = FileSystem.loadFromJson<TransactionManifest> dir.Manifest 

        let info = 
            match maybeManifest, m.Success with
            | Some manifest, _ -> manifest.Info
            | None, true -> {
                    Title = m.Groups.["title"].Value.Replace('-', ' ')
                    Date = DateTime(int m.Groups.["y"].Value, int m.Groups.["m"].Value, int m.Groups.["d"].Value)
                    Notes = None
                    Price = None
                }
            | _, _ -> {
                    Title = ""
                    Date = DateTime.MinValue
                    Notes = None
                    Price = None
                }

        let existingCardFiles = 
            Directory.GetFiles(dir.Path) 
            |> Seq.filter (fun f -> f.EndsWith(".txt"))

        let addFiles = 
            match maybeManifest with
            | Some manifest -> manifest.AddFiles |> Option.defaultValue []
            | _ -> []

        let subtractFiles = 
            match maybeManifest with
            | Some manifest -> manifest.SubtractFiles |> Option.defaultValue []
            | _ -> []

        let implicitAddFiles = 
            Set.difference
                (Set.ofSeq existingCardFiles)
                (Set.ofSeq subtractFiles)
        
        let addFiles = 
            Set.union 
                (Set.ofSeq addFiles)
                implicitAddFiles
            |> Set.toList

        return {
            Info = info
            AddFiles = Some addFiles
            SubtractFiles = Some subtractFiles
        }
    }

let loadTransactionDetails (dir: Workspace.TransactionDirectory) (log: ILogger) : Result<TransactionDetails, string> Async =
    async {
        let! manifest = loadManfiest dir

        let! add = 
            manifest.AddFiles 
            |> Option.defaultValue [] 
            |> collectAsync (fun f -> loadCardFile f dir log)

        let! subtract =
            manifest.SubtractFiles 
            |> Option.defaultValue []
            |> collectAsync (fun f -> loadCardFile f dir log)

        log.Information $"\tAdding {add.Length} and subtracting {subtract.Length} cards."
        return Ok {
            Info = manifest.Info
            Add = add
            Subtract = subtract
        }
    }