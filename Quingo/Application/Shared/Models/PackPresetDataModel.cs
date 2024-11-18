﻿using Quingo.Shared.Entities;

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
        MaxPlayers = data.MaxPlayers;
        GameTimer = data.GameTimer;
        EndgameTimer = data.EndgameTimer;
        ShowTagBadges = data.ShowTagBadges;
        EnableCall = data.EnableCall;
        JoinOnCreate = data.JoinOnCreate;
        Pattern = data.Pattern;
        MatchRule = data.MatchRule;
        SingleColumnConfig = data.SingleColumnConfig;

        Columns.MatchListSize(data.CardSize, () => new PackPresetColumnModel());
    }

    public PackPresetData ToData()
    {
        return new PackPresetData
        {
            CardSize = CardSize,
            FreeCenter = FreeCenter,
            LivesNumber = LivesNumber,
            MaxPlayers = MaxPlayers,
            GameTimer = GameTimer,
            EndgameTimer = EndgameTimer,
            ShowTagBadges = ShowTagBadges,
            EnableCall = EnableCall,
            JoinOnCreate = JoinOnCreate,
            Pattern = Pattern,
            MatchRule = MatchRule,
            SingleColumnConfig = SingleColumnConfig,
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
    
    public int MaxPlayers { get; set; } = 0;
    
    public int GameTimer { get; set; } = 0;

    public int EndgameTimer { get; set; } = 20;

    public bool ShowTagBadges { get; set; } = true;
    
    public bool EnableCall { get; set; } = true;

    public bool JoinOnCreate { get; set; } = true;

    public PackPresetPattern Pattern { get; set; } = PackPresetPattern.Lines;

    public PackPresetMatchRule MatchRule { get; set; } = PackPresetMatchRule.Default;

    public bool SingleColumnConfig { get; set; }

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
