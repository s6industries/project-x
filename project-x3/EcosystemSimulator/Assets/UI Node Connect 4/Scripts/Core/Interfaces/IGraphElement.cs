using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeadowGames.UINodeConnect4
{
    public interface IGraphElement : IElement
    {
        string ID { get; set; }
	// v4.1 - added string SID property to IGraphElement to facilitate serialization
        string SID { get; set; }
        Color ElementColor { get; set; }
    }
}
