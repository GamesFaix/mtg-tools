using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ShandalarToCockatrice
{
    static class Parser
    {
        public static ShandalarDeckModel ParseDeck(string path)
        {
            var lines = File.ReadAllLines(path);

            var titleLine = lines[0];

            var coreLines = lines
                .Skip(2) // Title and blank line
                .TakeWhile(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();

            var extensionLines = lines
                .Skip(2 + coreLines.Length + 1) // Title, blank, core, blank
                .TakeWhile(line => !string.IsNullOrWhiteSpace(line)); // Avoid trailing blank line

            var skipLength = 1; // vNone header
            var basicExtensionLines = extensionLines
                .Skip(skipLength)
                .TakeWhile(line => Regex.IsMatch(line, @"\d"))
                .ToArray();

            skipLength += basicExtensionLines.Length + 1;
            var blackExtensionLines = extensionLines
                .Skip(skipLength)
                .TakeWhile(line => Regex.IsMatch(line, @"\d"))
                .ToArray();

            skipLength += basicExtensionLines.Length + 1;
            var blueExtensionLines = extensionLines
                .Skip(skipLength)
                .TakeWhile(line => Regex.IsMatch(line, @"\d"))
                .ToArray();

            skipLength += basicExtensionLines.Length + 1;
            var greenExtensionLines = extensionLines
                .Skip(skipLength)
                .TakeWhile(line => Regex.IsMatch(line, @"\d"))
                .ToArray();

            skipLength += basicExtensionLines.Length + 1;
            var redExtensionLines = extensionLines
                .Skip(skipLength)
                .TakeWhile(line => Regex.IsMatch(line, @"\d"))
                .ToArray();

            skipLength += basicExtensionLines.Length + 1;
            var whiteExtensionLines = extensionLines
                .Skip(skipLength)
                .TakeWhile(line => Regex.IsMatch(line, @"\d"))
                .ToArray();

            return new ShandalarDeckModel
            {
                Name = ParseName(titleLine),
                Core = coreLines.Select(ParseDeckItem).ToArray(),
                DefaultExtension = basicExtensionLines.Select(ParseDeckItem).ToArray(),
                BlackExtension = blackExtensionLines.Select(ParseDeckItem).ToArray(),
                BlueExtension = blueExtensionLines.Select(ParseDeckItem).ToArray(),
                GreenExtension = greenExtensionLines.Select(ParseDeckItem).ToArray(),
                RedExtension = redExtensionLines.Select(ParseDeckItem).ToArray(),
                WhiteExtension = whiteExtensionLines.Select(ParseDeckItem).ToArray(),
            };
        }

        static string ParseName(string line)
        {
            var pattern = @"([\w ]+).*";
            var match = Regex.Match(line, pattern);
            if (!match.Success) throw new FormatException();
            return match.Groups[1].Captures[0].Value.Trim();
        }

        static DeckItem ParseDeckItem(string line)
        {
            var pattern = @"\.\d+\s+(\d+)\s+(.*)";
            var match = Regex.Match(line, pattern);
            if (!match.Success) throw new FormatException();

            return new DeckItem
            {
                Name = match.Groups[2].Captures[0].Value.Trim(),
                Count = int.Parse(match.Groups[1].Captures[0].Value)
            };
        }
    }
}
