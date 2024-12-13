using Quingo.Shared.Entities;

namespace Quingo.Application.State;

public class PlayerScore(PlayerState player)
{
    private const int CellMultiplier = 10;
    private const decimal TimeMultiplier = 1;
    private const decimal DrawMultiplier = 1;
    public PackPresetData Preset => player.GameState.Preset;

    private readonly CardPattern _bonusPattern =
        PatternGenerator.GeneratePatterns(player.GameState.Preset.CardSize, PackPresetPattern.Lines);

    private IEnumerable<PlayerCardCellData> AllCells => player.Card.AllCells;

    public int ScoreCells { get; private set; }

    public int ScorePatternBonus { get; private set; }

    public int ScoreRemainingTime { get; private set; }

    public int ScoreErrorPenalties { get; private set; }

    public int ScoreDrawPenalties { get; private set; }
    

    public int ScoreTotal => ScoreCells + ScorePatternBonus + ScoreRemainingTime - ScoreErrorPenalties - ScoreDrawPenalties;

    public void Calculate()
    {
        if (Preset.ScoringRules.HasFlag(PackPresetScoringRules.CellScore))
        {
            ScoreCells = CalculateCellScore() * CellMultiplier;
        }

        if (Preset.ScoringRules.HasFlag(PackPresetScoringRules.PatternBonus))
        {
            ScorePatternBonus = CalculatePatternBonuses() * CellMultiplier;
        }

        if (Preset.ScoringRules.HasFlag(PackPresetScoringRules.TimeBonus) && Preset.GameTimer > 0)
        {
            ScoreRemainingTime = CalculateTimeBonus();
        }

        if (Preset.ScoringRules.HasFlag(PackPresetScoringRules.ErrorPenalty))
        {
            ScoreErrorPenalties = CalculateErrorPenalties() * CellMultiplier;
        }

        if (Preset.ScoringRules.HasFlag(PackPresetScoringRules.DrawPenalty))
        {
            ScoreDrawPenalties = (int)Math.Round(CalculateDrawPenalties() * DrawMultiplier);
        }
    }

    private int CalculateCellScore()
    {
        return AllCells.Count(x => x.IsMarked && x.IsValid);
    }

    private int CalculatePatternBonuses()
    {
        var validPatterns = _bonusPattern.Validate(player.Card)
            .Where(x => x != null).Select(x => x!.Value).ToList();
        var rowsCols = validPatterns.Count(x => !IsDiagonal(x));
        var diagonals = validPatterns.Count(x => IsDiagonal(x));

        return rowsCols * 2 + diagonals * 3;
    }
    
    private int CalculateTimeBonus()
    {
        var timer = player.DoneTimer ??
               (player.GameState.State is GameStateEnum.FinalCountdown ? 0 : player.GameState.Timer.Value);
        var allCells = AllCells.ToList();
        var cellPercentage = allCells.Count(x => x.IsMarked && x.IsValid) / (decimal)allCells.Count;
        var result = timer * TimeMultiplier * cellPercentage;
        return (int)Math.Round(result);
    }

    private int CalculateErrorPenalties()
    {
        return AllCells.Count(x => !x.IsValid);
    }

    private int CalculateDrawPenalties()
    {
        return player.DrawState.DrawnNodes.Count;
    }

    // last 2 patterns are diagonals
    private bool IsDiagonal(int idx)
    {
        var count = _bonusPattern.Patterns.Count;
        return idx >= count - 2;
    }
}