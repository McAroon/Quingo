using Microsoft.EntityFrameworkCore;
using Quingo.Infrastructure.Database;
using Quingo.Shared.Entities;

namespace Quingo.Scripts;

public class GenerateStandardBingo
{
    private readonly ApplicationDbContext _context;

    public GenerateStandardBingo(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Execute()
    {
        var existing = await _context.Packs.FirstOrDefaultAsync(x => x.Name == "Bingo 75");
        if (existing != null) return;
        
        var pack = new Pack
        {
            Name = "Bingo 75",
            Description = "Classic bingo",
            Tags =
            [
                new("B"),
                new("I"),
                new("N"),
                new("G"),
                new("O"),
                new("AB"),
                new("AI"),
                new("AN"),
                new("AG"),
                new("AO"),
            ],
            NodeLinkTypes = [new() {Name = "default"}],
            IsPublished = true,
        };

        _context.Add(pack);
        await _context.SaveChangesAsync();

        var preset = new PackPreset
        {
            Data = new PackPresetData
            {
                CardSize = 5,
                FreeCenter = true,
                LivesNumber = 3,
                MaxPlayers = 0,
                GameTimer = 0,
                EndgameTimer = 20,
                AutoDrawTimer = 0,
                SeparateDrawPerPlayer = false,
                SamePlayerCards = false,
                ShowTagBadges = false,
                EnableCall = true,
                Pattern = PackPresetPattern.Lines,
                MatchRule = PackPresetMatchRule.Default,
                ScoringRules = PackPresetScoringRules.CellScore | PackPresetScoringRules.PatternBonus | PackPresetScoringRules.ErrorPenalty,
                JoinOnCreate = true,
                SingleColumnConfig = false,
                Columns =
                [
                    CreatePresetColumn("B", pack),
                    CreatePresetColumn("I", pack),
                    CreatePresetColumn("N", pack),
                    CreatePresetColumn("G", pack),
                    CreatePresetColumn("O", pack),
                ]
            }
        };
        
        pack.Presets = [preset];
        await _context.SaveChangesAsync();

        for (int i = 1; i <= 75; i++)
        {
            var letter = NumberToLetter(i);

            var question = new Node 
            { 
                Pack = pack, 
                Name = $"{i}.",
                NodeTags = 
                { 
                    new NodeTag 
                    { 
                        Tag = pack.Tags.First(t => t.Name == letter)
                    } 
                }
            };

            _context.Add(question);

            var answer = new Node
            {
                Pack = pack,
                Name = $"{i}",
                NodeTags =
                {
                    new NodeTag
                    {
                        Tag = pack.Tags.First(t => t.Name == $"A{letter}")
                    }
                }
            };

            _context.Add(answer);

            var link = new NodeLink 
            { 
                NodeLinkType = pack.NodeLinkTypes.First(),
                NodeFrom = question,
                NodeTo = answer
            };

            _context.Add(link);
        }

        await _context.SaveChangesAsync();
    }

    private static string NumberToLetter(int num)
    {
        return num switch
        {
            var n when n >= 1 && n <= 15 => "B",
            var n when n >= 16 && n <= 30 => "I",
            var n when n >= 31 && n <= 45 => "N",
            var n when n >= 46 && n <= 60 => "G",
            var n when n >= 61 && n <= 75 => "O",
            _ => throw new ArgumentOutOfRangeException(nameof(num))
        };
    }

    private PackPresetColumn CreatePresetColumn(string name, Pack pack)
    {
        return new PackPresetColumn
        {
            Name = name,
            QuestionTags = [pack.Tags.First(x => x.Name == name).Id],
            ColAnswerTags =
            [
                new PackPresetTag
                {
                    TagId = pack.Tags.First(x => x.Name == $"A{name}").Id
                }
            ]
        };
    }
}
