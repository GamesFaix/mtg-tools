module GamesFaix.MtgTools.Shared.Context

open Serilog

type IContext = 
    abstract member Log : ILogger

