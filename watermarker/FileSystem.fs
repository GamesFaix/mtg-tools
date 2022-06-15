module FileSystem

open Model

let private workingDir = "c:/github/jamesfaix/eccentria/bib/watermarks"

let private escapeSetCode = function
    | "con" -> "conflux" // Can't call a file 'con' on Windows
    | x -> x
    
let svgPath (code: string) =
    $"{workingDir}/{escapeSetCode code}.svg"

let maskPath (code: string) =
    $"{workingDir}/{escapeSetCode code}-mask.png"

let serialize = function
    | White -> "white"
    | Blue -> "blue"
    | Black -> "black"
    | Red -> "red"
    | Green -> "green"
    | WhiteBlue -> "wu"
    | BlueBlack -> "ub"
    | BlackRed -> "br"
    | RedGreen -> "rg"
    | GreenWhite -> "gw"
    | WhiteBlack -> "wb"
    | BlueRed -> "ur"
    | BlackGreen -> "bg"
    | RedWhite -> "rw"
    | GreenBlue -> "gu"
    | Colorless -> "colorless"
    | Gold -> "gold"
    | LandColorless -> "land-colorless"

let backgroundPath (color: WatermarkColor) =
    $"{workingDir}/background-{serialize color}.png"

let watermarkPath (code: string) (color: WatermarkColor) =
    $"{workingDir}/{escapeSetCode code}-{serialize color}.png"

let scryfallSetsDataPath () =
    $"{workingDir}/data/scryfall-sets.json"

let scryfallCardsDataPath () =
    $"{workingDir}/data/scryfall-cards.json"

let mtgdCardsDataPath () =
    $"{workingDir}/data/mtgd-cards.json"