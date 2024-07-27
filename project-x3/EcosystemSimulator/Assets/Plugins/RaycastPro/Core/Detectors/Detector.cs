namespace RaycastPro.Detectors
{
    using System;
    using UnityEngine;
    using UnityEngine.Events;

#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif
    [Serializable]
    public class ColliderEvent : UnityEvent<Collider> { }

    [Serializable]
    public class TransformEvent : UnityEvent<Transform> { }

    public abstract class Detector : BaseDetector
    {
        /// <summary>
        /// Temp Detected Point
        /// </summary>
        protected Vector3 TDP;

        #region BlockSystem
        public Vector3 detectVector;
        public override Vector3 FocusPoint => transform.TransformPoint(detectVector);
        #endregion

   
#if UNITY_EDITOR
        protected void DrawFocusVector()
        {
            if (!RCProPanel.DrawGuide) return;

            if (solverType == SolverType.Focused)
            {
                Gizmos.color = HelperColor;
                var point = transform.TransformDirection(detectVector) + transform.position;

                Gizmos.DrawLine(transform.position, point);
                Gizmos.DrawWireSphere(point, DotSize);

                if (RCProPanel.ShowLabels) Handles.Label(point, solverType + " Vector");
            }

            if (solverType != SolverType.Ignore && checkLineOfSight)
            {
                Gizmos.color = HelperColor;
                Gizmos.DrawLine(transform.position, transform.position);
                Gizmos.DrawWireSphere(transform.position, DotSize);
            }
        }
        protected abstract void DrawDetectorGuide(Vector3 point);


        protected void GeneralField(SerializedObject _so, bool layerField = true, bool hasTagField = true)
        {
            if (this is IPulse) PulseField(_so);
            if (layerField) DetectLayerField(_so);
            if (hasTagField) TagField(_so);
        }
#endif
    }
}