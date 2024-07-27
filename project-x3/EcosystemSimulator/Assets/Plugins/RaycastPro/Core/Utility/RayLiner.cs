namespace RaycastPro.Sensor
{
    using System.Collections.Generic;
    using RaySensors;
    using RaySensors2D;
    using UnityEngine;
#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Utility/" + nameof(RayLiner))]
    public sealed class RayLiner : BaseUtility
    {
        public RaySensor raySensor;
        public RaySensor2D raySensor2D;
        public LineRenderer liner;
        public bool cutOnHit;
        public bool clamped;
        public float basePosition;
        public float endPosition = 1f;

        private Vector3 _baseP, _tipP, _tipTarget;
        private float _pos;

        private (Vector3 point, int index) _p1, _p2;
        private Vector3 _hitPoint;
        private bool isDetect;

        private int detectIndex;
        private Vector3 dIndexPoint;

        private bool isPathRay;
        private List<Vector3> path = new List<Vector3>();
        private List<Vector3> _tPath = new List<Vector3>();
        private float sqrDis;
        public override bool Performed { get; protected set; }

        public void UpdateLiner()
        {
            if (!liner) return;
            if (!raySensor && !raySensor2D) return;

            if (raySensor)
            {
                _baseP = raySensor.Base;
                _tipP = raySensor.Tip;
                _tipTarget = raySensor.TipTarget;
                _hitPoint = raySensor.hit.point;
                isDetect = raySensor.hit.transform;
                if (raySensor is PathRay _pathRay)
                {
                    path = _pathRay.PathPoints;
                    detectIndex = _pathRay.DetectIndex;
                    isPathRay = true;
                }
                else isPathRay = false;
            }
            else
            {
                _baseP = raySensor2D.Base;
                _tipP = raySensor2D.Tip;
                _tipTarget = raySensor2D.TipTarget;
                _hitPoint = raySensor2D.hit.point;
                isDetect = raySensor2D.hit;
                if (raySensor2D is PathRay2D _pathRay)
                {
                    path = _pathRay.PathPoints.ToDepth(raySensor2D.z);
                    detectIndex = _pathRay.DetectIndex;
                    isPathRay = true;
                }
                else isPathRay = false;
            }
            if (isPathRay)
            {
                if (!clamped)
                {
                    if (cutOnHit) liner.SetSlicedPosition(path, _tipTarget, detectIndex);
                    else
                    {
                        liner.positionCount = path.Count;
                        liner.SetPositions(path.ToArray());
                    }
                }
                else // PathRay Clamped
                {
                    _p1 = path.GetPathInfo(basePosition);
                    _p2 = path.GetPathInfo(endPosition);

                    if (cutOnHit && isDetect) // Cut and detect Hit
                    {
                        dIndexPoint = path[detectIndex];
                        sqrDis = (_hitPoint - dIndexPoint).sqrMagnitude;
                        if (_p1.index < detectIndex + 1 || (_p1.index == detectIndex + 1 &&
                                                            (_p1.point - dIndexPoint).sqrMagnitude <=
                                                            sqrDis))
                        {
                            _tPath = new List<Vector3> {_p1.point};
                            var maxPoint = Mathf.Min(_p2.index, detectIndex + 1);
                            // Add remaining Points according to minimum index
                            for (var i = _p1.index; i < maxPoint; i++) _tPath.Add(path[i]);
                            if (detectIndex + 1 > _p2.index) _tPath.Add(_p2.point);
                            else
                                _tPath.Add((_p2.point - dIndexPoint).sqrMagnitude <= sqrDis
                                    ? _p2.point
                                    : _hitPoint);

                            liner.positionCount = _tPath.Count;
                            liner.SetPositions(_tPath.ToArray());
                        }
                        else // base Point over of Hit Point
                        {
                            liner.positionCount = 0;
                        }
                    }
                    else // Cut Without Detection
                    {
                        _tPath = new List<Vector3>() {_p1.point};
                        liner.positionCount = _p2.index - _p1.index + 2;
                        for (var i = _p1.index; i < _p2.index; i++) _tPath.Add(path[i]);
                        _tPath.Add(_p2.point);
                        liner.SetPositions(_tPath.ToArray());
                    }
                }
            }
            else // when normal ray
            {
                if (clamped)
                {
                    liner.positionCount = 2;
                    if (cutOnHit)
                    {
                        var _pos = raySensor.Performed ? raySensor.hit.distance / raySensor.RayLength : 1f;
                        var _b = raySensor.Base;
                        if (_pos >= raySensor.linerBasePosition)
                        {
                            liner.SetPosition(0, Vector3.Lerp(_b, raySensor.Tip, raySensor.linerBasePosition));
                            liner.SetPosition(1, _pos < raySensor.linerEndPosition ? raySensor.TipTarget : Vector3.Lerp(_b, raySensor.Tip, raySensor.linerEndPosition));
                        }
                        else liner.positionCount = 0;
                    }
                    else
                    {
                        var _t = raySensor.Tip;
                        var _b = raySensor.Base;
                        liner.SetPosition(0, Vector3.Lerp(_b, _t, raySensor.linerBasePosition));
                        liner.SetPosition(1, Vector3.Lerp(_b, _t, raySensor.linerEndPosition));
                    }
                }
                else // USE Full Clamp
                {
                    liner.positionCount = 2;
                    liner.SetPosition(0, _baseP);
                    liner.SetPosition(1, cutOnHit ? _tipTarget : _tipP);
                }
            }
        }

        protected override void OnCast()
        {
            UpdateLiner();
        }
#if UNITY_EDITOR
        
#pragma warning disable CS0414
        private static string Info = "Assigns a Line Renderer to the desired RaySource." + HDependent + HUtility;
#pragma warning restore CS0414
        internal override void OnGizmos()
        {
            if (IsSceneView && !IsPlaying)
            {
                raySensor?.Cast();
                raySensor2D?.Cast();
                UpdateLiner();
            }
        }

        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                if (!raySensor2D) EditorGUILayout.PropertyField(_so.FindProperty(nameof(raySensor)));
                if (!raySensor) EditorGUILayout.PropertyField(_so.FindProperty(nameof(raySensor2D)));

                if (liner) BeginVerticalBox();
                BeginHorizontal();
                var prop = _so.FindProperty(nameof(liner));
                EditorGUILayout.PropertyField(prop, CLiner.ToContent(TLiner));
                if (!liner && GUILayout.Button(CAdd, GUILayout.Width(50f)))
                {
                    // Fixed Liner Problem
                    if (TryGetComponent(out LineRenderer lineRenderer)) liner = lineRenderer;
                    else
                    {
                        prop.objectReferenceValue = gameObject.AddComponent<LineRenderer>();
                        _so.ApplyModifiedProperties();
                        liner.material =
                            new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
                    }

                    liner.endWidth = Mathf.Min(RCProPanel.linerMaxWidth, .1f);
                    liner.startWidth = Mathf.Min(RCProPanel.linerMaxWidth, .1f);
                    liner.numCornerVertices = Mathf.Min(0, 6);
                    liner.numCapVertices = Mathf.Min(0, 6);

                    UpdateLiner();
                }

                EndHorizontal();

                #region Liner Setting

                if (!liner) return;

                EditorGUILayout.PropertyField(_so.FindProperty(nameof(clamped)));
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(cutOnHit)));

                GUI.enabled = clamped;
                PropertyMinMaxField(_so.FindProperty(nameof(basePosition)), _so.FindProperty(nameof(endPosition)),
                    ref basePosition, ref endPosition, 0, 1);
                GUI.enabled = true;

                liner.startWidth = EditorGUILayout.Slider(CStartWidth, liner.startWidth, 0f, RCProPanel.linerMaxWidth);
                liner.endWidth = EditorGUILayout.Slider(CEndWidth, liner.endWidth, 0f, RCProPanel.linerMaxWidth);
                liner.numCapVertices = EditorGUILayout.IntField(CCap, liner.numCapVertices);
                liner.numCornerVertices = EditorGUILayout.IntField(CCorner, liner.numCornerVertices);

                GUI.backgroundColor = Color.white;
                liner.colorGradient = EditorGUILayout.GradientField(CGradient, liner.colorGradient);
                GUI.backgroundColor = RCProEditor.Violet;

                EndVertical();

                #endregion }

                if (hasGeneral)
                {
                    BaseField(_so);
                }

                if (hasInfo)
                {
                }
            }

        }
#endif
    }
}