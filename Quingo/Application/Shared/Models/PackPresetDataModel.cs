using Quingo.Shared.Entities;

namespace Quingo.Application.Shared.Models;

public class PackPresetDataModel
{
    public PackPresetDataModel()
    {

    }

    public PackPresetDataModel(PackPresetData data)
    {
        CardSize = data.CardSize;
        FreeCenter = data.FreeCenter;
        Columns = data.Columns.Count > 0 ? data.Columns.Select(c => new PackPresetColumnModel(c)).ToList() : Columns;
        LivesNumber = data.LivesNumber;
        EndgameTimer = data.EndgameTimer;
        ShowTagBadges = data.ShowTagBadges;
        Pattern = data.Pattern;
        MatchRule = data.MatchRule;

        Columns.MatchListSize(data.CardSize, () => new PackPresetColumnModel());
    }

    public PackPresetData ToData()
    {
        return new PackPresetData
        {
            CardSize = CardSize,
            FreeCenter = FreeCenter,
            LivesNumber = LivesNumber,
            EndgameTimer = EndgameTimer,
            ShowTagBadges = ShowTagBadges,
            Pattern = Pattern,
            MatchRule = MatchRule,
            Columns = Columns.Select(c => new PackPresetColumn
            {
                Name = c.Name,
                QuestionTags = c.QuestionTags.ToList(),
                AnswerTags = c.AnswerTags.ToList(),
                ExcludeTags = c.ExcludeTags.ToList(),
            }).ToList()
        };
    }

    public int CardSize { get; set; } = 5;

    public bool FreeCenter { get; set; } = true;

    public bool IsFreeCenterEnabled => CardSize % 2 != 0;

    public int LivesNumber { get; set; } = 3;

    public int EndgameTimer { get; set; } = 20;

    public bool ShowTagBadges { get; set; } = true;

    public PackPresetPattern Pattern { get; set; } = PackPresetPattern.Lines;

    public PackPresetMatchRule MatchRule { get; set; } = PackPresetMatchRule.Default;

    public List<PackPresetColumnModel> Columns { get; set; } =
    [
        new PackPresetColumnModel("B"),
        new PackPresetColumnModel("I"),
        new PackPresetColumnModel("N"),
        new PackPresetColumnModel("G"),
        new PackPresetColumnModel("O")
    ];
}

public class PackPresetColumnModel
{
    public PackPresetColumnModel()
    {

    }

    public PackPresetColumnModel(string name)
    {
        Name = name;
    }

    public PackPresetColumnModel(PackPresetColumn col)
    {
        Name = col.Name;
        QuestionTags = new List<int>(col.QuestionTags ?? []);
        AnswerTags = new List<int>(col.AnswerTags ?? []);
        ExcludeTags = new List<int>(col.ExcludeTags ?? []);
    }

    public string Name { get; set; } = "";

    public IEnumerable<int> QuestionTags { get; set; } = [];

    public IEnumerable<int> AnswerTags { get; set; } = [];

    public IEnumerable<int> ExcludeTags { get; set; } = [];
}
