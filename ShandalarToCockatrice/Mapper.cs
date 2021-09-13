using System;
using System.Collections.Generic;
using System.Linq;

namespace ShandalarToCockatrice
{
    static class Mapper
    {
        public static Deck MapDeck(ShandalarDeckModel shandalarDeck)
        {
            var cards = shandalarDeck.Core
                .Concat(shandalarDeck.DefaultExtension)
                .ConsolidateDuplicates(Enumerable.Sum) // Sum count of cards in core and default extension
                .FixBugs()
                .ToArray();

            var sideboard = shandalarDeck.BlackExtension
                .Concat(shandalarDeck.BlueExtension)
                .Concat(shandalarDeck.GreenExtension)
                .Concat(shandalarDeck.RedExtension)
                .Concat(shandalarDeck.WhiteExtension)
                .ConsolidateDuplicates(Enumerable.Max) // Take max count of each card used in any color extension
                .Subtract(shandalarDeck.DefaultExtension) // Subtract count from default extension
                .FixBugs()
                .ToArray();

            var d = new Deck
            {
                Name = shandalarDeck.Name,
                Comments = "",
                Cards = cards,
                Sideboard = sideboard
            };

            var astralCards = cards.Concat(sideboard).Where(CardInfo.IsAstral);
            if (astralCards.Any())
            {
                d.Comments += $"Contains these Astral cards: {astralCards.ToListString()}. ";
            }

            var anteCards = cards.Concat(sideboard).Where(CardInfo.IsAnte);
            if (anteCards.Any())
            {
                d.Comments += $"Contains these ante cards: {anteCards.ToListString()}. ";
            }

            return d;
        }

        public static string GetKey(this DeckItem item) => item.Name.ToLowerInvariant();

        public static IEnumerable<DeckItem> ConsolidateDuplicates(
            this IEnumerable<DeckItem> items,
            Func<IEnumerable<DeckItem>, Func<DeckItem, int>, int> consolidate)
        {
            return items
                .GroupBy(GetKey)
                .Select(grp => new DeckItem
                {
                    Name = grp.First().Name,
                    Count = consolidate(grp, item => item.Count)
                });
        }

        static IEnumerable<DeckItem> Subtract(
            this IEnumerable<DeckItem> items,
            IEnumerable<DeckItem> toSubtract)
        {
            var toSubtractCache = toSubtract.ToDictionary(GetKey);

            return items
                .Select(item =>
                {
                    var count = toSubtractCache.TryGetValue(GetKey(item), out var toSubtractItem)
                        ? item.Count - toSubtractItem.Count
                        : item.Count;

                    return new DeckItem
                    {
                        Name = item.Name,
                        Count = count
                    };
                })
                .Where(x => x.Count > 0);
        }

        static IEnumerable<DeckItem> FixBugs(
            this IEnumerable<DeckItem> items)
        {
            foreach (var item in items)
            {
                if (GetKey(item) == "will-o-the-wisp")
                    item.Name = "Will-o'-the-Wisp";

                if (GetKey(item) == "manaflare")
                    item.Name = "Mana Flare";

                yield return item;
            }
        }
    }
}
