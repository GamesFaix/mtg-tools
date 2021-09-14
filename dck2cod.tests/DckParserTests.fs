module GamesFaix.MtgTools.Dck2Cod.DckParserTests

open Xunit
open GamesFaix.MtgTools.Dck2Cod

[<Fact>]
let ``parseDeck should parse .dck file with sideboard`` () =
    // Arrange
    let path = "./TestData/LordOfFate.dck"

    let expected : Model.ShandalarDeck = {
        Name = "Lord of Fate"
        Core = [
            { Count = 11; Name = "Plains" }
            { Count = 11; Name = "Swamp" }
            { Count = 2; Name = "Dark Ritual" }
            { Count = 3; Name = "Castle" }
            { Count = 3; Name = "Healing Salve" }
            { Count = 3; Name = "Holy Strength" }
            { Count = 3; Name = "Unholy Strength" }
            { Count = 4; Name = "Yotian Soldiers" } // Note: typo preserved by parser
            { Count = 4; Name = "Serra Angel" }
            { Count = 3; Name = "Osai Vultures" }
            { Count = 3; Name = "Pestilence" }
            { Count = 2; Name = "Necropolis of Azaar" }
            { Count = 3; Name = "Drudge Skeletons" }
            { Count = 2; Name = "Divine Transformation" }
        ]
        DefaultExtension= [
            { Count = 3; Name = "Ornithopter" }
        ]
        BlackExtension = [
            { Count = 3; Name = "Brass Man" }
        ]
        BlueExtension = [
            { Count = 3; Name = "Ornithopter" }
        ]
        GreenExtension = [
            { Count = 3; Name = "Animate Dead" }
        ]
        RedExtension = [
            { Count = 3; Name = "Amulet of Kroog" }
        ]
        WhiteExtension = [
            { Count = 3; Name = "Brass Man" }
        ]
    }

    // Act
    let actual = DckParser.parseDeck path

    // Assert
    Assert.Equal(expected, actual)

[<Fact>]
let ``parseDeck should parse .dck file without sideboard`` () =
    // Arrange
    let path = "./TestData/Seer.dck"

    let expected : Model.ShandalarDeck = {
        Name = "Seer"
        Core = [
            { Count = 22; Name = "Island" }
            { Count = 3; Name = "Drain Power" }
            { Count = 4; Name = "Crystal Rod" }
            { Count = 2; Name = "Counterspell" }
            { Count = 4; Name = "Unsummon" }
            { Count = 4; Name = "Zephyr Falcon" }
            { Count = 4; Name = "Ghost Ship" }
            { Count = 4; Name = "Unstable Mutation" }
            { Count = 2; Name = "Time Elemental" }
            { Count = 3; Name = "Tetravus" }
            { Count = 3; Name = "Triskelion" }
            { Count = 2; Name = "Hurkyl's Recall" }
            { Count = 3; Name = "Ornithopter" }
        ]
        DefaultExtension= []
        BlackExtension = []
        BlueExtension = []
        GreenExtension = []
        RedExtension = []
        WhiteExtension = []
    }

    // Act
    let actual = DckParser.parseDeck path

    // Assert
    Assert.Equal(expected, actual)
