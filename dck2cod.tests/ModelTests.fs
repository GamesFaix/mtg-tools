module GamesFaix.MtgTools.Dck2Cod.Tests.ModelTests

open Xunit
open GamesFaix.MtgTools.Dck2Cod

module DeckTests =

    [<Fact>]
    let ``fromShandalar should map deck`` () =
        // Arrange
        let shandalarDeck = "LordOfFate" |> Data.readDckFile |> Dck.parse

        let expected : Model.Deck = {
            Name = "Lord of Fate"
            Cards = [
                { Count = 11; Name = "Plains" }
                { Count = 11; Name = "Swamp" }
                { Count = 2; Name = "Dark Ritual" }
                { Count = 3; Name = "Castle" }
                { Count = 3; Name = "Healing Salve" }
                { Count = 3; Name = "Holy Strength" }
                { Count = 3; Name = "Unholy Strength" }
                { Count = 4; Name = "Yotian Soldier" } // Note: typo corrected
                { Count = 4; Name = "Serra Angel" }
                { Count = 3; Name = "Osai Vultures" }
                { Count = 3; Name = "Pestilence" }
                { Count = 2; Name = "Necropolis of Azaar" }
                { Count = 3; Name = "Drudge Skeletons" }
                { Count = 2; Name = "Divine Transformation" }
                { Count = 3; Name = "Ornithopter" }
            ]
            Sideboard = [
                { Count = 3; Name = "Brass Man" }
                { Count = 3; Name = "Animate Dead" }
                { Count = 3; Name = "Amulet of Kroog" }
            ]
            Comments = ""
        }

        // Act
        let actual = Model.Deck.fromShandalar shandalarDeck

        // Assert
        Assert.Equal(expected, actual)

    [<Fact>]
    let ``fromShandalar should add comment about astral and ante cards`` () =
        // Arrange
        let shandalarDeck : Model.ShandalarDeck = {
            Name = "Test"
            Core = [
                { Count = 1; Name = "Gem Bazaar" }
                { Count = 1; Name = "Jeweled Bird" }
            ]
            DefaultExtension = []
            BlackExtension = []
            BlueExtension = []
            GreenExtension = []
            RedExtension = []
            WhiteExtension = []
        }

        let expected : Model.Deck = {
            Name = "Test"
            Cards = [
                { Count = 1; Name = "Gem Bazaar" }
                { Count = 1; Name = "Jeweled Bird" }
            ]
            Sideboard = []
            Comments = "Contains these Astral cards: Gem Bazaar. Contains these ante cards: Jeweled Bird. "
        }

        // Act
        let actual = Model.Deck.fromShandalar shandalarDeck

        // Assert
        Assert.Equal(expected, actual)