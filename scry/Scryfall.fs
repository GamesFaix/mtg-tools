module GamesFaix.MtgTools.Scry.Scryfall

open System
open ScryfallApi.Client.Models
open System.Net.Http
open System.Linq
open System.Text.RegularExpressions

let private client = new HttpClient()
client.BaseAddress <- Uri("https://api.scryfall.com/")
let private scryfall = ScryfallApi.Client.ScryfallApiClient(client)

let private throttleMilliseconds = 100

let private getAllPages getPage =
    async {
        let results = ResizeArray()
        let mutable i = 1
        let mutable hasMore = true
        while hasMore do
            let! (page, hm) = getPage i
            results.AddRange(page)
            hasMore <- hm
            i <- i + 1
            do! Async.Sleep throttleMilliseconds
        return results |> Seq.toList
    }

let search query =
    async {
        let options = SearchOptions()

        let getPage n =
            async {
                let! result = scryfall.Cards.Search(query, n, options) |> Async.AwaitTask
                return (result.Data.AsEnumerable(), result.HasMore)
            }

        return! getAllPages getPage
    }

let getSets () =
    async {
        let! results = scryfall.Sets.Get() |> Async.AwaitTask

        return results.Data |> Seq.toList
    }

module Card =
    let getManaCost (card: Card) : string =
        match card.ManaCost with
        | null ->
            let costs = card.CardFaces |> Seq.map (fun f -> f.ManaCost) 
            let str = String.Join(" / ", costs)            
            Regex.Replace(str, "{(\\d|W|U|B|R|G)}", "$1")
                 .PadLeft(6)
        | cost -> cost