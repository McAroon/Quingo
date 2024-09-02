namespace Quingo.Shared.Entities;

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

    public List<PackPresetColumn> Columns { get; set; } = [];
}

public record PackPresetColumn
{
    public string Name { get; set; } = "";

    public IList<int> QuestionTags { get; set; } = [];

    public IList<int> AnswerTags { get; set; } = [];
}


