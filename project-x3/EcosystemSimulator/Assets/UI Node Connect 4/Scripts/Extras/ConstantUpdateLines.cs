using System.Collections;
using System.Collections.Generic;
using MeadowGames.UINodeConnect4;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.Extra
{
    public class ConstantUpdateLines : MonoBehaviour
    {
        public GraphManager graphManager;

        void OnEnable()
        {
            graphManager = GetComponentInParent<GraphManager>();
            if (graphManager)
                UICSystemManager.AddToUpdate(OnUpdate);
        }

        void OnDisable()
        {
            UICSystemManager.RemoveFromUpdate(OnUpdate);
        }

        void OnUpdate()
        {
            foreach (Connection conn in UICSystemManager.Connections)
            {
                conn.UpdateLine();
            }
        }
    }
}