module GamesFaix.MtgTools.Dck2Cod.CardInfo

open GamesFaix.MtgTools.Dck2Cod.Model

let private basicLandNames = [
    "plains"
    "island"
    "swamp"
    "mountain"
    "forest"
    "snow-covered plains"
    "snow-covered island"
    "snow-covered swamp"
    "snow-covered mountain"
    "snow-covered forest"
    "wastes"
]

let private astralCardNames = [
    "aswan jaguar"
    "call from the grave"
    "faerie dragon"
    "gem bazaar"
    "goblin polka band"
    "necropolis of azar"
    "orcish catapult"
    "pandora's box"
    "power struggle"
    "prismatic dragon"
    "rainbow knights"
    "whimsy"
]

let private anteCardNames = [
    "amulet of quoz"
    "bronze tablet"
    "contract from below"
    "darkpact"
    "demonic attorney"
    "jeweled bird"
    "rebirth"
    "tempest efreet"
    "timmerian fiends"
]

let isBasicLand (item: DeckItem) : bool =
    basicLandNames |> List.contains item.Key

let isAstral (item: DeckItem) : bool =
    astralCardNames |> List.contains item.Key

let isAnte (item: DeckItem) : bool =
    anteCardNames |> List.contains item.Key
