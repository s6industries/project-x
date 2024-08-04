using UnityEngine;

namespace MeadowGames.UINodeConnect4
{
    public interface ISelectable
    {
        bool EnableSelect { get; set; }
        void Select();
        void Unselect();
        void Remove();
    }
}