﻿namespace Quingo.Shared.Entities;

public class PackPreset : EntityBase
{
    public int PackId { get; set; }

    public Pack Pack { get; set; } = default!;

    public PackPresetData Data { get; set; } = new PackPresetData();
}

public record PackPresetData
{
    public int CardSize { get; set; } = 5;

    public bool FreeCenter { get; set; } = true;

    public int LivesNumber { get; set; } = 3;

    public int EndgameTimer { get; set; } = 20;

    public bool ShowTagBadges { get; set; } = true;

    public PackPresetPattern Pattern { get; set; }

    public PackPresetMatchRule MatchRule { get; set; }
    
    public bool SingleColumnConfig { get; set; }

    public List<PackPresetColumn> Columns { get; set; } = [];
}

public record PackPresetColumn
{
    public string Name { get; set; } = "";

    public IList<int> QuestionTags { get; set; } = [];

    public IList<int> AnswerTags { get; set; } = [];

    public IList<int> ExcludeTags { get; set; } = [];
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


