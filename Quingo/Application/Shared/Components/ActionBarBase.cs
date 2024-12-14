using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using Quingo.Application.Core;

namespace Quingo.Application.Shared.Components;

public abstract class ActionBarBase : ComponentBase, IAsyncDisposable
{
    [Inject] protected ILogger<ActionBarBase> Logger { get; set; } = default!;

    [Inject] protected GameService GameService { get; set; } = default!;

    [Inject] protected IDialogService DialogService { get; set; } = default!;

    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    
    [Inject] protected IJSRuntime Js { get; set; } = default!;

    public virtual GameInstance Game { get; set; } = default!;

    public virtual string UserId { get; set; } = default!;

    protected virtual bool DisableDraw { get; } = false;

    protected bool IsLoading { get; set; }

    private readonly DotNetObjectReference<ActionBarBase> _reference;

    protected ActionBarBase()
    {
        _reference = DotNetObjectReference.Create(this);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        await Js.InvokeVoidAsync("window.gameKeyListener.add", _reference);
    }

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
            await GameService.EndGame(Game.GameSessionId, UserId);
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            Snackbar.Add(e.Message, Severity.Error);
        }
    }

    protected virtual void DrawFunc()
    {
        Game.Draw();
    }

    [JSInvokable("Draw")]
    public void Draw()
    {
        if (DisableDraw) return;
        
        try
        {
            DrawFunc();
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
            GameService.PlayAgain(Game.GameSessionId);
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

    protected void PauseGame()
    {
        try
        {
            Game.PauseGame();
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            Snackbar.Add(e.Message, Severity.Error);
        }
    }
    
    protected void ResumeGame()
    {
        try
        {
            Game.ResumeGame();
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.Message);
            Snackbar.Add(e.Message, Severity.Error);
        }
    }

    public async ValueTask DisposeAsync(){
        _reference.Dispose();
        try
        {
            await Js.InvokeVoidAsync("window.gameKeyListener.remove");
        }
        catch
        {
            // ignored
        }
    }
}