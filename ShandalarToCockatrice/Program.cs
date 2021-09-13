using System;
using System.Collections.Generic;
using System.IO;

namespace ShandalarToCockatrice
{
    static class Program
    {
        static void Main()
        {
            var sourceDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "MagicTG/Decks");

            var targetDir = Path.GetFullPath(
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "../../../../../Microprose"));

            var allIssues = new List<string>();

            var allFiles = Directory.GetFiles(sourceDir);

            foreach (var f in allFiles)
            {
                Console.WriteLine($"Processing {f}");

                var shandalarDeck = Parser.ParseDeck(f);

                var deck = Mapper.MapDeck(shandalarDeck);

                var issues = Validator.Validate(deck);
                allIssues.AddRange(issues);

                var targetPath = Path.Combine(targetDir, $"{deck.Name}.cod");
                Writer.WriteDeck(targetPath, deck);
            }

            foreach (var issue in allIssues)
            {
                Console.WriteLine(issue);
            }

            Console.WriteLine("Done");
            Console.Read();
        }
    }
}
