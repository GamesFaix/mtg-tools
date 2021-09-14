module Utils

open System.Threading.Tasks
open FSharp.Control.Tasks

let concurrentMap<'a, 'b> (f : 'a -> 'b Task) (xs : 'a list) : 'b list Task =
    task {
        let tasks = xs |> List.map f
        let! _ = Task.WhenAll tasks
        return tasks |> List.map (fun t -> t.Result)
    }
    
let mergeUnit (xs : unit list Task) : unit Task =
    task {
        let! _ = xs
        return ()
    }

let seriesMap<'a, 'b> (f : 'a -> 'b Task) (xs : 'a list) : 'b list Task =
    task {
        let results = System.Collections.Generic.List()

        for x in xs do
            let! result = f x
            results.Add result
            ()

        return results |> Seq.toList
    }

