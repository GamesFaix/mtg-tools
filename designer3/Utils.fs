namespace GamesFaix.MtgTools.Designer

[<AutoOpen>]
module Async =
    let map<'a, 'b> (projection: 'a -> 'b) (source: 'a Async) : 'b Async =
        async {
            let! a = source
            return projection a
        }

    let bind<'a, 'b> (projection: 'a -> 'b Async) (source: 'a Async) : 'b Async =
        async {
            let! a = source
            return! projection a
        }

    let fromValue<'a> (value: 'a) : 'a Async =
        async {
            return value
        }