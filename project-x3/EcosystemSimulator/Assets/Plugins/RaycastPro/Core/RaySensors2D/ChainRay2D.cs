namespace RaycastPro.RaySensors2D
{
    using System;
    using System.Linq;
    using UnityEngine;
#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Rey Sensors/" + nameof(ChainRay2D))]
    public sealed class ChainRay2D : PathRay2D, IRadius
    {
        public ChainReference chainReference = ChainReference.Point;
        
        [SerializeField] private float radius = .1f;
        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0,value);
        }
        public Vector2[] chainPoints = {Vector2.right, Vector2.up};
        public Transform[] targets = Array.Empty<Transform>();

        public bool relative;
        
        private Vector2 sum;
        private int i, j;
        internal void ToRelative()
        {
            PathPoints.Clear();
            PathPoints.Add(transform.position);
            for (i = 0; i < chainPoints.Length; i++)
            {
                sum = Vector2.zero;
                for (j = 0; j <= i; j++) sum += chainPoints[j];
                if (local) sum = transform.TransformPoint(sum);
                PathPoints.Add(sum.ToDepth());
            }
        }
        protected override void OnCast()
        {
            UpdatePath();
            if (pathCast)
            {
                DetectIndex = PathCast(out hit, radius);
                isDetect = hit && FilterCheck(hit);
            }
        }

        protected override void UpdatePath()
        {
            PathPoints.Clear();
            PathPoints.Add(Position2D);
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
                    
                    PathPoints.Clear();
                    foreach (var t in targets)
                    {
                        if (t) PathPoints.Add(t.position.To2D());
                        
                    }
                    break;
            }
        }

#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Send a point oriented 2D ray and return Hit information." + HAccurate + HPathRay + HIRadius + HScalable;
#pragma warning restore CS0414
        
        internal override void OnGizmos()
        {
            EditorUpdate();

            FullPathDraw(radius, true);
            DrawNormal2D(hit, z);
            DrawNormalFilter();
        }

        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                PropertyEnumField(_so.FindProperty(nameof(chainReference)), 2, CReferenceType.ToContent(TReferenceType), new GUIContent[]
                    {"Transform".ToContent("Can adjust game object's position as chain reference."), "Point".ToContent("Adjust Points as regular vector2 positions with a relative mode option.")}
                );
                BeginVerticalBox();
                if (chainReference == ChainReference.Point)
                {
                    RCProEditor.PropertyArrayField(_so.FindProperty(nameof(chainPoints)), "Points".ToContent("Points"),
                        (i) => $"Points {i+1}".ToContent($"Index {i}"));
                    
                    EditorGUILayout.PropertyField(_so.FindProperty(nameof(relative)),
                        CRelative.ToContent(TRelative), relative);
                }
                else RCProEditor.PropertyArrayField(_so.FindProperty(nameof(targets)), "Targets".ToContent("Targets"),
                    (i) => $"Target {i+1}".ToContent($"Index {i}"));
                EndVertical();
                RadiusField(_so);
            }


            if (hasGeneral) PathRayGeneralField(_so);
            
            if (hasEvents) EventField(_so);

            if (hasInfo) HitInformationField();
        }
#endif
    }
}