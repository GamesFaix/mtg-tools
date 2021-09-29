module GamesFaix.MtgTools.Shared.Log

open Serilog

let logger =
    LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .CreateLogger()
