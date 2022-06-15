module MtgDesign

open FSharp.Control.Tasks
open System.Net.Http
open System.Text.RegularExpressions
open System.IO
open System.Text.Json

let private client = new HttpClient()

let getCardNames (setCode: string) = task {
    let path = FileSystem.mtgdCardsDataPath ()

    let options = JsonSerializerOptions()
    options.WriteIndented <- true

    let inner () = task {
        printfn "Parsing card names from MTG.design..."
        let! html = client.GetStringAsync($"https://mtg.design/u/tautologist/{setCode}")
        let pattern = Regex "<li class=\"lazy\" id=\"(.*)\">"
        let matches = pattern.Matches html 
        let data = matches |> Seq.map (fun m -> m.Groups.[1].Value) |> Seq.toList
        let json = JsonSerializer.Serialize(data, options)
        do! File.WriteAllTextAsync(path, json)     
        return data
    }

    if File.Exists path then
        printfn "Reading cached cards from disk."
        let json = File.ReadAllText path
        let data = JsonSerializer.Deserialize<string list>(json)
        return data
    else
        return! inner()
}