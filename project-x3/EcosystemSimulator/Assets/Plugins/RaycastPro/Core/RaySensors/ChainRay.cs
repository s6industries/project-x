namespace RaycastPro.RaySensors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Rey Sensors/" + nameof(ChainRay))]
    public sealed class ChainRay : PathRay, IRadius
        #if UnityEditor
,ISceneGUI
        #endif
    {
        public ChainReference chainReference = ChainReference.Point;
        [SerializeField] private float radius = 0f;
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0,value);
        }
        public Vector3[] chainPoints = {Vector3.forward, Vector3.right, Vector3.up};
        public Transform[] targets = Array.Empty<Transform>();
        public enum ChainReference
        {
            /// <summary>
            /// As setup reference to transform, You could animate chain points on playmode.
            /// </summary>
            Transform,
            Point,
        }

        public bool relative;

        protected override void OnCast()
        {
            UpdatePath();
            if (pathCast) DetectIndex = PathCast(radius);
        }

        private Transform target;

        private Vector3 sum;
        private int i, j;
        internal void ToRelative()
        {
            PathPoints.Clear();
            PathPoints.Add(transform.position);
            for (i = 0; i < chainPoints.Length; i++)
            {
                sum = Vector3.zero;
                for (j = 0; j <= i; j++) sum += chainPoints[j];
                if (local) sum = transform.TransformPoint(sum);
                PathPoints.Add(sum);
            }
        }
        protected override void UpdatePath()
        {
            PathPoints.Clear();
            PathPoints.Add(transform.position);
            switch (chainReference)
            {
                case ChainReference.Point:
                    if (relative)
                    {
                        ToRelative();
                    }
                    else
                    {
                        foreach (var _cP in chainPoints)
                        {
                            PathPoints.Add(transform.TransformPoint(_cP));
                        }
                    }
                    break;
                case ChainReference.Transform:
                {
                    for (var index = 0; index < targets.Length; index++) // For is fastest
                    {
                        if (targets[index]) PathPoints.Add(targets[index].position);
                    }
                    break;
                }
            }
        }
#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Send a point oriented ray and return Hit information." + HAccurate + HPathRay +HIRadius + HScalable;
#pragma warning restore CS0414
        internal override void OnGizmos()
        {
            EditorUpdate();
            if (PathPoints.Count == 0) return;
            if (hit.transform) DrawNormal(hit.point, hit.normal, hit.transform.name);

            FullPathDraw(radius);
            
            if (RCProPanel.DrawGuide)
            {
                Handles.color = Gizmos.color = HelperColor;
                Handles.DrawDottedLine(Base, Tip, StepSizeLine);
                DrawCap(PathPoints.Last(), PathPoints.LastDirection(TipDirection));
                if (RCProPanel.ShowLabels)
                {
                    Handles.Label((Base + Tip) / 2,
                        "<color=#2BC6D2>Distance:</color> <color=#FFFFFF>" + TipLength.ToString("F2") + "</color>", new GUIStyle {richText = true});
                }
            }
        }

        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true, bool hasInfo = true)
        {
            if (hasMain)
            {
                PropertyEnumField(_so.FindProperty(nameof(chainReference)), 2, CReferenceType.ToContent(TReferenceType), new GUIContent[]
                    {"Transform".ToContent("Can adjust game object's position as chain reference."), "Point".ToContent("Adjust Points as regular vector3 positions with a relative mode option.")}
                );
                BeginVerticalBox();
                if (chainReference == ChainReference.Point)
                {
                    RCProEditor.PropertyArrayField(_so.FindProperty(nameof(chainPoints)), "Points".ToContent(),
                        (i) => $"Points {i+1}".ToContent($"Index {i}"));
                    
                    EditorGUILayout.PropertyField(_so.FindProperty(nameof(relative)),
                        CRelative.ToContent(TRelative), relative);
                }
                else
                {
                    RCProEditor.PropertyArrayField(_so.FindProperty(nameof(targets)), "Targets".ToContent(),
                        (i) => $"Target {i + 1}".ToContent($"Index {i}"));
                }
                
                EndVertical();
                BeginHorizontal();
                RadiusField(_so);
                LocalField(_so.FindProperty(nameof(local)));
                EndHorizontal();
            }
            
            if (hasGeneral) PathRayGeneralField(_so);

            if (hasEvents) EventField(_so);

            if (hasInfo) InformationField();
        }

        private Vector3 _l;
#endif
    }
}