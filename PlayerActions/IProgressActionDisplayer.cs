namespace ToolForExiled.PlayerActions;

public interface IProgressActionDisplayer
{
    void ShowLoading(Player player, float progress, float duration);
    void ClearLoading(Player player);
}