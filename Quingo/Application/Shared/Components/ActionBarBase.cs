using Microsoft.AspNetCore.Components;
using MudBlazor;
using Quingo.Application.State;

namespace Quingo.Application.Shared.Components;

public abstract class ActionBarBase : ComponentBase
{
    [Inject] protected ILogger<ActionBarBase> Logger { get; set; } = default!;

    [Inject] protected GameStateService StateService { get; set; } = default!;

    [Inject] protected IDialogService DialogService { get; set; } = default!;

    [Inject] protected ISnackbar Snackbar { get; set; } = default!;

    public virtual GameState Game { get; set; } = default!;

    public virtual string UserId { get; set; } = default!;

    protected bool IsLoading { get; set; }

    protected async Task ConfirmEndGame()
    {
        var parameters = new ConfirmDialog.ConfirmDialogParams
        {
            Prompt = "Do you want to end the game?",
            ButtonText = "End game",
            ButtonColor = Color.Error
        };
        var confirmed = await ConfirmDialog.CallDialog(DialogService, parameters);

        if (confirmed)
        {
            await EndGame();
        }
    }

    private async Task EndGame()
    {
        try
        {
            await StateService.EndGame(Game.GameSessionId, UserId);
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            Snackbar.Add(e.Message, Severity.Error);
        }
    }

    protected void Draw()
    {
        try
        {
            Game.Draw();
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            Snackbar.Add(e.Message, Severity.Error);
        }
    }

    protected void PlayAgain()
    {
        IsLoading = true;
        try
        {
            StateService.PlayAgain(Game.GameSessionId);
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            Snackbar.Add(e.Message, Severity.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
}