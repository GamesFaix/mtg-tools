module GamesFaix.MtgTools.Cockatrize.Tests.DckTests

open Xunit
open GamesFaix.MtgTools.Cockatrize

[<Fact>]
let ``Line.tryParseTitle should work`` () =
    let line = "Queltosh (U/W/R, 4th)"
    let title : Dck.DckTitle = {
        Name = "Queltosh"
        Description = "U/W/R, 4th"
    }
    let expected = (true, Some title)
    let actual = Dck.Line.tryParseTitle line
    Assert.Equal(expected, actual)

[<Fact>]
let ``Line.tryParseTitle should work with no description`` () =
    let line = "Queltosh"
    let title : Dck.DckTitle = {
        Name = "Queltosh"
        Description = ""
    }
    let expected = (true, Some title)
    let actual = Dck.Line.tryParseTitle line
    Assert.Equal(expected, actual)

[<Fact>]
let ``Line.tryParseTitle should not work on cards`` () =
    let line = ".221 1 Serra Angel"
    let expected = (false, None)
    let actual = Dck.Line.tryParseTitle line
    Assert.Equal(expected, actual)

[<Fact>]
let ``Line.tryParseSectionHeader should work`` () =
    let line = ".vGreen"
    let expected = (true, Some "Green")
    let actual = Dck.Line.tryParseSectionHeader line
    Assert.Equal(expected, actual)

[<Fact>]
let ``Line.tryParseCard should work`` () =
    let line = ".221	1	Serra Angel"
    let title : Dck.DckCard = {
        Name = "Serra Angel"
        Id = 221
        Count = 1
    }
    let expected = (true, Some title)
    let actual = Dck.Line.tryParseCard line
    Assert.Equal(expected, actual)

[<Fact>]
let ``parse should parse .dck file without sideboard`` () =
    // Arrange
    let text = Data.readDckFile "Seer"

    let expected : Dck.DckDeck = {
        Name = "Seer"
        Description = "Ub, 4th"
        Cards = [
            { Id = 126; Count = 22; Name = "Island" }
            { Id =  69; Count =  3; Name = "Drain Power" }
            { Id =  52; Count =  4; Name = "Crystal Rod" }
            { Id =  48; Count =  2; Name = "Counterspell" }
            { Id = 260; Count =  4; Name = "Unsummon" }
            { Id = 859; Count =  4; Name = "Zephyr Falcon" }
            { Id = 326; Count =  4; Name = "Ghost Ship" }
            { Id = 462; Count =  4; Name = "Unstable Mutation" }
            { Id = 817; Count =  2; Name = "Time Elemental" }
            { Id = 534; Count =  3; Name = "Tetravus" }
            { Id = 538; Count =  3; Name = "Triskelion" }
            { Id = 502; Count =  2; Name = "Hurkyl's Recall" }
            { Id = 514; Count =  3; Name = "Ornithopter" }
        ]
        Extensions = [
        ]
    }

    // Act
    let actual = Dck.parse text

    // Assert
    Assert.Equal(expected, actual)

[<Fact>]
let ``parse should parse .dck file with sideboard`` () =
    // Arrange
    let text = Data.readDckFile "LordOfFate"

    let expected : Dck.DckDeck = {
        Name = "Lord of Fate"
        Description = "Bl/Wh, 4th Edition"
        Cards = [
            { Id = 188; Count = 11; Name = "Plains" }
            { Id = 239; Count = 11; Name = "Swamp" }
            { Id =  55; Count =  2; Name = "Dark Ritual" }
            { Id =  28; Count =  3; Name = "Castle" }
            { Id = 108; Count =  3; Name = "Healing Salve" }
            { Id = 112; Count =  3; Name = "Holy Strength" }
            { Id = 259; Count =  3; Name = "Unholy Strength" }
            { Id = 550; Count =  4; Name = "Yotian Soldiers" } // Note: typo preserved by parser
            { Id = 221; Count =  4; Name = "Serra Angel" }
            { Id = 736; Count =  3; Name = "Osai Vultures" }
            { Id = 182; Count =  3; Name = "Pestilence" }
            { Id = 871; Count =  2; Name = "Necropolis of Azaar" }
            { Id =  70; Count =  3; Name = "Drudge Skeletons" }
            { Id = 616; Count =  2; Name = "Divine Transformation" }
        ]
        Extensions = [
            { Name = "None"; Cards = [
                { Id = 514; Count = 3; Name = "Ornithopter" }
            ]}
            { Name = "Black"; Cards = [
                { Id = 930; Count = 3; Name = "Brass Man" }
            ]}
            { Name = "Blue"; Cards = [
                { Id = 514; Count = 3; Name = "Ornithopter" }
            ]}
            { Name = "Green"; Cards = [
                { Id = 3; Count = 3; Name = "Animate Dead" }
            ]}
            { Name = "Red"; Cards = [
                { Id = 466; Count = 3; Name = "Amulet of Kroog" }
            ]}
            { Name = "White"; Cards = [
                { Id = 930; Count = 3; Name = "Brass Man" }
            ]}
        ]
    }

    // Act
    let actual = Dck.parse text

    // Assert
    Assert.Equal(expected, actual)
