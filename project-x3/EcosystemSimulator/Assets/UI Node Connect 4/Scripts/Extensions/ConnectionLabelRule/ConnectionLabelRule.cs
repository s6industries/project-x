using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.Extension
{
    [ExecuteInEditMode]
    public abstract class ConnectionLabelRule : MonoBehaviour
    {
        public static bool warnMultipleRulesInScene = true;

        public virtual void ExecuteRule(Connection connection)
        {

        }

        void OnEnable()
        {
            UICSystemManager.UICEvents.StartListening(UICEventType.ConnectionAdded, ExecuteConnectionLabelRule);
        }

        void OnDisable()
        {
            UICSystemManager.UICEvents.StopListening(UICEventType.ConnectionAdded, ExecuteConnectionLabelRule);
        }

        public void ExecuteConnectionLabelRule(IElement connectionElement)
        {
            Connection connection = connectionElement as Connection;
            if (connection != null)
            {
                ExecuteRule(connection);
                if (connection.label)
                {
                    float scale = 1/connection.graphManager.lineRenderer.rectScaleX;
                    connection.label.transform.localScale = new Vector3(scale, scale, 1);
                }
            }
        }

        void Awake()
        {
            if (warnMultipleRulesInScene)
            {
                if (FindObjectsOfType<ConnectionLabelRule>().Length > 1)
                {
                    Debug.LogWarning("There are multiple Connection Label Rules in the scene. If that is intentional you can disable this warning at the rule inspector");
                }
            }
        }
    }
}