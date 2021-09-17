module GamesFaix.MtgTools.Designer.Model

open Serilog
open System.Net.Http
open Serilog.Events
open System.Collections.Generic
open System.IO

/// <summary> Basic card info for identification. Used on card list pages. </summary>
type CardInfo = {
    Id : string
    Name : string
    Set: string
}

/// <summary> Full card info. </summary>
type CardDetails = {
    Id: string
    Number: string
    Total: string
    Set: string
    Lang: string
    Designer: string
    Name: string
    ManaCost: string
    SuperType: string
    Type: string
    SubType: string
    SpecialFrames: string
    ColorIndicator: string
    Rarity: string
    RulesText: string
    FlavorText: string
    TextSize: string
    Center: string
    Foil: string
    Border: string
    ArtworkUrl: string
    CustomSetSymbolUrl: string
    WatermarkUrl: string
    LightenWatermark: string
    Artist: string
    Power: string
    Toughness: string
    // Extra properties that are conditional based on other choices, like saga frame
    LandOverlay: string
    Template: string
    Accent: string

    // Sagas & Planeswalkers
    PlaneswalkerSize: string
    LoyaltyCost1: string
    Rules2: string
    LoyaltyCost2: string
    Rules3: string
    LoyaltyCost3: string
    Rules4: string
    LoyaltyCost4: string
}

/// <summary> Group used for assigning card numbers. </summary>
type CollectorNumberGroup =
    | Colorless = 1
    | White = 2
    | Blue = 3
    | Black = 4
    | Red = 5
    | Green = 6
    | Multi = 7
    | Hybrid = 8
    | Artifact = 9
    | Land = 10
    | Token = 11

type Credentials = {
    Email : string
    Password : string
}

type SetDirectory = {
    Path : string
    HtmlLayout : string
    JsonDetails : string
    CenterFixes : string
    CardImage : string -> string
}
module SetDirectory =
    let getCardFileName (cardName: string) =
        cardName.Replace(" ", "-").Replace("?", "-") + ".jpg"

    let create (rootDir: string) (name: string) : SetDirectory =
        let path = Path.Combine(rootDir, name)
        {
            Path = path
            HtmlLayout = Path.Combine(path, "layout.html")
            JsonDetails = Path.Combine(path, "details.json")
            CenterFixes = Path.Combine(path, "center-fixes.txt")
            CardImage = (fun name -> Path.Combine(path, getCardFileName name))
        }

type WorkspaceDirectory = {
    Path : string
    Cookie : string
    Credentials : string
    Set : string -> SetDirectory
}
module WorkspaceDirectory =
    let create (rootDir: string) : WorkspaceDirectory =
        {
            Path = rootDir
            Cookie = Path.Combine(rootDir, "cookie.json")
            Credentials = Path.Combine(rootDir, "credentials.json")
            Set = SetDirectory.create rootDir
        }

/// <summary> Dependencies that are needed all over. </summary>
type Context = {
    Logger : ILogger
    Http : HttpClient
    Cookie : KeyValuePair<string, string>
    Workspace : WorkspaceDirectory
}
with
    member this.Log (msg: string, ?level: LogEventLevel) =
        let lvl = level |> Option.defaultValue LogEventLevel.Information
        this.Logger.Write(lvl, msg)
