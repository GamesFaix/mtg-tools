using System;
using System.Linq;

namespace ShandalarToCockatrice
{
    public static class CardInfo
    {
        public static bool IsBasicLand(this DeckItem item) =>
            _basicLandNames.Contains(Mapper.GetKey(item));

        static readonly string[] _basicLandNames = new[]
        {
            "plains",
            "island",
            "swamp",
            "mountain",
            "forest",
            "snow-covered plains",
            "snow-covered island",
            "snow-covered swamp",
            "snow-covered mountain",
            "snow-covered forest",
            "wastes"
        };

        public static bool IsAstral(this DeckItem item) =>
            _astralCardNames.Contains(Mapper.GetKey(item));

        static readonly string[] _astralCardNames = new[]
        {
            "aswan jaguar",
            "call from the grave",
            "faerie dragon",
            "gem bazaar",
            "goblin polka band",
            "necropolis of azar",
            "orcish catapult",
            "pandora's box",
            "power struggle",
            "prismatic dragon",
            "rainbow knights",
            "whimsy"
        };

        public static bool IsAnte(this DeckItem item) =>
            _anteCardNames.Contains(Mapper.GetKey(item));

        static readonly string[] _anteCardNames = new[]
        {
            "amulet of quoz",
            "bronze tablet",
            "contract from below",
            "darkpact",
            "demonic tutor",
            "jeweled bird",
            "rebirth",
            "tempest efreet",
            "timmerian fiends"
        };

    }
}
