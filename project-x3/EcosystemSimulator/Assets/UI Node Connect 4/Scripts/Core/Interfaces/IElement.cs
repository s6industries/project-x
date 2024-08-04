using UnityEngine;

namespace MeadowGames.UINodeConnect4
{
    public interface IElement
    {
        int Priority { get; }
        void Remove();
    }
}