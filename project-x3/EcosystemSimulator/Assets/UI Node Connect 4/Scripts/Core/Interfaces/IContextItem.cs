
namespace MeadowGames.UINodeConnect4.UICContextMenu
{
    public interface IContextItem
    {
        ContextMenuManager ContextMenu { get; set; }
        void OnChangeSelection();
    }
}