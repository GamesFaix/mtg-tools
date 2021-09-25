module GamesFaix.MtgTools.Archivist.Cli

open Argu
open GamesFaix.MtgTools.Archivist.Context

type JobResult = Async<Result<unit, string>>

type Args =
    | [<CliPrefix(CliPrefix.None)>] Echo of string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Echo _ -> "Prints the input string"

let getJob (results: Args ParseResults) (ctx: Context) : JobResult =
    async {
        match results.GetAllResults().Head with
        | Echo str ->
            ctx.Log.Information str
            return Ok ()
    }
