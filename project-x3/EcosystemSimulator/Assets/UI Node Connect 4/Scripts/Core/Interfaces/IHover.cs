namespace MeadowGames.UINodeConnect4
{
    public interface IHover
    {
        bool EnableHover { get; set; }
        void OnPointerHoverEnter();
        void OnPointerHoverExit();
    }
}
