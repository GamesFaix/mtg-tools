module GamesFaix.MtgTools.Designer.Cli

open Argu
open Model

type CardArguments =
    | [<CliPrefix(CliPrefix.None)>] Copy of oldSet:string * cardName:string * newSet:string
    | [<CliPrefix(CliPrefix.None)>] Delete of set:string * card:string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Copy _ -> "Copies a card"
            | Delete _ -> "Deletes a card"

type SetArguments =
    | [<CliPrefix(CliPrefix.None)>] Audit of set:string
    | [<CliPrefix(CliPrefix.None)>] AutoNumber of set:string
    | [<CliPrefix(CliPrefix.None)>] Copy of oldSet:string * newSet:string
    | [<CliPrefix(CliPrefix.None)>] CreateHtmlLayout of set:string
    | [<CliPrefix(CliPrefix.None)>] Delete of set:string
    | [<CliPrefix(CliPrefix.None)>] DownloadImages of set:string
    | [<CliPrefix(CliPrefix.None)>] Rename of oldSet:string * newSet:string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Audit _ -> "Audits a set for anomalies"
            | AutoNumber _ -> "Updates the collector's number on all cards in a set"
            | Copy _ -> "Copies a set"
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
        | CardArguments.Copy (oldSet, cardName, newSet) -> Macro.Card.copy ctx oldSet cardName newSet
        | CardArguments.Delete (set, card) -> Macro.Card.delete ctx set card

    | Set results ->
        match results.GetAllResults().Head with
        | Audit set -> Macro.Set.audit ctx set
        | AutoNumber set -> Macro.Set.autonumber ctx set
        | SetArguments.Copy (oldSet, newSet) -> Macro.Set.copy ctx oldSet newSet
        | CreateHtmlLayout set -> Macro.Set.createHtmlLayout ctx set
        | SetArguments.Delete set -> Macro.Set.delete ctx set
        | DownloadImages set -> Macro.Set.downloadImages ctx set
        | Rename (oldSet, newSet) -> Macro.Set.rename ctx oldSet newSet
