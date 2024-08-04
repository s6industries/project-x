using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.SampleScene.MultipleCharts
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] Camera _camera;
        [SerializeField] List<GraphManager> _graphs = new List<GraphManager>();
        [SerializeField] float _duration = 1;

        bool _moving = false;
        public void GotoChart(int graph)
        {
            if (!_moving)
            {
                foreach (GraphManager manager in _graphs)
                {
                    if (manager != _graphs[graph])
                    {
                        manager.enabled = false;
                    }
                    else
                    {
                        manager.enabled = true;
                    }
                }

                StartCoroutine(C_MoveToPosition(_graphs[graph].transform.position - new Vector3(0, 0, 50), graph));
            }
        }

        IEnumerator C_MoveToPosition(Vector3 position, int graph)
        {
            _moving = true;

            foreach (GraphManager manager in _graphs)
            {
                manager.pointer.customImage.enabled = false;
            }

            float t = 0.0f;
            Vector3 start = _camera.transform.position;
            Vector3 end = position;

            while (t < _duration)
            {
                t += Time.deltaTime;
                _camera.transform.position = Vector3.Lerp(start, end, t / _duration);
                yield return null;
            }

            _graphs[graph].pointer.customImage.enabled = true;
            _graphs[graph].pointer.customImage.sprite = _graphs[graph].pointer.iconDefault;

            _moving = false;
        }
    }
}