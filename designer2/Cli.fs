module GamesFaix.MtgTools.Designer.Cli

open Argu
open Model

type CardArguments =
    | [<CliPrefix(CliPrefix.None)>] Clone of oldSet:string * oldCard:string * newSet:string * newCard:string
    | [<CliPrefix(CliPrefix.None)>] Delete of set:string * card:string
    | [<CliPrefix(CliPrefix.None)>] DownloadImage of set:string * card:string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Clone _ -> "Copies a card"
            | Delete _ -> "Deletes a card"
            | DownloadImage _ -> "Downloads the image of a card"

type SetArguments =
    | [<CliPrefix(CliPrefix.None)>] Audit of set:string
    | [<CliPrefix(CliPrefix.None)>] AutoNumber of set:string
    | [<CliPrefix(CliPrefix.None)>] Clone of oldSet:string * newSet:string
    | [<CliPrefix(CliPrefix.None)>] CreateHtmlLayout of set:string
    | [<CliPrefix(CliPrefix.None)>] Delete of set:string
    | [<CliPrefix(CliPrefix.None)>] DownloadImages of set:string
    | [<CliPrefix(CliPrefix.None)>] Rename of oldSet:string * newSet:string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Audit _ -> "Audits a set for anomalies"
            | AutoNumber _ -> "Updates the collector's number on all cards in a set"
            | Clone _ -> "Copies a set"
            | CreateHtmlLayout _ -> "Creates an HTML layout file for printing a set"
            | Delete _ -> "Deletes a set"
            | DownloadImages _ -> "Downloads images for each card in a set"
            | Rename _ -> "Renames a set"

type MainArguments =
    | [<CliPrefix(CliPrefix.None)>] Card of CardArguments ParseResults
    | [<CliPrefix(CliPrefix.None)>] Set of SetArguments ParseResults

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Card _ -> "Allows manipulating individual cards"
            | Set _ -> "Allows manipulating sets (expansions) of cards"

let getJob (ctx: Context) (results: MainArguments ParseResults) : unit Async =
    match results.GetAllResults().Head with

    | Card results ->
        match results.GetAllResults().Head with
        | CardArguments.Clone (oldSet, oldCard, newSet, newCard) -> failwith "Not implemented"
        | CardArguments.Delete (set, card) -> failwith "Not implemented"
        | DownloadImage (set, card) -> failwith "Not implemented"

    | Set results ->
        match results.GetAllResults().Head with
        | Audit set -> failwith "Not implemented"
        | AutoNumber set -> failwith "Not implemented"
        | SetArguments.Clone (oldSet, newSet) -> Macro.cloneSet ctx oldSet newSet
        | CreateHtmlLayout set -> failwith "Not implemented"
        | SetArguments.Delete set -> failwith "Not implemented"
        | DownloadImages set -> failwith "Not implemented"
        | Rename (oldSet, newSet) -> failwith "Not implemented"
