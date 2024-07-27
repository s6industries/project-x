

namespace RaycastPro
{
    using UnityEngine;

#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    public abstract class BasePlanar : RaycastCore
    {
        public Transform poolManager;

        public float offset = .05f;
        public enum LengthControl { Continues, Constant, Sync, }
        public LengthControl lengthControl = LengthControl.Continues;
        public float length = 1;
        public enum OuterType { Auto, Reference, Clone }
        public OuterType outerType = OuterType.Auto;
        public enum DirectionOutput
        {
            /// <summary>
            /// Influence CloneRay Rotation by _Planar.transform.forward Direction as base Rotation.
            /// </summary>
            PlanarForward,
            /// <summary>
            /// Influence CloneRay Rotation by Sensor Local Direction as base Rotation.
            /// </summary>
            SensorLocal,
            /// <summary>
            /// Influence CloneRay Rotation by -Hit.Normal as base Rotation.
            /// </summary>
            NegativeHitNormal,
            /// <summary>
            /// Influence CloneRay Rotation by HitDirection as base Rotation.
            /// </summary>
            HitDirection,
        }

        public DirectionOutput baseDirection = DirectionOutput.PlanarForward;

        [Tooltip("Clones no longer continue RayCasting when disabled. This option has no effect on Line Renderer (except CutOnHit) and can help to optimize and secure cloning.")]
        public bool clonePathCast = true;
        protected override void OnCast() { } // NOTHING FOR NOW
        
#if UNITY_EDITOR

        protected static void RenameClone(RaycastCore sensor, string key = "C")
        {
            sensor.gameObject.name = $"{key}: {sensor.GetInstanceID().ToString()}";
        }
        protected void PlanarBaseField(SerializedObject _so)
        {
            EditorGUILayout.PropertyField(_so.FindProperty(nameof(clonePathCast)));
        }
        protected void OuterField(SerializedProperty property, SerializedProperty _outerRay)
        {
            BeginVerticalBox();
            
            PropertyEnumField(property, 3, "Outer Type".ToContent("Outer Type"), new GUIContent[]
            {
                "Auto".ToContent("Auto"),
                "Reference".ToContent("Reference"),
                "Clone".ToContent("Clone"),
            });
            
            if (outerType == OuterType.Reference) EditorGUILayout.PropertyField(_outerRay);
            
            EndVertical();
        }
        protected void BaseDirectionField(SerializedObject _so)
        {
            BeginVerticalBox();
            PropertyEnumField(_so.FindProperty(nameof(baseDirection)), 2, CBaseDirection.ToContent(TBaseDirection), new GUIContent[]
            {
                "Planar Forward".ToContent("Planar Forward"),
                "Sensor Local".ToContent("Sensor Local"),
                "-Hit Normal".ToContent("-Hit Normal"),
                "Hit Direction".ToContent("Hit Direction"),
            });
            
            EndVertical();
        }

        protected void LengthControlField(SerializedObject _so)
        {
            BeginVerticalBox();
            PropertyEnumField(_so.FindProperty(nameof(lengthControl)), 3, CLengthControl.ToContent(TLengthControl), new GUIContent[]
            {
                "Continues".ToContent("This option is the most common possible mode you can see, the Ray takes its remaining length out of the planar, which is normal."),
                "Constant".ToContent("As it is known, certain length of Ray is out of planar."),
                "Sync".ToContent("The same length of input Ray comes out of the Planar."),
            });
            PropertyMaxField(_so.FindProperty(nameof(length)), (lengthControl == LengthControl.Constant) ? CLength.ToContent() : CMultiplier.ToContent());
            EndVertical();
        }
#endif
    }
}