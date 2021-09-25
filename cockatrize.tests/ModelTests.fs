module GamesFaix.MtgTools.Cockatrize.Tests.ModelTests

open Xunit
open GamesFaix.MtgTools.Cockatrize

module DeckTests =

    [<Fact>]
    let ``fromDck should map deck`` () =
        // Arrange
        let dckDeck = "LordOfFate" |> Data.readDckFile |> Dck.parse

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
        let actual = Model.Deck.fromDck dckDeck

        // Assert
        Assert.Equal(expected, actual)

    [<Fact>]
    let ``fromDck should add comment about astral and ante cards`` () =
        // Arrange
        let dckDeck : Dck.DckDeck = {
            Name = "Test"
            Description = ""
            Cards = [
                { Id = 1; Count = 1; Name = "Gem Bazaar" }
                { Id = 1; Count = 1; Name = "Jeweled Bird" }
            ]
            Extensions = []
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
        let actual = Model.Deck.fromDck dckDeck

        // Assert
        Assert.Equal(expected, actual)