namespace ShandalarToCockatrice
{
    public class DeckItem
    {
        public string Name;
        public int Count;

        public override string ToString() =>
            $"{Count} {Name}";
    }

    public class Deck
    {
        public string Name;
        public string Comments;
        public DeckItem[] Cards;
        public DeckItem[] Sideboard;
    }

    public class ShandalarDeckModel
    {
        public string Name;
        public DeckItem[] Core;
        public DeckItem[] DefaultExtension;
        public DeckItem[] BlackExtension;
        public DeckItem[] BlueExtension;
        public DeckItem[] GreenExtension;
        public DeckItem[] RedExtension;
        public DeckItem[] WhiteExtension;
    }
}
