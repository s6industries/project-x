namespace RaycastPro.Sensor
{
    using UnityEngine;
#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Utility/" + nameof(PointSensor))]
    public sealed class PointSensor : BaseSensor, IRadius
    {
        public enum DotType
        {
            Sphere,
            Box
        }

        public DotType dotType = DotType.Sphere;

        [SerializeField] private float radius = 2f;

        [SerializeField] private float height;

        public float Height
        {
            get => height;
            set => height = Mathf.Max(0, value);
        }

        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0, value);
        }

        public Vector3 extents = Vector3.one;

        public override bool Performed
        {
            get => performed;
            protected set {}
        }

        private bool performed;
        
        protected override void OnCast()
        {
            var _t = transform;
            
            performed = false;
            switch (dotType)
            {
                case DotType.Sphere:
                    if (height > 0)
                    {
                        var h = _t.up * height / 2;
                        performed = Physics.CheckCapsule(_t.position - h, _t.position + h, radius,
                            detectLayer.value, triggerInteraction);
                    }
                    else
                    {
                        performed = Physics.CheckSphere(_t.position, radius, detectLayer.value, triggerInteraction);
                    }

                    break;
                case DotType.Box:
                    performed = Physics.CheckBox(_t.position, extents / 2, _t.rotation, detectLayer.value,
                        triggerInteraction);
                    break;
            }
        }
#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Simple Point that returns if collider on it."+ HUtility + HAccurate + HIRadius;
#pragma warning restore CS0414
        internal override void OnGizmos()
        {
            if (IsSceneView) OnCast();

            Gizmos.color = Handles.color = Performed ? DetectColor : DefaultColor;

            var _t = transform;
            switch (dotType)
            {
                case DotType.Sphere:
                    GizmoColor = Performed ? DetectColor : HelperColor;
                    if (radius == 0) DrawArrows();

                    else if (height > 0)
                    {
                        var h = _t.up * (height / 2);
                        DrawCapsuleLine(_t.position - h, _t.position + h, radius: radius, _t: _t);
                    }
                    else DrawSphere(_t.position, _t.up, radius);

                    break;

                case DotType.Box:
                    if (extents == Vector3.zero)
                    {
                        Gizmos.color = Handles.color = Performed ? DetectColor : HelperColor;
                        DrawArrows();
                    }

                    Gizmos.matrix = _t.localToWorldMatrix;
                    Gizmos.DrawWireCube(Vector3.zero, extents);
                    break;
            }
        }

        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                dotType = RCProEditor.EnumLabelField(dotType, "Dot Type".ToContent(), 2);
                RCProEditor.LayerField(CDetectLayer.ToContent(TDetectLayer), ref detectLayer);
                switch (dotType)
                {
                    case DotType.Sphere:
                        Radius = EditorGUILayout.FloatField(CRadius, radius);

                        Height = EditorGUILayout.FloatField(CHeight, height);
                        break;
                    case DotType.Box:
                        extents = EditorGUILayout.Vector3Field(CExtents, extents);
                        break;
                }
            }

            if (hasGeneral)
            {
                BaseField(_so);
            }

            if (hasEvents)
            {
                EventsField(_so);
            }

            if (hasInfo)
            {
                InformationField(() =>
                {
                    BeginHorizontal();
                    GUILayout.Label("Performed");
                    GUI.contentColor = Performed ? DetectColor : DefaultColor;
                    GUILayout.Label(Performed.ToString());
                    GUI.contentColor = RCProEditor.Aqua;
                    EndHorizontal();
                });
            }
        }
#endif
    }
}