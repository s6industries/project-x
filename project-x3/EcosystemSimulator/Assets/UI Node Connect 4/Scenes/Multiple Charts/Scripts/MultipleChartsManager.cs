using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.SampleScene.MultipleCharts
{
    public class MultipleChartsManager : MonoBehaviour
    {
        public List<GameObject> chartsList;

        public void OpenChart(GameObject chart)
        {
            foreach (GameObject c in chartsList)
            {
                c.SetActive(false);
            }
            chart.SetActive(true);
        }
    }
}