[<AutoOpen>]
module GamesFaix.MtgTools.Archivist.Utils

module List =

    let collectAsync<'a, 'b> (projection: 'a -> 'b list Async) (source: 'a list) : 'b list Async =
        async {
            let results = ResizeArray()
            for x in source do
                let! ys = projection x
                results.AddRange ys
            return results |> Seq.toList
        }
    