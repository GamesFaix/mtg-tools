using System;
using System.Collections.Generic;
using System.Linq;

namespace ShandalarToCockatrice
{
    static class Validator
    {
        public static IEnumerable<string> Validate(Deck deck)
        {
            if (string.IsNullOrWhiteSpace(deck.Name)) throw new Exception("Deck name cannot be blank.");

            var duplicates = deck.Cards.GroupBy(Mapper.GetKey).Where(grp => grp.Count() > 1);
            if (duplicates.Any()) yield return $"The deck {deck.Name} has duplicate listings for {duplicates.Select(grp => grp.First()).ToListString()}";

            duplicates = deck.Sideboard.GroupBy(Mapper.GetKey).Where(grp => grp.Count() > 1);
            if (duplicates.Any()) yield return $"The sideboard of {deck.Name} has duplicate listings for {duplicates.Select(grp => grp.First()).ToListString()}";

            var deckWithSideboard = deck.Cards.Concat(deck.Sideboard).ConsolidateDuplicates(Enumerable.Sum).ToArray();

            var lessThan1 = deckWithSideboard.Where(x => x.Count < 1);
            if (lessThan1.Any()) yield return $"The deck {deck.Name} has less than 1 of {lessThan1.ToListString()}";

            var moreThan4 = deckWithSideboard.Where(x => x.Count > 4 && !CardInfo.IsBasicLand(x));
            if (moreThan4.Any()) yield return $"The deck {deck.Name} has more than 4 of {moreThan4.ToListString()}";

            var count = deck.Cards.Sum(x => x.Count);
            if (count < 60) yield return $"The deck {deck.Name} has less than 60 cards.";

            var sideboardCount = deck.Sideboard.Sum(x => x.Count);
            if (sideboardCount > 15) yield return $"The sideboard of {deck.Name} has more than 15 cards.";
        }

        public static string ToListString(this IEnumerable<DeckItem> items) =>
            string.Join(", ", items.Select(x => x.Name));

    }
}
