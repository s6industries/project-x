
using UnityEngine.Events;

namespace MeadowGames.UINodeConnect4
{
    public interface IClickable
    {
        bool DisableClick { get; }
        void OnPointerDown();
        void OnPointerUp();
    }
}