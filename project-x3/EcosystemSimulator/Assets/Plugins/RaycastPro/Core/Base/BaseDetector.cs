namespace RaycastPro
{
    using System.Linq;
    using System;
    using System.Globalization;
    using System.Collections.Generic;

    using UnityEngine;
    using UnityEngine.Events;
#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    public interface ITransformDetector
    {
        List<Transform> BlockedTargets { get; set; }
        Transform[] PreviousBlockedTargets { get; set; }
    }
    public interface IPulse { }
    public abstract class BaseDetector : RaycastCore
    {
        public LayerMask blockLayer;

        
        public enum SolverType { Ignore, Nearest, Pivot, Furthest, Focused, Dodge, }
        protected bool IsIgnoreSolver => solverType == SolverType.Ignore;
        protected bool IsFocusedSolver => solverType == SolverType.Focused;
        protected bool IsNearestSolver => solverType == SolverType.Nearest;
        protected bool IsPivotSolver => solverType == SolverType.Pivot;
        protected bool IsFurthestSolver => solverType == SolverType.Furthest;
        protected bool IsDodgeSolver => solverType == SolverType.Dodge;
        
        public abstract Vector3 FocusPoint { get; }

        public SolverType solverType;

        [SerializeField]
        protected bool checkLineOfSight = true;

        [SerializeField] protected bool usingTagFilter;
        [SerializeField] protected string tagFilter = "Untagged";

        #region Events

        public UnityEvent onCast;
        public UnityEvent onDetect;

        #endregion
        
        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Suitable for manual ray casting.
        /// </summary>
        /// <param name="_hit"></param>
        /// <returns></returns>
        public bool Cast()
        {
#if UNITY_EDITOR
            alphaCharge = AlphaLifeTime;
#endif
            OnCast();
            return Performed;
        }

        public float pulseTime;
        private float currentPulse;

        protected void PulseUpdate()
        {
            if (pulseTime > 0)
            {
                currentPulse += Time.unscaledDeltaTime;
                if (currentPulse >= pulseTime)
                {
                    currentPulse = 0f;
                    OnCast();
                    onCast?.Invoke();
                }
            }
            else
            {
                OnCast();
                onCast?.Invoke();
            }
        }

        protected void Update() { if (autoUpdate == UpdateMode.Normal) PulseUpdate(); }
        protected void LateUpdate() { if (autoUpdate == UpdateMode.Late) PulseUpdate(); }
        protected void FixedUpdate() { if (autoUpdate == UpdateMode.Fixed) PulseUpdate(); }
        
        /// <summary>
        /// Its an optimized foreach OnEnter/Exit callback event system.
        /// </summary>
        /// <param name="detected"></param>
        /// <param name="previous"></param>
        /// <param name="onMember"></param>
        /// <param name="onNewMember">When new member entered in detected Array</param>
        /// <param name="onLostMember">When a member lost the detected Array</param>
        /// <typeparam name="T"></typeparam>
        protected static void CallEvents<T>(List<T> detected, T[] previous, UnityEvent<T> onMember, UnityEvent<T> onNewMember, UnityEvent<T> onLostMember)
        {
            #region Events (Optimized)
            if (onMember != null) foreach (var _member in detected) onMember.Invoke(_member);
            if (onNewMember != null)
            {
                foreach (var _member in detected.Except(previous)) onNewMember.Invoke(_member);
            }
            if (onLostMember != null)
            {
                foreach (var _member in previous.Except(detected)) onLostMember.Invoke(_member);
            }
            #endregion
        }
        
        /// <summary>
        /// CleanGate Outside of #IF Unity_Editor Because of processing in OnCast()
        /// </summary>

        
#if UNITY_EDITOR
        protected Action PanelGate;
        
        protected const string TIgnore = "This solver works very fast and avoids spending unnecessary performance to check the detection point.";
        protected const string TNearest = "This solver sets the closest point of the Collider to SolverPoint as the criterion for being in the area and detecting the line of sight.";
        protected const string TPivot = "Pivot works as fast as Ignore. This option is very useful when you want to have the speed and detection point at the same time.";
        protected const string TCenter = "Detect Point on center of collider bounds.";
        protected const string TFurthest = "This reverse option works the closest point and is similar to it in terms of speed, it will be the best option when you want the collider to be located entirely in the range area.";
        protected const string TFocused = "With this option, you can custom move the focus point so use it if you want the detection point to stare at a certain point.";    
        protected const string TDodge = "The dodge option is based on pivot at default, will find the best way to escape blocking by using multiple checks. Although the performance of using this option is slightly higher than others, it simulates a smart line of sight.";
        
        protected bool PassHit(RaycastHit hit)
        {
            PanelGate += () => HitInfoField(hit);
            GizmoGate += () => { if (IsGuide)
            {
                Handles.color = DetectColor;
                DrawCross(hit.point, hit.normal);
            } };
            return true;
        }
        
        protected void PassColliderGate(Collider c)
        {
            PanelGate += () => DetectorInfoField(c.transform, c.bounds.center, false);
            GizmoGate += () =>
            {
                if (IsLabel) Handles.Label(c.bounds.center, c.name);
                if (IsGuide)
                {
                    Gizmos.color = DetectColor;
                    Gizmos.DrawWireCube(c.bounds.center, c.bounds.size);
                }
            };
        }
        protected void PassColliderGate(Collider2D c)
        {
            PanelGate += () => DetectorInfoField(c.transform, c.transform.position, false);
            
            GizmoGate += () =>
            {
                if (IsLabel) Handles.Label(c.bounds.center, $"<color=#34FF5F>{c.name}</color>", RCProEditor.LabelStyle);
                if (IsGuide)
                {
                    Gizmos.color = DetectColor;
                    Gizmos.DrawWireCube(c.bounds.center, c.bounds.size);
                }
            };
        }
        
        /// <summary>
        /// Panel and Gizmo gate clean (Without IF #UnityEditor)
        /// </summary>
        protected void CleanGate() { GizmoGate = PanelGate = null; }
        /// <summary>
        /// Includes OnCast + GizmoGate?.Invoke(), Avoiding core Process in editor scene.
        /// </summary>
        protected void EditorUpdate()
        {
            if(!RCProPanel.realtimeEditor) return;
            if (IsSceneView && !IsPlaying) OnCast();
            GizmoGate?.Invoke();
        }
        protected static void DrawBlockLine2D(Vector2 p1, Vector2 p2, float depth,
            Transform blockedTransform, RaycastHit2D blockHit = default)
        {
            if (!blockHit) return;
            
            if (IsBlockLine)
            {
                Handles.color = BlockColor;
                Handles.DrawDottedLine(p1.ToDepth(depth), blockHit.point.ToDepth(depth), StepSizeLine);

                Handles.color = DefaultColor;
                Handles.DrawDottedLine(blockHit.point.ToDepth(depth), p2.ToDepth(depth), StepSizeLine);
            }

            if (IsLabel)
            {
                Handles.Label(blockHit.point, $"{blockedTransform.name} blocked by {blockHit.collider.name}");
            }
        }

        protected static void DrawBlockLine2D(Vector2 p1, Vector2 p2, float depth, RaycastHit2D blockHit = default)
        {
            if (blockHit)
            {
                Handles.color = BlockColor;
                Handles.DrawDottedLine(p1.ToDepth(depth), blockHit.point.ToDepth(depth), StepSizeLine);

                Handles.color = DefaultColor;
                Handles.DrawDottedLine(blockHit.point.ToDepth(depth), p2.ToDepth(depth), StepSizeLine);
            }
            else
            {
                Handles.color = DetectColor;
                Handles.DrawLine(p1.ToDepth(depth), p2.ToDepth(depth));
            }
        }

        protected static void HitInfoField(RaycastHit hit)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Box(hit.transform.name);
            GUILayout.Box(hit.point.ToString());
            GUILayout.EndHorizontal();
        }
        protected static void DetectorInfoField(Transform t, Vector3 targetPivot, bool blockResult)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Box(t.name, RCProEditor.LabelStyle);
            GUILayout.Box(targetPivot.ToString());

            GUI.contentColor = blockResult ? BlockColor : DetectColor;
            GUILayout.Box(blockResult ? "Blocked" : "Detect",  RCProEditor.BoxStyle);
            GUI.contentColor = RCProEditor.Aqua;
            GUILayout.EndHorizontal();
        }

        protected static void DetectorInfoField(Collider2D c)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Box(c.name);
            GUILayout.Box("Pivot: " + c.bounds.size);
            GUILayout.Box("Depth: " + c.transform.position.z.ToString(CultureInfo.InvariantCulture));
            GUILayout.EndHorizontal();
        }
        protected void BaseSolverField(SerializedObject _so, Action inBox = null)
        {
            if (Application.isPlaying) GUI.enabled = false;
            BeginVerticalBox();
            PropertyEnumField(_so.FindProperty(nameof(solverType)), 3, CSolverType.ToContent(TSolverType)
            ,new[] {"Ignore".ToContent(TIgnore),"Nearest".ToContent(TNearest),"Pivot".ToContent(TPivot),"Furthest".ToContent(TFurthest),"Focused".ToContent(TFocused), "Dodge".ToContent(TDodge)}
            );
            
            inBox?.Invoke();
            EndVertical();
            GUI.enabled = true;
        }

        protected void PulseField(SerializedObject _so)
        {
            if (enabled && this is IPulse)
            {
                PropertyMaxField(_so.FindProperty(nameof(pulseTime)), "Pulse Time".ToContent("It creates a time gap between each process, which is used to a large extent in process optimization."));
            }
        }
        
        protected void EventField(SerializedObject _so)
        {
            EventFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(EventFoldout, CEvents.ToContent(TEvents),
                RCProEditor.HeaderFoldout);
            EditorGUILayout.EndFoldoutHeaderGroup();
            if (!EventFoldout) return;
            RCProEditor.EventField(_so, events);
        }

        private static readonly string[] events = new[] {nameof(onCast), nameof(onDetect)};
        protected void TagField(SerializedObject _so)
        {
            BeginHorizontal();
            
            var usingTagFilterProp = _so.FindProperty(nameof(usingTagFilter));
            EditorGUILayout.PropertyField(usingTagFilterProp, "Tag Filter".ToContent());
            GUI.enabled = usingTagFilter;
            var rect = EditorGUILayout.GetControlRect(false);
            var tagFilterProp = _so.FindProperty(nameof(tagFilter));
            EditorGUI.BeginProperty(rect, "Tag Filter".ToContent(), tagFilterProp);
            EditorGUI.BeginChangeCheck();
            var newTag = EditorGUI.TagField(rect, tagFilter);
            if (EditorGUI.EndChangeCheck()) tagFilterProp.stringValue = newTag;
            EditorGUI.EndProperty();
            GUI.enabled = true;
            EndHorizontal();
        }

#endif
    }
}