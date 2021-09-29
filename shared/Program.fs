module GamesFaix.MtgTools.Shared.Program

open Argu
open Serilog
open Context

let main<'ctx, 'args 
    when 'ctx :> IContext 
    and 'args :> IArgParserTemplate
    > 
    (programName: string)
    (run: ParseResults<'args> -> 'ctx -> Async<Result<unit, string>>)
    args
    : 'ctx -> int Async =
    
    fun ctx ->
        async {
            ctx.Log.Information programName

            let parser = ArgumentParser.Create<'args>(programName = programName)

            try
                let args = parser.Parse(inputs = args, raiseOnUsage = true)
                ctx.Log.Debug (args.ToString())

                let! result = run args ctx

                match result with
                | Ok () ->
                    ctx.Log.Information "Done."
                    return 0

                | Error err ->
                    ctx.Log.Error err
                    return -1
            with
            | :? ArguParseException ->
                ctx.Log.Information (parser.PrintUsage())
                return -2
            | ex ->
                ctx.Log.Error(ex, "An unexpected error occurred.")
                return -3
        }