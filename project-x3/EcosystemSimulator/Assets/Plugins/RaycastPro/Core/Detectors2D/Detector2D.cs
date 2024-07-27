namespace RaycastPro.Detectors2D
{
    using System;
    using UnityEngine;
    using UnityEngine.Events;
#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif
    [Serializable]
    public class Collider2DEvent : UnityEvent<Collider2D> { }
    public abstract class Detector2D : BaseDetector
    {
        /// <summary>
        /// TempDetectedPoint
        /// </summary>
        protected Vector3 TDP;

        #region BlockSystem
        public Vector2 focusPointOffset;

        /// <summary>
        /// POV based on offset
        /// </summary>
        public override Vector3 FocusPoint => transform.position + transform.TransformDirection(focusPointOffset);
        public Vector2 Position2D => transform.position;
        #endregion

        /// <summary>
        /// Local Min Depth
        /// </summary>
        [SerializeField] protected float minDepth = .5f;
        /// <summary>
        /// Local Max Depth
        /// </summary>
        [SerializeField] protected float maxDepth = -.5f;

        /// <summary>
        /// Return Min Depth as World space. Form : transform.position.z + minDepth
        /// </summary>
        public float MinDepth => transform.position.z + minDepth;
        
        /// <summary>
        /// Return Max Depth as World space. Form : transform.position.z + maxDepth
        /// </summary>
        public float MaxDepth => transform.position.z + maxDepth;

        public float z => transform.position.z;

#if UNITY_EDITOR
        
        protected void PassGate(Collider2D c, Vector3 point, RaycastHit2D blockHit = default)
        {
            GizmoGate += () =>
            {
                if(blockHit && blockHit.collider != c)
                {
                    Gizmos.color = BlockColor;
                    if (IsGuide) Gizmos.DrawWireCube(c.bounds.center, c.bounds.extents * 2);
                    DrawBlockLine2D(transform.position, point, z, c.transform, blockHit);
                }
                else if (IsDetectLine)
                {
                    GizmoColor = DetectColor;
                    if (IsGuide) Gizmos.DrawWireCube(c.bounds.center, c.bounds.extents * 2);
                    Handles.DrawLine(transform.position.ToDepth(z), point.ToDepth(z));
                }
            };
            PanelGate += () => DetectorInfoField(c.transform, point, blockHit && blockHit.collider != c);
        }
        protected void DrawFocusLine()
        {
            if (!RCProPanel.DrawGuide) return;
            
            if (RCProPanel.ShowLabels)  Handles.Label(transform.position, "Block Solver Point");

            if (!IsFocusedSolver) return;

            var point = FocusPoint;
            Handles.color = HelperColor;
            Handles.DrawLine(transform.position, point);
            Handles.DrawWireDisc(point, Vector3.forward, DotSize);

            if (RCProPanel.ShowLabels) Handles.Label(point, solverType + " Vector");
        }

        protected void DrawDepthLine(Vector3 position, bool dotted = true) =>  DrawLineZTest(position.ToDepth(MinDepth), position.ToDepth(MaxDepth), dotted);

        protected void GeneralField(SerializedObject _so)
        {
            if (this is IPulse) PulseField(_so);
            DetectLayerField(_so);
            TagField(_so);
            DepthField(_so);
        }

        protected void DepthField(SerializedObject _so)
        {
            EditorGUILayout.PropertyField(_so.FindProperty(nameof(minDepth)), "Min Depth".ToContent());
            EditorGUILayout.PropertyField(_so.FindProperty(nameof(maxDepth)), "Max Depth".ToContent());
        }
        protected void Draw4AxesDepth(float offset)
        {
            DrawDepthLine(transform.position + Vector3.right * offset);
            DrawDepthLine(transform.position - Vector3.right * offset);
            DrawDepthLine(transform.position + Vector3.up * offset);
            DrawDepthLine(transform.position - Vector3.up * offset);
        }
        protected void DrawDepthCircle(float radius)
        {
            Draw4AxesDepth(radius);

            Handles.color = DefaultColor;

            Handles.DrawWireDisc(transform.position.ToDepth(MinDepth), Vector3.forward, radius);

            Handles.DrawWireDisc(transform.position.ToDepth(MaxDepth), Vector3.forward, radius);
        }
#endif
    }
}