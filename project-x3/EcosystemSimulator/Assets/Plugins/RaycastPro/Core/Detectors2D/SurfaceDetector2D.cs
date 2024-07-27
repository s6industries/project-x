using System.Collections.Generic;
using System.Linq;
using UnityEngine.SocialPlatforms;

namespace RaycastPro.Detectors2D
{
    using System;
    using UnityEngine;

#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Detectors/" + nameof(SurfaceDetector2D))]
    public class SurfaceDetector2D : ColliderDetector2D, IPulse
    {
        [SerializeField] private PolygonCollider2D polygonCollider;

        [SerializeField] private bool limited;
        [SerializeField] private int limitCount = 3;


        [SerializeField] private Collider2D[] colliders = Array.Empty<Collider2D>();
        

        private Vector3 _pos, forward;
        private float step;
        private Quaternion Q;

        private Vector2 pos2D;
        private RaycastHit2D blockHit;

        private List<Collider2D> colliderList = new List<Collider2D>();
        protected override void OnCast()
        {
            PreviousColliders = DetectedColliders.ToArray();
            
#if UNITY_EDITOR
            CleanGate();
#endif
            
            pos2D = transform.position.To2D();
            var contactFilter2D = new ContactFilter2D
            {
                layerMask = detectLayer,
                minDepth = MinDepth,
                maxDepth = MaxDepth,
                useLayerMask = true,
                useDepth = true,
                useTriggers = true,
            };

            Clear();
            
            if (limited)
            {
                for (var i = 0; i < colliders.Length; i++) colliders[i] = null;
                polygonCollider?.OverlapCollider(contactFilter2D, colliders);
                foreach (var c in colliders) Pass(c);
                EventPass();
            }
            else
            {
                polygonCollider?.OverlapCollider(contactFilter2D, colliderList);
                foreach (var c in colliderList) Pass(c);
                EventPass();
            }
        }

        private void Pass(Collider2D c)
        {
            if (!TagPass(c)) return;
            if (IsIgnoreSolver)
            {
#if UNITY_EDITOR
                PassColliderGate(c);
#endif
                ColliderPass(c);
                return;
            }
            TDP = DetectFunction(c);
            if (!polygonCollider.OverlapPoint(TDP)) return;
            blockHit = Physics2D.Linecast(transform.position, TDP, blockLayer.value, MinDepth, MaxDepth);
#if UNITY_EDITOR
            PassGate(c, TDP, _blockHit);
#endif
            if (!_blockHit || _blockHit.transform == c.transform)
            {
                ColliderPass(c);
            }
        }


#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Receiving colliders within the 2D polygon Collider with a detect point solver." +
                                     HCDetector + HLOS_Solver + HINonAllocator + HDependent;
#pragma warning restore CS0414
        internal override void OnGizmos()
        {
            EditorUpdate();
            
            if (polygonCollider)
            {
                var points = new Vector2[polygonCollider.points.Length + 1];
                
                for (var i = 0; i < polygonCollider.points.Length; i++)
                {
                    points[i] = polygonCollider.points[i];
                }

                points[polygonCollider.points.Length] = points[0];

                points = points.ToLocal(transform);
                
                Handles.color = (Performed ? DetectColor : DefaultColor);

                Handles.DrawPolyLine(points.ToDepth(MinDepth));
                Handles.DrawPolyLine(points.ToDepth(MaxDepth));

                var col = Handles.color;
                col.ToAlpha(RCProPanel.alphaAmount);
                Handles.color = col;
                
                Handles.DrawAAConvexPolygon(points.ToDepth(z));
                
                DrawFocusLine();
            }

        }
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(polygonCollider)));
            }

            if (hasGeneral)
            {
                GeneralField(_so);
                NonAllocatorField(_so, _so.FindProperty(nameof(colliders)));
                BaseField(_so);
                SolverField(_so);
                IgnoreListField(_so);
            }

            if (hasEvents) EventField(_so);
            if (hasEvents)
            {
                if (EventFoldout) RCProEditor.EventField(_so, CEventNames);
            }
            if (hasInfo) InformationField(PanelGate);
            }
#endif
    }
}