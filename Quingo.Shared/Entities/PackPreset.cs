namespace Quingo.Shared.Entities;

public class PackPreset : EntityBase, IPackOwned
{
    public int PackId { get; set; }

    public Pack Pack { get; set; } = default!;

    public PackPresetData Data { get; set; } = new();
}

public record PackPresetData
{
    public int CardSize { get; set; } = 5;

    public bool FreeCenter { get; set; } = true;

    public int LivesNumber { get; set; } = 3;

    public int MaxPlayers { get; set; } = 0;
    
    public int GameTimer { get; set; } = 0;

    public int EndgameTimer { get; set; } = 20;

    public int AutoDrawTimer { get; set; }

    public bool SeparateDrawPerPlayer { get; set; }

    public bool SamePlayerCards { get; set; }
    
    public bool ShowTagBadges { get; set; } = true;

    public bool EnableCall { get; set; } = true;

    public bool JoinOnCreate { get; set; } = true;

    public PackPresetPattern Pattern { get; set; }

    public PackPresetMatchRule MatchRule { get; set; }

    public PackPresetScoringRules ScoringRules { get; set; }

    public int? MinDifficulty { get; set; }
    
    public int? MaxDifficulty { get; set; }
    
    public bool SingleColumnConfig { get; set; }
    
    public List<PackPresetColumn> Columns { get; set; } = [];
}

public record PackPresetColumn
{
    public string Name { get; set; } = "";

    public IList<int> QuestionTags { get; set; } = [];

    public IList<PackPresetTag> ColAnswerTags { get; set; } = [];

    public IList<int> ExcludeTags { get; set; } = [];
}

public record PackPresetTag
{
    public int TagId { get; set; }

    public int? ItemsMin { get; set; }

    public int? ItemsMax { get; set; }
}

public enum PackPresetPattern
{
    Lines,
    FullCard
}

public enum PackPresetMatchRule
{
    Default,
    LastDrawn
}

[Flags]
public enum PackPresetScoringRules
{
    None = 0,
    CellScore = 1 << 0,
    PatternBonus = 1 << 1,
    TimeBonus = 1 << 2,
    ErrorPenalty = 1 << 3,
    DrawPenalty = 1 << 4,
    CustomCellScore = 1 << 5,
}

public static class ScoringRulesExtensions
{
    public static bool HasFlag(this PackPresetScoringRules value, PackPresetScoringRules flag)
    {
        return (value & flag) != 0;
    }
    
    public static string GetName(this PackPresetScoringRules value) => value switch
    {
        PackPresetScoringRules.None => "None",
        PackPresetScoringRules.CellScore => "Cell Score",
        PackPresetScoringRules.PatternBonus => "Line Bonus",
        PackPresetScoringRules.TimeBonus => "Time Bonus",
        PackPresetScoringRules.ErrorPenalty => "Error Penalty",
        PackPresetScoringRules.DrawPenalty => "Draw Penalty",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };

    public static IEnumerable<PackPresetScoringRules> ToEnumerable(this PackPresetScoringRules value)
    {
        return Enum.GetValues<PackPresetScoringRules>()
            .Where(x => x != PackPresetScoringRules.None && value.HasFlag(x));
    }

    public static PackPresetScoringRules ToFlagsEnum(this IEnumerable<PackPresetScoringRules> value)
    {
        return value
            .Aggregate(PackPresetScoringRules.None, (a, b) => a | b);
    }
    
}


