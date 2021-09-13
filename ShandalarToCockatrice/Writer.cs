using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ShandalarToCockatrice
{
    static class Writer
    {
        public static void WriteDeck(string path, Deck deck)
        {
            var doc = deck.ToDocument();
            using var stream = File.Open(path, FileMode.Create);
            doc.Save(stream);
        }

        static XDocument ToDocument(this Deck deck)
        {
            var children = new List<XElement>
            {
                new XElement("deckname", deck.Name),
                new XElement("comments", deck.Comments),
                new XElement("zone", new XAttribute("name", "main"), deck.Cards.Select(ToElement))
            };

            if (deck.Sideboard.Length > 0)
            {
                children.Add(
                    new XElement("zone", new XAttribute("name", "side"), deck.Sideboard.Select(ToElement)));
            }

            return new XDocument(
                new XElement("cockatrice_deck", new XAttribute("version", "1"), children)
            );
        }

        static XElement ToElement(this DeckItem item) =>
            new XElement("card",
                new XAttribute("number", item.Count),
                new XAttribute("name", item.Name)
            );
    }
}
