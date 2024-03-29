﻿module GamesFaix.MtgTools.Designer.Model

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
module CardDetails =
    let toInfo (details: CardDetails) : CardInfo =
        {
            Id = details.Id
            Name = details.Name
            Set = details.Set
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
    | Artifact = 8
    | Land = 9
    | Token = 10

type NumberFix = {
    name: string
    backOf: string
}