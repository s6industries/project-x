
using RaycastPro.RaySensors;
using RaycastPro.RaySensors2D;

namespace RaycastPro
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Rendering;
    
#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif
    internal interface IPath<T>
    {
        List<T> Path { get; }
    }

    internal interface IRadius
    {
        /// <summary>
        /// Radius area control as well as all radii use the "IRadius" interface. 
        /// </summary>
        float Radius { get; set; }
    }
    public interface ISceneGUI
    {
        void OnSceneGUI();
    }
    
    /// <summary>
    /// 
    /// </summary>
    public abstract class RaycastCore : MonoBehaviour
    {
        #region Public Enums
        /// <summary>
        /// Indicates whether ray has detected a target.
        /// </summary>
        public abstract bool Performed { get; protected set; }
        public enum TimeMode
        {
            /// <summary>
            /// Delta Time
            /// </summary>
            DeltaTime,
            /// <summary>
            /// Smooth Delta Time
            /// </summary>
            SmoothDeltaTime,
            /// <summary>
            /// Fixed Delta Time
            /// </summary>
            FixedDeltaTime,
            /// <summary>
            /// Unscaled Delta Time
            /// </summary>
            UnscaledDeltaTime,
            /// <summary>
            /// Unscaled Fixed Delta Time
            /// </summary>
            UnscaledFixedDeltaTime,
        }
        public enum WeightType
        {
            /// <summary>
            /// A value between 0 and 1 that includes the entire path.
            /// </summary>
            Clamp,

            /// <summary>
            /// The free amount that can be included beyond the enclosure.
            /// </summary>
            Distance,
            
            /// <summary>
            /// Adjust target Offset.
            /// </summary>
            Offset,
        }
        public enum Axis { X, Y, Z }
        public enum UpdateMode
        {
            /// <summary>
            /// Normal: It is done at the same time as the usual update of Unity, which is suitable for graphic movements such as Liner movement.
            /// </summary>
            Normal,

            /// <summary>
            /// Fixed (Recommended): will be processed with physics calculations and will be suitable for the condition.
            /// </summary>
            Fixed,

            /// <summary>
            /// Late: is executed after performing physical movements and animations.
            /// </summary>
            Late,
        }
        public enum RayType { Ray, Pipe, Box }
        public enum BodyType { Ray, Pipe }
        #endregion

        #region Parameteres
        /// <summary>
        /// This auxiliary variable is defined for coefficient on other variables affected by ray. For example: gunDamage = influence * power
        /// </summary>
        [Range(0, 1)] [SerializeField] internal float influence = 1;
        [SerializeField]
        public QueryTriggerInteraction triggerInteraction;
        [SerializeField]
        public LayerMask detectLayer = 1;
        [SerializeField]
        public UpdateMode autoUpdate = UpdateMode.Fixed;
        /// <summary>
        /// This auxiliary variable is defined for coefficient on other variables affected by ray. For e.g: gunDamage = influence * power
        /// </summary>
        public float Influence
        {
            get => influence;
            set => influence = Mathf.Clamp01(value);
        }
        #endregion
        [Serializable]
        internal class AxisRun
        {
            [SerializeField]
            public Axis axis;
            [SerializeField]
            public bool syncAxis;
            [SerializeField]
            public bool flipAxis;
            
            public void SyncAxis(Transform _t, Vector3 forward)
            {
                switch (axis)
                {
                    case Axis.X:
                        _t.right = forward * (flipAxis ? -1 : 1);
                        break;
                    case Axis.Z:
                        _t.forward = forward * (flipAxis ? -1 : 1);
                        break;
                    case Axis.Y:
                        _t.up = forward * (flipAxis ? -1 : 1);
                        break;
                }
            }
            
            /// <summary>
            /// Damped Sync Only Support forward for now
            /// </summary>
            /// <param name="_t"></param>
            /// <param name="forward"></param>
            /// <param name="dampSharpness"></param>
            /// <param name="deltaTime"></param>
            public void DampedSyncAxis(Transform _t, Vector3 forward, float dampSharpness = 15, in float deltaTime = .02f)
            {
                _t.rotation = Quaternion.Lerp(_t.rotation, 
                    Quaternion.LookRotation(forward.normalized * (flipAxis ? -1 : 1), _t.up)
                    , 1 - Mathf.Exp(-dampSharpness * deltaTime));
            }


#if UNITY_EDITOR
            
            private static GUIContent[] tips = new GUIContent[] {"X".ToContent(), "Y".ToContent(), "Z".ToContent()};
            internal void EditorPanel(SerializedProperty axisRunProperty, bool withBox = false)
            {
                if (withBox) BeginVerticalBox();
                var rect = EditorGUILayout.GetControlRect(true, 18);
                var axisProp = axisRunProperty.FindPropertyRelative(nameof(axis));
                EditorGUI.BeginProperty(rect, "Axis".ToContent(), axisProp);
                EditorGUILayout.GetControlRect(true, -18);
                BeginHorizontal();
                EditorGUILayout.PropertyField(axisRunProperty.FindPropertyRelative(nameof(syncAxis)),
                    CSyncAxis.ToContent(TSyncAxis));
                GUI.enabled = syncAxis;
                EditorGUI.BeginChangeCheck();
                var newValue = GUILayout.SelectionGrid(axisProp.enumValueIndex, tips, 3);
                if (EditorGUI.EndChangeCheck()) axisProp.enumValueIndex = newValue;
                EditorGUI.EndProperty();
                MiniField(axisRunProperty.FindPropertyRelative(nameof(flipAxis)), "F".ToContent("Flip"));
                GUI.enabled = true;
                EndHorizontal();
                if (withBox) EndVertical();
            }
#endif
        }
        
        protected static Vector3 GetPointOnLine(Vector3 p1, Vector3 p2, Vector3 point)
        {
            return p1 + Vector3.ClampMagnitude(Vector3.Project(point - p1, p2 - p1), (p2 - p1).magnitude);
        }

        protected static float GetDelta(TimeMode mode)
        {
            switch (mode)
            {
                case TimeMode.DeltaTime: return Time.deltaTime;
                case TimeMode.FixedDeltaTime: return Time.fixedDeltaTime;
                case TimeMode.UnscaledDeltaTime: return Time.unscaledDeltaTime;
                case TimeMode.UnscaledFixedDeltaTime: return Time.fixedUnscaledDeltaTime;
                default: return Time.deltaTime;
            }
        }

        #region Public Methods
        
        public void RCProLog(string message)
        {
#if UNITY_EDITOR
            RCProEditor.Log(message);
#endif
        }

        public void SetInfluence(float value) => Influence = value;

        public void AddInfluence(float value) => Influence += value;
        
        /// <summary>
        /// Add influence based on fixed deltaTime
        /// </summary>
        /// <param name="value"></param>
        public void AddFixedInfluence(float value) => Influence += value*Time.fixedDeltaTime;
        /// <summary>
        /// Add influence based on deltaTime
        /// </summary>
        /// <param name="value"></param>
        public void AddDeltaInfluence(float value) => Influence += value*Time.deltaTime;
        /// <summary>
        /// With self Parenting
        /// </summary>
        /// <param name="prefab"></param>
        public void InstantiateOnSelf(GameObject prefab)
        {
            Instantiate(prefab, transform.position, transform.rotation, transform);
        }
        /// <summary>
        /// Without parenting
        /// </summary>
        /// <param name="prefab"></param>
        public void InstantiateOnPoint(GameObject prefab)
        {
            Instantiate(prefab, transform.position, transform.rotation);
        }
        /// <summary>
        /// Instatiate in point with default rotation
        /// </summary>
        /// <param name="prefab"></param>
        public void InstantiateOnPointIdentity(GameObject prefab)
        {
            Instantiate(prefab, transform.position, Quaternion.identity);
        }
        #endregion

        protected bool InLayer(GameObject obj) => detectLayer == (detectLayer | (1 << obj.layer));
        protected bool InLayer(GameObject obj, LayerMask layerMask) => layerMask == (layerMask | (1 << obj.layer));
        
        protected abstract void OnCast();

        public void DestroySelf(float delay) => Destroy(gameObject, delay);
        protected static IEnumerator DelayRun(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);
            action();
        }
        protected static void IgnoreCollider(IEnumerable<Collider> colliders, Collider[] ignoreColliders)
        {
            foreach (var collider in colliders)
            {
                if (!collider) continue;

                foreach (var ignoreC in ignoreColliders)
                {
                    if (ignoreC) Physics.IgnoreCollision(collider, ignoreC);
                }
            }
        }
        protected static void IgnoreCollider(IEnumerable<Collider2D> colliders, Collider2D[] ignoreColliders)
        {
            foreach (var collider in colliders)
            {
                if (!collider) continue;

                foreach (var ignoreC in ignoreColliders)
                {
                    if (ignoreC) Physics2D.IgnoreCollision(collider, ignoreC);
                }
            }
        }
        protected void PathCastAll(List<Vector2> path, ref List<RaycastHit2D> output, float radius, float MinDepth,
            float MaxDepth)
        {
            output.Clear();
            
            if (radius > 0)
            {
                for (var i = 0; i < path.Count - 1; i++)
                {
                    var _dir = path[i + 1] - path[i];
                    output.AddRange(Physics2D.CircleCastAll(path[i], radius, _dir.normalized, _dir.magnitude, detectLayer.value, MinDepth, MaxDepth));
                }
            }
            else
            {
                for (var i = 0; i < path.Count - 1; i++)
                {
                    var _dir = path[i + 1] - path[i];
                    output.AddRange(Physics2D.RaycastAll(path[i], _dir.normalized, _dir.magnitude, detectLayer.value, MinDepth, MaxDepth));
                }
            }
        }

        protected void PathCastAll(List<Vector3> path, ref List<RaycastHit> output, float radius = 0)
        {
            output.Clear();
            
            if (radius > 0)
            {
                for (var i = 0; i < path.Count - 1; i++)
                {
                    var _dir = path[i + 1] - path[i];
                    output.AddRange(Physics.SphereCastAll(path[i], radius, _dir.normalized, _dir.magnitude, detectLayer.value, triggerInteraction));
                }
            }
            else
            {
                for (var i = 0; i < path.Count - 1; i++)
                {
                    var _dir = path[i + 1] - path[i];
                    output.AddRange(Physics.RaycastAll(path[i], _dir.normalized, _dir.magnitude, detectLayer.value, triggerInteraction));
                }
            }
        }
        protected static Vector3[] CircularPoints(Vector3 _p, float radius, Vector3 normal, Vector3 tangent,
            int count, bool closed = false)
        {
            var points = new Vector3[closed ? count + 1 : count];

            var step = 360f / count;
            
            for (var i = 0; i < count; i++)
            {
                var pos = Quaternion.AngleAxis(step * i, normal) * (tangent * radius) + _p;

                points[i] = pos;
            }

            if (closed) points[count] = points[0];

            return points;
        }
        
        protected static void CircularPointsNonAllocator(Vector3[] points, Vector3 position, float radius, Vector3 forward, Vector3 right)
        {
            var count = points.Length;
            var step = 360f / count;
            for (var i = 0; i < count; i++)
            {
                var pos = (Quaternion.AngleAxis(step * i, forward) * (right * radius)) + position;
                points[i] = pos.ToDepth(position.z);
            }
        }

        /// <summary>
        /// For 3D in local space
        /// </summary>
        protected static bool IsInPolygon(Vector3[] poly, Vector3 point)
        {
            var _tList = poly.Skip(1).Select((p, i) =>
                    (point.z - poly[i].z) * (p.x - poly[i].x)
                    - (point.x - poly[i].x) * (p.z - poly[i].z))
                .ToList();

            if (_tList.Any(p => p == 0))
                return true;

            for (var i = 1; i < _tList.Count; i++)
            {
                if (_tList[i] * _tList[i - 1] < 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// For 2D in local space
        /// </summary>
        protected static bool IsInPolygon2D(Vector2[] poly, Vector2 point)
        {
            var coef = poly.Skip(1).Select((p, i) =>
                    (point.y - poly[i].y) * (p.x - poly[i].x)
                    - (point.x - poly[i].x) * (p.y - poly[i].y))
                .ToList();

            if (coef.Any(p => p == 0))
                return true;

            for (int i = 1; i < coef.Count; i++)
            {
                if (coef[i] * coef[i - 1] < 0)
                    return false;
            }

            return true;
        }

        protected static T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            var type = original.GetType();
            var dst = destination.GetComponent(type) as T;
            if (!dst) dst = destination.AddComponent(type) as T;
            var fields = type.GetFields();
            foreach (var field in fields)
            {
                if (field.IsStatic) continue;

                var val = field.GetValue(original);
                if (val != null) field.SetValue(dst, val);
            }

            var props = type.GetProperties();
            foreach (var prop in props)
            {
                if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name") continue;
                var val = prop.GetValue(original, null);

                if (val != null) prop.SetValue(dst, val, null);
            }

            return dst;
        }
        
#if UNITY_EDITOR

        /// <summary>
        /// In Editor (No Playing) mode & Auto Update On. /n Use "Else"  when you want to force update ray in scene while update mode is off.
        /// </summary>
        protected internal bool IsManuelMode => !enabled && IsPlaying;
        
        
        protected bool EventFoldout = false;

        #region Const

        #region Strings

        protected const string HAccurate = " <color=#2BC6D2>#Accurate</color>";
        protected const string HUtility = " <color=#CB83FF>#Utility</color>";
        protected const string HDirectional = " <color=#D571FF>#Directional</color>";
        protected const string HPathRay = " <color=#88FF75>#PathRay</color>";
        protected const string HCDetector = " <color=#FFD238>#CDetector</color>";
        protected const string HRDetector = " <color=#35D6FF>#RDetector</color>";
        protected const string HExperimental = " <color=#FF5548>#Experimental</color>";
        protected const string HLOS_Solver = " <color=#2BC6D2>#LOS_Solver</color>";
        protected const string HRecursive = " <color=#FFFC26>#Recursive</color>";
        protected const string HVirtual = " <color=#FFFC26>#Virtual</color>";
        protected const string HPreview = " <color=#FFFC26>#Preview</color>";
        protected const string HDependent = " <color=#C1D133>#Dependent</color>";
        protected const string HIRadius = " <color=#6861D1>#IRadius</color>";
        protected const string HIPulse = " <color=#2EEBFF>#IPulse</color>";
        protected const string HINonAllocator = " <color=#87B06F>#NonAllocator</color>";
        protected const string HScalable = " <color=#B1976C>#Scalable</color>";
        protected const string HRotatable = " <color=#D18EC0>#Rotatable</color>";

        //private Color col = new Color(0.53f, 0.69f, 0.44f);

        protected const string CInformation = "Information";
        protected const string CInfluence = "Influence";
        protected const string TInfluence =
            "This auxiliary variable is defined for coefficient on other variables affected by ray. For e.g: gunDamage = influence * power";

        protected const string CDetectLayer = "Detect Layer";
        protected const string TDetectLayer = "This layer determines what objects will be sensitive to this sensor.";

        protected const string CBlockLayer = "Block Layer";
        protected const string TBlockLayer = "This layer determines which objects block the detector's line of sight.";

        protected const string CReflectLayer = "Reflect Layer";
        protected const string TReflectLayer = "Determines that ray can be reflected by objects of this layer";

        protected const string CTriggerInteraction = "Trigger Interaction";
        protected const string TTriggerInteraction =
            "Determine Ray behavior on \"IsTrigger\" colliders.\n\nUseGlobal: means using the Project settings. \n\nCollide: means ray collision with \"IsTrigger\" colliders.\n\nIgnore: means ray doesn't collide with \"IsTrigger\" colliders.";

        protected const string CCheckLineOfSight = "Check Line Of Sight";
        protected const string TCheckLineOfSight = "With this option active, the line of sight and blocking objects will be checked. Turning it off will help increase performance.";
        
        
        protected const string CBlockSolverOffset = "Block Solver Offset";
        protected const string TBlockSolverOffset =
            "If your view is different from the center point of the detector, you can change its offset and see the result.";

        protected const string CFocusPoint = "Focus Point";
        protected const string TFocusPoint = "It will tilt the detection point towards this axis. For example, the long forward axis creates a function similar to the human eye.";
        protected const string CLiner = "Liner";

        protected const string TLiner =
            "Liner is the abbreviation for Line Renderer, by this option installs a LineRenderer on the ray and automatically controls its graphical location by ray.";

        protected const string CAdd = "Add";
        protected const string CPoint = "Point";
        protected const string TPoint = "Point";

        protected const string COffset = "Offset";
        protected const string CCap = "Cap";
        protected const string CCorner = "Corner";
        protected const string CCamera = "Main Camera";

        protected const string CLength = "Length";
        protected const string TLength = "Length";

        protected const string CPower = "Power";
        protected const string TPower = "Power";

        protected const string CCollider = "Collider";

        protected const string CCastType = "Cast Type";
        protected const string TCastType = "Cast Type";

        protected const string CMoveType = "Move Type";
        protected const string TMoveType = "Move Type";

        protected const string CRaySensor = "RaySensor";
        protected const string TRaySensor = "Ray Sensor includes all Rays. (with 2D and 3D separation)";

        protected const string CSequenceOnTip = "Sequence On Tip";
        protected const string TSequenceOnTip = "This option causes the chain of rays to be placed behind each other. Be sure to click this option after combining the Rays and making sure they are not duplicated.";
        
        protected const string CPathCast = "Path Cast";
        protected const string TPathCast = "This option forces Ray to perform Raycast physical calculations, if you only need Path, it is better to keep it off just to improve performance.";
            
        protected const string CSpace = "Space";
        protected const string TSpace = "\nWorld: The world location accepts general directions and rotation has no effect on it." +
                                        "\nLocal: The local position will change depending on the rotation of the object.";

        protected const string CBullet = "Bullet";

        protected const string CCast = "Cast";
        protected const string TCast = "Cast";

        protected const string CRemove = "R";
        protected const string TRemove = "Remove";

        
        protected const string COuter = "Outer";
        protected const string TOuter = "It will clone ray same as received ray if this param null.";

        protected const string CBaseDirection = "Base Dircetion";
        protected const string TBaseDirection = "The output angle of Planar can be selected according to one of the following formulas.";
        
        protected const string CPoolManager = "Pool Manager";
        protected const string TPoolManager = "Automatically instantiate into this object.";
        
        protected const string CArrayCasting = "Array Casting";
        protected const string TArrayCasting = "This option keeps the number of bullets produced in a limited volume of the array to prevent the continuous production of Garbage. You will also have access to the array of bullets fired. To change the bullets of a different type, just use a different and desired Bullet ID so that they are automatically changed.";

        protected const string CLengthControl = "Length Controll";
        protected const string TLengthControl = "It will clone ray same as received ray if this param null.";

        protected const string CMultiplier = "Multiplier";
        protected const string CStartWidth = "Start Width";
        protected const string CEndWidth = "End Width";
        protected const string CGradient = "Gradient";

        protected const string CStamp = "Stamp";

        protected const string TStamp =
            "Stamp is a Transform that will place it on the Target Tip according the options.";

        protected const string CSyncAxis = "Sync Axis";
        protected const string TSyncAxis = "Allows transform Axis to sync with defined direction.";

        protected const string CStampOnHit = "Stamp On Hit";
        protected const string TStampOnHit = "When Enable stamp to move on hit instead of tip of ray.";

        protected const string CStampAutoHide = "Auto Hide";
        protected const string TStampAutoHide = "Auto Activate stamp when hit a collider.";

        protected const string CStampOffset = "Offset";
        protected const string TStampOffset = "Offset stamp from hit point around normal vector by amount.";

        protected const string CRadius = "Radius";
        protected const string TRadius = "Radius";

        protected const string CWaveSpeed = "Wave Speed";
        protected const string CNoise = "Noise";
        protected const string CScale = "Scale";

        protected const string CClump = "Clump";
        protected const string CClumpX = "ClumpX";
        protected const string CClumpY = "ClumpY";
        protected const string CClumpZ = "ClumpZ";
        
        protected const string CDigitStep = "DigitStep";

        protected const string CMinRadius = "Min Radius";
        protected const string TMinRadius = "The smallest possible radius.";

        protected const string CMaxRadius = "Max Radius";
        protected const string TMaxRadius = "The largest possible radius.";

        protected const string CEdgeCount = "Edge Count";
        protected const string TEdgeCount = "It's the number of sides in your Core that helps the smoothness of the ring, but the more the number, the more processing time it takes.";

        protected const string CHeight = "Height";
        protected const string THeight = "The height of Core in Detector or Ray, which you remember changes the type of processing to Capsule.";

        protected const string CRefreshTime = "Refresh Time";
        protected const string CCacheTime = "Cache Time";
        
        protected const string CExtents = "Extents";

        protected const string CGizmos = "Gizmos Update";
        protected const string TGizmos = "Define how to update gizmos frequantly.";

        protected const string CUpdate = "Auto Update";

        protected const string TUpdate =
            "Fixed (Recommended): will be processed with physics calculations and will be suitable for the condition. \n\nNormal: It is done at the same time as the usual update of Unity, which is suitable for graphic movements such as Liner movement.\n\nLate: is executed after performing physical movements and animations.\n\nOff: is suitable for manual ray casting and avoiding continuous processing, which you can call through the script by executing the Cast(out Hit) function. Consider that the update of gizmos will be more limited by activating it.";

        protected const string CNormal = "Normal";
        protected const string CEvents = "Events";

        protected const string TEvents =
            "Events are used to execute code commands, the header of each event determines the starting point of the code.";

        protected const string CDirection = "Direction";

        protected const string TDirection =
            "Covers direction and ray length together. \"Z\" value is a straight length at usual.";

        protected const string CSegments = "Segments";
        protected const string TSegments = "It is recommended to keep the number of points on the Ray as low as possible, which helps the smoothness of the line but makes the processing heavy.";

        protected const string CSpeed = "Speed";
        protected const string CDuration = "Duration";
        protected const string CCurve = "Curve";
        protected const string CCount = "Count";
        protected const string CAngle = "Angle";
        protected const string CCuts = "Cuts";
        protected const string CArcHorizontal = "Horizontal Arc";
        protected const string CArcVertical = "Vertical Arc";
        protected const string CArcAngle = "Arc Angle";
        protected const string CSubdivide = "Subdivide";
        protected const string TSubdivide = "Subdivide";

        protected const string CTarget = "Target";
        protected const string TTarget = "This option asks for the Core destination and makes decisions based on it.";
        protected const string CWeight = "Weight";
        protected const string CDistance = "Distance";
        protected const string CVelocity = "Velocity";
        protected const string CLight = "Light";

        protected const string CRelative = "Relative";
        protected const string TRelative = "When this feature is activated, the new point will be offset from its back position.";

        protected const string CReferenceType = "Reference Type";
        protected const string TReferenceType = "\nAuto: Automatic means automatic choice between virtual Clone Ray or normal clone. According to this system, Path Rays should be virtual cloned, and the rest of the Rays can use clones similar to themselves." +
                                                "\nReference: According to this parameter, you can choose a single ray to be cloned. For example, the clone of a Basic Ray can be a Pipe." +
                                                "\nClone: Instead of cloning a natural ray, this option creates a virtual ray that can copy and coordinate its previous ray points.";

        protected const string CSolverType = "LOS Solver";
        protected const string TSolverType = "This feature helps you quickly implement a point-based Los system on the Collider.";

        protected const string CBodyType = "Ray Body Type";
        protected const string TBodyType = "This option is related to the Ray body, where you may choose between Pipe and Line. If your Radius is zero, it is recommended to use Line directly.";

        protected const string CCollectAll = "Collect All";
        protected const string CAudio = "Audio";

        protected const string CPlanarSensitive = "Planar Sensitive";
        protected const string TPlanarSensitive = "Determine that the ray react to planers or not.";

        #endregion

        protected static Color DefaultColor => RCProPanel.DefaultColor;
        protected static Color DetectColor => RCProPanel.DetectColor;
        protected static Color HelperColor => RCProPanel.HelperColor;
        protected static Color BlockColor => RCProPanel.BlockColor;

        /// <summary>
        /// Get standard Amount from panel
        /// </summary>
        protected static float AlphaAmount => RCProPanel.alphaAmount;
        protected static float StepSizeLine => RCProPanel.raysStepSize;
        protected static float DiscSize => RCProPanel.normalDiscRadius;
        protected static float DotSize => RCProPanel.elementDotSize;

        protected static float LineSize = .4f;

        #endregion
        
        internal enum GizmosMode
        {
            Auto,
            Select,
            Fix,
            Off,
        }

        [SerializeField] internal GizmosMode gizmosUpdate = GizmosMode.Auto;

        protected Action GizmoGate;

        internal static Camera SceneCamera => GameObject.Find( "SceneCamera" ).GetComponent<Camera>();

        internal static bool InEditMode => !IsPlaying && IsSceneView;
        internal static bool IsSceneView => SceneView.currentDrawingSceneView;
        internal static bool IsPlaying => Application.isPlaying;
        internal static bool IsLabel => RCProPanel.ShowLabels;
        internal static bool IsGuide => RCProPanel.DrawGuide;
        internal static bool IsDetectLine => RCProPanel.DrawDetectLine;
        internal static bool IsBlockLine => RCProPanel.DrawBlockLine;
        internal abstract void OnGizmos();
        protected static void DrawDetectLine(Vector3 p1, Vector3 p2, RaycastHit hit, bool isDetect)
        {
            if (isDetect)
            {
                Gizmos.color = DetectColor;

                Gizmos.DrawLine(p1, hit.point);

                Handles.color = BlockColor;

                Handles.DrawDottedLine(hit.point, p2, StepSizeLine);
            }
            else
            {
                Gizmos.color = DefaultColor;

                DrawLineZTest(p1, p2);
            }
        }
        protected Color GizmoColor
        {
            set => Gizmos.color = Handles.color = value.Alpha(alphaCharge);
        }
        
        /// <summary>
        /// Set Gizmo And Handles Color Together;
        /// </summary>
        /// <param name="drawColor"></param>
        protected void DrawArrows(float length = 1)
        {
            var position = transform.position;
            var up = transform.up * length;
            var right = transform.right * length;
            var forward = transform.forward * length;

            Gizmos.DrawLine(position - up, position + up);
            Gizmos.DrawLine(position - right, position + right);
            Gizmos.DrawLine(position - forward, position + forward);
        }
        protected void DrawCross(Vector3 point, Vector3 normal)
        {
            var cross = Vector3.Cross(point - transform.position+new Vector3(0.01f,0.01f,0.01f), normal).normalized;
            
            Handles.DrawLine(point - cross * DotSize, point + cross * DotSize);
            var angleAxis = Quaternion.AngleAxis(90f ,normal) * cross;
            Handles.DrawLine(point - angleAxis * DotSize, point + angleAxis * DotSize);
        }
        protected static void DrawNormal(Vector3 point, Vector3 normal, string label = "",  float offset = 0, float radius = 0f)
        {
            Handles.color = HelperColor;
            
            Handles.DrawWireDisc(point, normal, radius > 0 ? radius : DiscSize);
            Handles.DrawLine(point, point + normal * DiscSize);
            if (RCProPanel.ShowLabels && label != "")
            {
                Handles.Label(point, label, RCProEditor.HeaderStyle);
            }
        }
        protected static void DrawNormal2D(RaycastHit2D Hit, float depth)
        {
            if (!Hit) return;
            Handles.color = HelperColor;
            var p = Hit.point.ToDepth(depth);
            Handles.DrawWireDisc(p, Hit.normal, DiscSize);
            Handles.DrawLine(p, p + Hit.normal.ToDepth() * LineSize);
            if (RCProPanel.ShowLabels)
            {
                Handles.Label(p, Hit.transform.name, RCProEditor.HeaderStyle);
            }
        }
        protected void DrawCapLine(Vector3 startPoint, Vector3 endPoint)
        {
            Handles.DrawDottedLine(startPoint, endPoint, StepSizeLine);

            DrawCap(endPoint, endPoint-startPoint);

            Handles.color = HelperColor.Alpha(alphaCharge);
            var right = transform.right *  DotSize;
            DrawWidthLine(startPoint, endPoint, right);
        }

        protected void DrawCapLine2D(Vector3 startPoint, Vector3 endPoint)
        {
            Handles.DrawDottedLine(startPoint, endPoint, StepSizeLine);

            Handles.ConeHandleCap(0, endPoint, Quaternion.LookRotation(endPoint - startPoint, Vector3.up), DotSize,
                EventType.Repaint);

            Handles.color = HelperColor.Alpha(alphaCharge);;
            var forward = transform.forward * DotSize;
            DrawWidthLine(startPoint, endPoint, forward);
        }
        protected void DrawCap(Vector3 point, Vector3 forward, float sizeMultiplier = 1)
        {
            Handles.ConeHandleCap(0, point, Quaternion.LookRotation(forward + new Vector3(0, 0, 0.01f), transform.up),
                DotSize * sizeMultiplier,
                EventType.Repaint);
        }
        protected void DrawBlockLine(Vector3 p1, Vector3 p2, Transform blocked, RaycastHit blockPoint, float alpha = 1f)
        {
            if (IsLabel) Handles.Label(p2, $"<color=#60FFF5>{blocked.name}</color> blocked by <color=#FF392B>{blockPoint.collider.name}</color>", RCProEditor.LabelStyle);
            if (!IsBlockLine) return;
            if (IsGuide)
            {
                Handles.color = BlockColor.Alpha(alpha);
                Handles.DrawDottedLine(p1, blockPoint.point, StepSizeLine);
                DrawCross(blockPoint.point, blockPoint.normal);
                Handles.color = Color.white.Alpha(alpha);
                Handles.DrawDottedLine(blockPoint.point, p2, StepSizeLine);
            }
        }
        protected void DrawBlockLine(Vector3 p1, Vector3 p2, RaycastHit blockHit, float alpha = 1)
        {
            DrawBlockLine(p1, p2, blockHit.transform, blockHit.point, alpha);
        }
        protected void DrawBlockLine(Vector3 p1, Vector3 p2, bool blocked, Vector3 blockPoint, float alpha = 1)
        {
            if (blocked)
            {
                Handles.color = DetectColor.Alpha(alpha);
                Handles.DrawLine(p1, blockPoint);
                Handles.color = BlockColor.Alpha(alpha);
                Handles.DrawDottedLine(blockPoint, p2, StepSizeLine);
            }
            else
            {
                Handles.color = DefaultColor.Alpha(alpha);
                Handles.DrawLine(p1, p2);
            }
        }
        protected void DrawBlockLine(Vector3 p1, Vector3 p2, RaycastHit2D blockPoint = default, float depth = 0, float alpha = 1)
        {
            if (blockPoint.transform)
            {
                Handles.color = DetectColor.Alpha(alpha);
                Handles.DrawLine(p1, blockPoint.point.ToDepth(depth));
                if (IsBlockLine)
                {
                    Handles.color = BlockColor.Alpha(alpha);
                    Handles.DrawDottedLine(blockPoint.point.ToDepth(depth), p2, StepSizeLine);
                }
            }
            else
            {
                Handles.color = DefaultColor.Alpha(alpha);
                Handles.DrawLine(p1, p2);
            }
        }
        protected void DrawBlockLine(Vector3 p1, Vector3 p2, Vector3 planeNormal, float radius = 0f,
            bool drawCross = true, bool drawSphereBase = false, bool drawSphereTarget = false,
            RaycastHit blockHit = default, float alpha = 1f)
        {
            if (blockHit.transform)
            {
                if (RCProPanel.DrawBlockLine)
                {
                    var breakOn = radius > 0 ? GetPointOnLine(p1, p2, blockHit.point) : blockHit.point;
                    Handles.color = DetectColor.Alpha(alpha);
                    DrawCapsuleLine(p1, breakOn, radius, forwardS: false);
                    if (drawCross) DrawCross(blockHit.point, blockHit.normal);
                    Handles.color = BlockColor.Alpha(alpha);
                    DrawCapsuleLine(breakOn, p2, radius, backS: false);
                }
            }
            else
            {
                if (RCProPanel.DrawDetectLine)
                {
                    Handles.color = DefaultColor.Alpha(alpha);
                    DrawCapsuleLine(p1, p2, radius);
                }
            }

            if (drawSphereBase) DrawSphere(p1, planeNormal, radius);
            if (drawSphereTarget) DrawSphere(p2, planeNormal, radius);
        }
        protected static void DrawCapsuleLine(Vector3 p1, Vector3 p2, float radius, float height = 0f, bool dotted = false, bool forwardS = true, bool backS = true, Transform _t = default)
        {
            var direction = (p2 - p1).normalized;
            Vector3 forward;
            Vector3 up;
            if (_t && height > 0)
            {
                forward = _t.forward;
                up = _t.up;
            }
            else
            {
                forward = (p2 - p1).normalized;
                up = Vector3.ProjectOnPlane(Vector3.up + Vector3.forward * .0001f, forward).normalized;
            }
            var dotZ = Vector3.Dot(forward, direction) > 0;
            var dotY = Vector3.Dot(up, direction) > 0;
            var h = height / 2;
            var projectDirection = Vector3.ProjectOnPlane(direction, up).normalized;
            var tAngle = Vector3.Angle(direction, projectDirection);
            float angle;
            if (dotY) angle = dotZ ? tAngle : 180 - tAngle;
            else angle = dotZ ? -tAngle : tAngle + 180;
            var cAngle = 90 - angle;
            var heightUp = up * h;
            var tRight = Vector3.Cross(direction, -up).normalized * radius;
            var rangeUp = Quaternion.AngleAxis(angle, tRight) * up * radius;
            var IRangeUp = Quaternion.AngleAxis(dotZ ? -angle : 180 - angle, dotZ ? tRight : -tRight) * up * radius;

            DrawZTest(() =>
            {
                if (height > 0) // Draw Height Pipe
                {
                    DrawLine(p1 + IRangeUp + heightUp, p2 + IRangeUp + heightUp, dotted);
                    DrawLine(p1 - IRangeUp - heightUp, p2 - IRangeUp - heightUp, dotted);

                    DrawLine(p1 + heightUp + tRight, p2 + heightUp + tRight, dotted);
                    DrawLine(p1 - heightUp + tRight, p2 - heightUp + tRight, dotted);

                    DrawLine(p1 + heightUp - tRight, p2 + heightUp - tRight, dotted);
                    DrawLine(p1 - heightUp - tRight, p2 - heightUp - tRight, dotted);

                    DrawLine(p1 + heightUp - tRight, p1 - heightUp - tRight, dotted);
                    DrawLine(p1 + heightUp + tRight, p1 - heightUp + tRight, dotted);
                    DrawLine(p2 + heightUp - tRight, p2 - heightUp - tRight, dotted);
                    DrawLine(p2 + heightUp + tRight, p2 - heightUp + tRight, dotted);

                    DrawLine(p1 + heightUp - projectDirection * radius, p1 - heightUp - projectDirection * radius, dotted);
                    DrawLine(p2 + heightUp + projectDirection * radius, p2 - heightUp + projectDirection * radius, dotted);
                
                    if (backS)
                    {
                        Handles.DrawWireArc(p1 + heightUp, direction, tRight, 180, radius);
                        Handles.DrawWireArc(p1 + heightUp, height > 0 ? up : -up, tRight, 180, radius);
                        Handles.DrawWireArc(p1 + heightUp, dotZ ? tRight : -tRight, -projectDirection, cAngle, radius);
                        //     
                        Handles.DrawWireArc(p1 - heightUp, direction, tRight, -180, radius);
                        Handles.DrawWireArc(p1 - heightUp, height > 0 ? up : -up, tRight, 180, radius);
                        Handles.DrawWireArc(p1 - heightUp, dotZ ? tRight : -tRight, -projectDirection,
                            dotZ ? -90 - angle : 270 - angle, radius);
                    }

                    if (forwardS)
                    {
                        Handles.DrawWireArc(p2 - heightUp, direction, tRight, -180, radius);
                        Handles.DrawWireArc(p2 - heightUp, height > 0 ? up : -up, tRight, -180, radius);
                        Handles.DrawWireArc(p2 - heightUp, dotZ ? tRight : -tRight, projectDirection, cAngle, radius);

                        Handles.DrawWireArc(p2 + heightUp, direction, tRight, 180, radius);
                        Handles.DrawWireArc(p2 + heightUp, height > 0 ? up : -up, tRight, -180, radius);
                        Handles.DrawWireArc(p2 + heightUp, dotZ ? tRight : -tRight, projectDirection,
                            dotZ ? -90 - angle : 270 - angle, radius);
                    }
                }
                else
                {
                    if (radius == 0)
                    {
                        DrawLine(p1, p2, dotted);

                        return;
                    }

                    DrawLine(p1 + rangeUp, p2 + rangeUp, dotted);
                    DrawLine(p1 - rangeUp, p2 - rangeUp, dotted);

                    DrawLine(p1 + tRight, p2 + tRight, dotted);
                    DrawLine(p1 - tRight, p2 - tRight, dotted);

                    if (backS)
                    {
                        Handles.DrawWireArc(p1 + heightUp, direction, tRight, 360, radius);
                        Handles.DrawWireArc(p1 + heightUp, height > 0 ? up : -up, tRight, -180, radius);
                        Handles.DrawWireArc(p1 + heightUp, tRight, up, -180, radius);
                    }
                    if (forwardS)
                    {
                        Handles.DrawWireArc(p2 + heightUp, direction, tRight, 360, radius);
                        Handles.DrawWireArc(p2 + heightUp, height > 0 ? up : -up, tRight, 180, radius);
                        Handles.DrawWireArc(p2 + heightUp, tRight, up, 180, radius);
                    }
                }
            });

        }
        protected internal static void DrawCircleLine(Vector3 p1, Vector3 p2, float radius = 0, bool dotted = false, RaycastHit2D hit2D = default, bool backHemi = true, bool forawrdHemi = true)
        {
            var _lDir = p2 - p1;
            var up = Vector3.Cross(p2 - p1, Vector3.forward).normalized * radius;

            DrawLineZTest(p1+up, p2+up, dotted);
            DrawLineZTest(p1-up, p2-up, dotted);

            if (backHemi)
            {
                Handles.DrawWireArc(p1, Vector3.forward, -_lDir, 90, radius);
                Handles.DrawWireArc(p1, Vector3.forward, -_lDir, -90, radius);
            }

            if (forawrdHemi)
            {
                Handles.DrawWireArc(p2, Vector3.forward, _lDir, 90, radius);
                Handles.DrawWireArc(p2, Vector3.forward, _lDir, -90, radius);
            }
        }
        protected internal void DrawCircleRay(Vector3 p1, Vector3 _dir, Vector3 _lDir,bool local,  float radius = 0, float height = 0)
        {
            
            _lDir = _lDir.To2D().normalized * _dir.magnitude;
            
            var p2 = (p1 + _lDir);
            Vector3 up;

            if (height > 0)
            {
                up = local ? transform.up.To2D().normalized : Vector2.up;

                if (local && Vector3.Dot(up, Vector3.up) < 0) up = -up;

                var halfHeight = height / 2;
                
                var side = Vector3.Dot(transform.right, Vector3.right) > 0 && _dir.x > 0 ? 1:-1;
                
                var right = (local ? Vector3.Cross(up, Vector3.forward) : Vector3.right) * radius*side;

                var ATAN = -Vector2.SignedAngle(_dir, Vector3.right * _dir.x);
                var Angle = Vector3.Dot(transform.forward, Vector3.forward) * ATAN;

                var pBB = p1 - right - up * halfHeight;
                var pBT = p1 - right + up * halfHeight;
                var pFB = p2 + right - up * halfHeight;
                var pFT = p2 + right + up * halfHeight;

                DrawLineZTest(pBB, pBT);
                
                DrawLineZTest(pFB, pFT);

                var angleOffsetPointUp = Quaternion.AngleAxis(Angle, Vector3.forward) * up * radius;
                var angleOffsetPointDown = Quaternion.AngleAxis(Angle, Vector3.forward) * -up * radius;
                
                var HH = up*halfHeight;
                
                DrawLineZTest(p1+angleOffsetPointUp+HH, p2+angleOffsetPointUp+HH);
                DrawLineZTest(p1+angleOffsetPointDown-HH, p2+angleOffsetPointDown-HH);


                Handles.DrawWireArc(p1 + HH, Vector3.forward, -right, -90*side + Angle, radius);
                Handles.DrawWireArc(p1 - HH, Vector3.forward, -right, 90*side + Angle, radius);
                
                Handles.DrawWireArc(p2 + HH, Vector3.forward, right, 90*side + Angle, radius);
                Handles.DrawWireArc(p2 - HH, Vector3.forward, right, -90*side + Angle, radius);
            }
            else
            {   
                up = Vector3.Cross(_lDir, Vector3.forward).normalized * radius;
                
                DrawLineZTest(p1+up, p2+up);
            
                DrawLineZTest(p1-up, p2-up);
                
                Handles.DrawWireArc(p1, Vector3.forward, -_lDir, 90, radius);
                Handles.DrawWireArc(p1, Vector3.forward, -_lDir, -90, radius);
                
                Handles.DrawWireArc(p2, Vector3.forward, _lDir, 90, radius);
                Handles.DrawWireArc(p2, Vector3.forward, _lDir, -90, radius);
            }
        }
        protected internal static void DrawZTest(Action draw, Color inside, Color outSide)
        {
            Handles.color = inside;
            Handles.zTest = CompareFunction.Greater;

            draw();

            Handles.color = outSide;
            Handles.zTest = CompareFunction.LessEqual;

            draw();
            
            Handles.zTest = CompareFunction.Always;
        }
        
        protected internal static void DrawZTest(Action draw)
        {
            Handles.zTest = CompareFunction.LessEqual;
            draw();
            Handles.color = Handles.color.Alpha(RCProPanel.alphaAmount);
            Handles.zTest = CompareFunction.Greater;
            draw();
            Handles.zTest = CompareFunction.Always;
        }
        protected internal static void DrawSphere(Vector3 position, Vector3 normal, float radius)
        {
            var transformUp = Vector3.ProjectOnPlane(Vector3.up + new Vector3(0, 0, 001f), normal).normalized * radius;
            var transformRight = Quaternion.AngleAxis(90, normal) * transformUp;
            void Draw()
            {
                Handles.DrawWireDisc(position, normal, radius);
                Handles.DrawWireDisc(position, transformUp, radius);
                Handles.DrawWireDisc(position, transformRight, radius);
            }

            DrawZTest(Draw, Handles.color.Alpha(RCProPanel.alphaAmount), Handles.color);
        }
        protected internal static void DrawLineZTest(Vector3 p1, Vector3 p2, bool dotted = false, Color color = default)
        {
            if (color != default)
            {
                Handles.color = color;
            }
            if (dotted)
            {
                DrawZTest(() => Handles.DrawDottedLine(p1, p2, StepSizeLine), Handles.color.Alpha(RCProPanel.alphaAmount),
                    Handles.color);
            }
            else
            {
                DrawZTest(() => Handles.DrawLine(p1, p2), Handles.color.Alpha(RCProPanel.alphaAmount), Handles.color);
            }
        }

        protected internal static void DrawLine(Vector3 p1, Vector3 p2, bool dotted = false)
        {
            if (dotted)
            {
                Handles.DrawDottedLine(p1, p2, StepSizeLine);
            }
            else
            {
                Handles.DrawLine(p1, p2);
            }
        }
        
        protected internal static void DrawThickLine(Vector3 p1, Vector3 p2, float thickness = 0f) => Handles.DrawLine(p1, p2, thickness);
        protected internal static void DrawDottedLine(Vector3 p1, Vector3 p2) => Handles.DrawDottedLine(p1, p2, StepSizeLine);
        protected internal static void DrawBox(Transform _t, Vector3 size, float zOffset = 0, bool local = true)
        {
            var pos = _t.position;
            
            var right = local ? _t.right : Vector3.right;
            var forward = local ? _t.forward : Vector3.forward;
            var up = local ? _t.up : Vector3.up;
            var rightSize = right * size.x;
            var sizeX = rightSize / 2;
            var upSize = up * size.y;
            var sizeY = upSize / 2;


            var forward_x_zOffset = forward * zOffset;

            Gizmos.DrawRay(pos - sizeX - sizeY + forward_x_zOffset, upSize);
            Gizmos.DrawRay(pos - sizeX - sizeY + forward_x_zOffset, rightSize);
            Gizmos.DrawRay(pos + sizeX + sizeY + forward_x_zOffset, -upSize);
            Gizmos.DrawRay(pos + sizeX + sizeY + forward_x_zOffset, -rightSize);
        }
        protected internal static void DrawBox2D(Transform p, Vector3 size, float minDepth, float maxDepth, bool local = false)
        {
            var pos = p.position;

            var right = local ? Vector3.ProjectOnPlane(p.right, Vector3.forward).normalized : Vector3.right;
            var forward = Vector3.forward;
            var up = Vector3.Cross(right, forward);
            
            var rightSize = right * size.x;
            var sizeX = rightSize / 2;
            var upSize = up * size.y;
            var sizeY = upSize / 2;
            
            Gizmos.DrawRay((pos - sizeX - sizeY).ToDepth(minDepth), upSize.ToDepth());
            Gizmos.DrawRay((pos - sizeX - sizeY).ToDepth(minDepth), rightSize.ToDepth());
            Gizmos.DrawRay((pos + sizeX + sizeY).ToDepth(minDepth), -upSize.ToDepth());
            Gizmos.DrawRay((pos + sizeX + sizeY).ToDepth(minDepth), -rightSize.ToDepth());
            
            Gizmos.DrawRay((pos - sizeX - sizeY).ToDepth(maxDepth), upSize.ToDepth());
            Gizmos.DrawRay((pos - sizeX - sizeY).ToDepth(maxDepth), rightSize.ToDepth());
            Gizmos.DrawRay((pos + sizeX + sizeY).ToDepth(maxDepth), -upSize.ToDepth());
            Gizmos.DrawRay((pos + sizeX + sizeY).ToDepth(maxDepth), -rightSize.ToDepth());
            
            DrawLineZTest((pos - sizeX - sizeY).ToDepth(minDepth), (pos - sizeX - sizeY).ToDepth(maxDepth), true);
            DrawLineZTest((pos + sizeX - sizeY).ToDepth(minDepth), (pos + sizeX - sizeY).ToDepth(maxDepth), true);
            DrawLineZTest((pos - sizeX + sizeY).ToDepth(minDepth), (pos - sizeX + sizeY).ToDepth(maxDepth), true);
            DrawLineZTest((pos + sizeX + sizeY).ToDepth(minDepth), (pos + sizeX + sizeY).ToDepth(maxDepth), true);
        }
        protected internal static void DrawSolidArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius)
        {
            Handles.DrawSolidArc(center, normal, from, angle / 2, radius);
            Handles.DrawSolidArc(center, normal, from, -angle / 2, radius);
        }
        protected internal static void DrawRectLines(Transform p, Vector3 size, float zOffset, float length, bool local = true)
        {
            var pos = p.position;
            var forward = local ? p.forward : Vector3.forward;
            var up = local ? p.up : Vector3.up;
            var right = local ? p.right : Vector3.right;
            Gizmos.DrawRay(pos - right * size.x / 2 - up * size.y / 2 + forward * zOffset, forward * length);
            Gizmos.DrawRay(pos - right * size.x / 2 + up * size.y / 2 + forward * zOffset, forward * length);

            Gizmos.DrawRay(pos + right * size.x / 2 + up * size.y / 2 + forward * zOffset, forward * length);
            Gizmos.DrawRay(pos + right * size.x / 2 - up * size.y / 2 + forward * zOffset, forward * length);
        }
        protected internal void DrawBoxLine(Vector3 p1, Vector3 p2, Vector3 extents, bool boxes = false, bool dotted = false)
        {
            var halfExtents = extents / 2;
            var halfExtentsZ = transform.forward * halfExtents.z;
            var halfExtentsY = transform.up * halfExtents.y;
            var halfExtentsX = transform.right * halfExtents.x;
            if (boxes)
            {
                var matrix = Gizmos.matrix;
                
                Gizmos.matrix = Matrix4x4.TRS(p1, transform.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, extents);
            
                Gizmos.matrix = Matrix4x4.TRS(p2, transform.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, extents);
            
                Gizmos.matrix = matrix;
            }

            DrawLineZTest(p1 - halfExtentsX + halfExtentsY - halfExtentsZ,
                p2 - halfExtentsX + halfExtentsY - halfExtentsZ, dotted);
            
            DrawLineZTest(p1 + halfExtentsX + halfExtentsY - halfExtentsZ,
                p2 + halfExtentsX + halfExtentsY - halfExtentsZ, dotted);
            DrawLineZTest(p1 - halfExtentsX - halfExtentsY + halfExtentsZ,
                p2 - halfExtentsX - halfExtentsY + halfExtentsZ, dotted);
            DrawLineZTest(p1 + halfExtentsX - halfExtentsY + halfExtentsZ,
                p2 + halfExtentsX - halfExtentsY + halfExtentsZ, dotted);
            DrawLineZTest(p1 - halfExtentsX - halfExtentsY - halfExtentsZ,
                p2 - halfExtentsX - halfExtentsY - halfExtentsZ, dotted);
            DrawLineZTest(p1 + halfExtentsX - halfExtentsY - halfExtentsZ,
                p2 + halfExtentsX - halfExtentsY - halfExtentsZ, dotted);
                
            DrawLineZTest(p1 - halfExtentsX + halfExtentsY + halfExtentsZ,
                p2 - halfExtentsX + halfExtentsY + halfExtentsZ, dotted);
            DrawLineZTest(p1 + halfExtentsX + halfExtentsY + halfExtentsZ,
                p2 + halfExtentsX + halfExtentsY + halfExtentsZ, dotted);
            
            //Advanced Box Ray not Supporting for now
            //
            // var dir = p2 - p1;
            // var dotZ = Vector3.Dot(dir, transform.forward) > 0;
            // var dotY = Vector3.Dot(dir, transform.up) > 0;
            // var dotX = Vector3.Dot(dir, transform.right);
            //
            // if (dotY ^ !dotZ) // Upper
            // {
            //     DrawLine(p1 - halfExtentsX + halfExtentsY - halfExtentsZ,
            //         p2 - halfExtentsX + halfExtentsY - halfExtentsZ, dotted);
            //
            //     DrawLine(p1 + halfExtentsX + halfExtentsY - halfExtentsZ,
            //         p2 + halfExtentsX + halfExtentsY - halfExtentsZ, dotted);
            //     DrawLine(p1 - halfExtentsX - halfExtentsY + halfExtentsZ,
            //         p2 - halfExtentsX - halfExtentsY + halfExtentsZ, dotted);
            //
            //     DrawLine(p1 + halfExtentsX - halfExtentsY + halfExtentsZ,
            //         p2 + halfExtentsX - halfExtentsY + halfExtentsZ, dotted);
            //
            //     if (dotX > 0)
            //     {
            //         DrawLine(p1 - halfExtentsX + halfExtentsY + halfExtentsZ,
            //             p2 - halfExtentsX + halfExtentsY + halfExtentsZ, dotted);
            //         DrawLine(p1 + halfExtentsX - halfExtentsY - halfExtentsZ,
            //             p2 + halfExtentsX - halfExtentsY - halfExtentsZ, dotted);
            //
            //     }
            //     else  if (dotX < 0)
            //     {
            //         DrawLine(p1 - halfExtentsX - halfExtentsY - halfExtentsZ,
            //             p2 - halfExtentsX - halfExtentsY - halfExtentsZ, dotted);
            //         DrawLine(p1 + halfExtentsX + halfExtentsY + halfExtentsZ,
            //             p2 + halfExtentsX + halfExtentsY + halfExtentsZ, dotted);
            //     }
            // }
            // else // Down
            // {
            //     DrawLine(p1 - halfExtentsX - halfExtentsY - halfExtentsZ,
            //         p2 - halfExtentsX - halfExtentsY - halfExtentsZ, dotted);
            //     DrawLine(p1 + halfExtentsX - halfExtentsY - halfExtentsZ,
            //         p2 + halfExtentsX - halfExtentsY - halfExtentsZ, dotted);
            //     
            //     DrawLine(p1 - halfExtentsX + halfExtentsY + halfExtentsZ,
            //         p2 - halfExtentsX + halfExtentsY + halfExtentsZ, dotted);
            //     DrawLine(p1 + halfExtentsX + halfExtentsY + halfExtentsZ,
            //         p2 + halfExtentsX + halfExtentsY + halfExtentsZ, dotted);
            //     
            //     if (dotX > 0)
            //     {
            //         DrawLine(p1 + halfExtentsX + halfExtentsY - halfExtentsZ,
            //             p2 + halfExtentsX + halfExtentsY - halfExtentsZ, dotted);
            //         
            //         DrawLine(p1 - halfExtentsX - halfExtentsY + halfExtentsZ,
            //             p2 - halfExtentsX - halfExtentsY + halfExtentsZ, dotted);
            //
            //     }
            //     else if (dotX < 0)
            //     {
            //         DrawLine(p1 - halfExtentsX + halfExtentsY - halfExtentsZ,
            //             p2 - halfExtentsX + halfExtentsY - halfExtentsZ, dotted);
            //         
            //         DrawLine(p1 + halfExtentsX - halfExtentsY + halfExtentsZ,
            //             p2 + halfExtentsX - halfExtentsY + halfExtentsZ, dotted);
            //     }
            // }
        }
        protected internal static void DrawWidthLine(Vector3 p1, Vector3 p2, Vector3 right)
        {
            Handles.DrawDottedLine(p1 + right, p2 + right, StepSizeLine);
            Handles.DrawDottedLine(p1 - right, p2 - right, StepSizeLine);

            Handles.DrawLine(p1 + right, p1 - right);
            Handles.DrawLine(p2 + right, p2 - right);
        }
        protected internal void DrawPath(List<Vector3> path, RaycastHit breakHit = default, float radius = 0f,
            bool coneCap = false, bool dotted = false, bool drawSphere = false, int detectIndex = -1, Color color = default)
        {
            if (path.Count == 0) return;

            for (var i = 0; i < path.Count - 1; i++)
            {
                if (detectIndex != i) // in break index
                {
                    if (detectIndex > -1) // with line detection
                    {
                        Handles.color = (breakHit.transform && i < detectIndex ? DetectColor : BlockColor).Alpha(alphaCharge);
                    }
                    else // without any detection
                    {
                        Handles.color = (color == default ? DefaultColor : color).Alpha(alphaCharge);
                    }
                    DrawCapsuleLine(path[i], path[i + 1], radius, dotted: dotted);
                }
                else
                {
                    Handles.color = DetectColor.Alpha(alphaCharge);
                    var breakOn = radius > 0
                        ? GetPointOnLine(path[i], path[i + 1], breakHit.point)
                        : breakHit.point;
                    DrawCapsuleLine(path[i], breakOn, radius, forwardS: false);

                    Handles.color = BlockColor.Alpha(alphaCharge);
                    DrawCapsuleLine(breakOn, path[i + 1], radius, backS: false);
                }
            }
            if (!coneCap || !RCProPanel.DrawGuide) return;
            Handles.color = HelperColor.Alpha(alphaCharge);
            DrawCap(path.Last(), radius > 0 ? radius : DotSize * 2, path.LastDirection(Vector3.forward));
        }
        protected void DrawPath2D(List<Vector3> path, Vector3 breakPoint, bool isDetect = false,
            float radius = 0f, bool pointLabel = false, bool drawDisc = false, bool coneCap = false,
            bool dotted = false, int detectIndex = -1, float z = 0, Color _color = default)
        {
            if (path.Count == 0) return;
            for (var i = 0; i < path.Count - 1; i++)
            {
                if (detectIndex != i)
                {
                    if (detectIndex > -1) // with line detection
                    {
                        GizmoColor = (isDetect && i < detectIndex ? DetectColor : BlockColor).Alpha(alphaCharge);
                    }
                    else // without any detection
                    {
                        GizmoColor = (_color == default ? DefaultColor : _color).Alpha(alphaCharge);
                    }
                    DrawCircleLine(path[i], path[i + 1], radius, dotted);
                }
                else
                {
                    if (drawDisc)
                    {
                        GizmoColor = DetectColor.Alpha(alphaCharge);
                        var breakOn = radius > 0 ? GetPointOnLine(path[i], path[i + 1], breakPoint) : breakPoint;
                        breakOn = breakOn.ToDepth(z);
                        
                        DrawCircleLine(path[i], breakOn, radius, forawrdHemi: false);
                        
                        GizmoColor = BlockColor.Alpha(alphaCharge);
                        DrawCircleLine(breakOn, path[i + 1], radius, backHemi: false);
                    }
                }

                if (drawDisc && radius > 0) Handles.DrawWireDisc(path[i + 1], Vector3.forward, radius);
                if (pointLabel) Handles.Label(path[i + 1], $"Point {i + 1}");
            }

            if (!coneCap) return;

            Handles.color = HelperColor.Alpha(alphaCharge);
            DrawCap(path.Last(), radius > 0 ? radius : DotSize * 2, path.LastDirection(Vector3.right));
        }

        private static void DrawCap(Vector3 position, float radius, Vector3 direction)
        {
            if (direction != Vector3.zero)
            {
                Handles.ConeHandleCap(0, position, Quaternion.LookRotation(direction),
                    radius > 0 ? radius : DotSize * 2,
                    EventType.Repaint);
            }
        }

        private bool InfoFoldout;
        private const string Information = "Information";
        protected void InformationField(Action action = null) // Information
        {
            if (action == null) return;

            InfoFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(InfoFoldout, Information.ToContent(), RCProEditor.HeaderFoldout);
            if (InfoFoldout)
            {
                BeginVerticalBox();
                action.Invoke();
                EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        protected static void PercentProgressField(float value, string label, int height = 20)
        {
            var rect = EditorGUILayout.GetControlRect(false, height);
            var percent = value;
            EditorGUI.ProgressBar(rect, percent, label + ": " + percent.ToString("P"));
        }
        protected static void ProgressField(float value, string label, int height = 20)
        {
            var rect = EditorGUILayout.GetControlRect(false, height);
            EditorGUI.ProgressBar(rect, value, label);
        }
        internal static void PropertyTimeModeField(SerializedProperty _timeMode)
        {
            PropertyEnumField(_timeMode, 5, "Time Mode".ToContent(), new []
            {
                "DT".ToContent("Delta Time"),
                "SDT".ToContent("Smooth Delta Time"),
                "FDT".ToContent("Fixed Delta Time"),
                "UDT".ToContent("Unscaled Delta Time"),
                "UFDT".ToContent("Unscaled Fixed Delta Time"),
            });
        }
        internal static void PropertyMaxField(SerializedProperty property,GUIContent label , float max = 0)
        {
            EditorGUILayout.PropertyField(property, label);
            property.floatValue = Mathf.Max(property.floatValue, max);
        }
        internal static void PropertyMaxField(SerializedProperty property, float max = 0)
        {
            EditorGUILayout.PropertyField(property);
            property.floatValue = Mathf.Max(property.floatValue, max);
        }
        internal static void PropertyMaxIntField(SerializedProperty property,GUIContent label , int max = 0)
        {
            EditorGUILayout.PropertyField(property, label);
            
            property.intValue = Mathf.Max(property.intValue, max);
        }
        internal static void PropertySliderField(SerializedProperty property, float leftValue, float rightValue, GUIContent label)
        {
            var rect = EditorGUILayout.GetControlRect();
            label = EditorGUI.BeginProperty(rect, label, property);

            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUI.Slider(rect, label, property.floatValue, leftValue, rightValue);
            if (EditorGUI.EndChangeCheck()) property.floatValue = newValue;

            EditorGUI.EndProperty();
        }
        internal static void PropertyIntSliderField(SerializedProperty property, int leftValue, int rightValue, GUIContent label)
        {
            var rect = EditorGUILayout.GetControlRect();
            label = EditorGUI.BeginProperty(rect, label, property);

            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUI.IntSlider(rect, label, property.intValue, leftValue, rightValue);
            if (EditorGUI.EndChangeCheck()) property.intValue = newValue;

            EditorGUI.EndProperty();
        }
        internal static void PropertySliderField(SerializedProperty property, int leftValue, int rightValue, GUIContent label, Action<int> onChange)
        {
            var rect = EditorGUILayout.GetControlRect();
            label = EditorGUI.BeginProperty(rect, label, property);

            EditorGUI.BeginChangeCheck();
            var newValue = EditorGUI.IntSlider(rect, label, property.intValue, leftValue, rightValue);
   
            if (EditorGUI.EndChangeCheck())
            {
                property.intValue = newValue;
                
                onChange?.Invoke(newValue);
            }
            EditorGUI.EndProperty();
        }
        internal static void PropertyMinMaxField(SerializedProperty minProp, SerializedProperty maxProp, ref float min, ref float max, float minValue = 0f, float maxValue = 0f)
        {
            var rect = EditorGUILayout.GetControlRect(false, 18);
            EditorGUI.BeginProperty(rect, GUIContent.none, minProp);
            EditorGUI.BeginProperty(rect, GUIContent.none, maxProp);
            EditorGUILayout.GetControlRect(false, -18);
            BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            min = EditorGUILayout.FloatField(min, GUILayout.Width(40f));
            EditorGUILayout.MinMaxSlider( ref min, ref max, minValue, maxValue);
            max = EditorGUILayout.FloatField(max, GUILayout.Width(40f));
            EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                minProp.floatValue = min;
                maxProp.floatValue = max;
            }
            EditorGUI.EndProperty();
            EditorGUI.EndProperty();
        }
        internal static void PropertyEnumField(SerializedProperty property, int xCount, GUIContent label, GUIContent[] tips)
        {
            var rect = EditorGUILayout.GetControlRect();

            if (property.prefabOverride)
            {
                var guiStyle = RCProEditor.HeaderStyle;
                
                guiStyle.fontStyle = FontStyle.Bold;
                guiStyle.alignment = TextAnchor.UpperCenter;

                GUI.Label(rect, label, guiStyle);
            }
            else
            {
                var guiStyle = RCProEditor.HeaderStyle;
                
                guiStyle.fontStyle = FontStyle.Normal;
                guiStyle.alignment = TextAnchor.UpperCenter;

                GUI.Label(rect, label, guiStyle);
            }

            EditorGUI.BeginProperty(rect, label, property);
            EditorGUI.BeginChangeCheck();
            var newValue = GUILayout.SelectionGrid(property.enumValueIndex, tips, xCount);
            if (EditorGUI.EndChangeCheck()) property.enumValueIndex = newValue;
            EditorGUI.EndProperty();
        }
        protected static void LocalField(SerializedProperty localProp) => MiniField(localProp, "L".ToContent("Local"));
        protected static void ScaleField(SerializedProperty localProp) => MiniField(localProp, "S".ToContent("Scalable"));
        protected static void MiniField(SerializedProperty localProp, GUIContent content)
        {
            var style = new GUIStyle(EditorStyles.label);
            if (localProp.prefabOverride)
            {
                style.fontStyle = FontStyle.BoldAndItalic;
                EditorGUILayout.LabelField(content, style, GUILayout.Width(15f));
            }
            else
            {
                style.fontStyle = FontStyle.Normal;
                EditorGUILayout.LabelField(content, style, GUILayout.Width(15f));
            }
            
            EditorGUILayout.PropertyField(localProp, GUIContent.none,
                true, GUILayout.Width(15f));
        }
        protected void RadiusField(SerializedObject _so)
        {
            var prop = _so.FindProperty("radius");
            EditorGUILayout.PropertyField(prop,
                CRadius.ToContent(TRadius));
            prop.floatValue = Mathf.Max(0, prop.floatValue);
        }
        protected void RadiusField(SerializedObject _so, string propName, GUIContent label)
        {
            var prop = _so.FindProperty(propName);
            EditorGUILayout.PropertyField(prop, label);
            prop.floatValue = Mathf.Max(0, prop.floatValue);
        }
        protected void HeightField(SerializedObject _so)
        {
            var h = _so.FindProperty("height");
            EditorGUILayout.PropertyField(h, CHeight.ToContent(CHeight));
            h.floatValue = Mathf.Max(0, h.floatValue); 
        }
        protected void ExtentsField(SerializedObject _so) => EditorGUILayout.PropertyField(_so.FindProperty("extents"), CExtents.ToContent(CExtents));
        // ReSharper disable Unity.PerformanceAnalysis
        protected void BaseField(SerializedObject _so, bool hasInfluence = true, bool hasInteraction = true, bool hasUpdateMode = true, bool hasGizmoUpdate = true)
        {
            if (hasInfluence)
            {
                PropertySliderField(_so.FindProperty("influence"), 0f, 1f, CInfluence.ToContent(TInfluence));
            }
            if (hasInteraction)
            {
                EditorGUILayout.PropertyField(_so.FindProperty("triggerInteraction"),
                    CTriggerInteraction.ToContent(TTriggerInteraction));
            }
            if (hasUpdateMode)
            {
                if (enabled)
                {
                    EditorGUILayout.PropertyField(_so.FindProperty("autoUpdate"),
                        CUpdate.ToContent(TUpdate));
                }
                else
                {
                    BeginHorizontalBox();
                    EditorGUILayout.LabelField("Auto Update is OFF.".ToContent("You can manually trigger core via \"Cast()\" method"), RCProEditor.LabelStyle);
                    if (IsPlaying && GUILayout.Button("Cast"))
                    {
                        TestCast();
                    }
                    EndHorizontal();
                }
            }
            if (hasGizmoUpdate)
            {
                BeginVerticalBox();
                var label = CGizmos.ToContent(TGizmos);
                var tips = new[]
                {
                    "Auto".ToContent("Show Gizmos when current 'Core' are running"), "Select".ToContent("Only Show gizmos when select object."),
                    "Fix".ToContent("Always show gizmos except when component is collapsed."), "Off".ToContent("Off")
                };
                
                PropertyEnumField(_so.FindProperty("gizmosUpdate"), 4, label, tips);
                EndVertical();
            }
        }

        internal void TestCast()
        {
            // This is special Test Cast that I added this line of code here too.
#if UNITY_EDITOR
            alphaCharge = AlphaLifeTime;
#endif
            
            if (this is RaySensor rS)
            {
                rS.RuntimeUpdate();
            }
            else if (this is RaySensor2D rS2D)
            {
                rS2D.RuntimeUpdate();
            }
            else if (this is BaseDetector bD)
            {
                bD.OnCast();
            }
        }
        internal static BodyType BodyTypeField(BodyType bodyType, ref float radius)
        {
            var _bodyType = RCProEditor.EnumLabelField(bodyType, CBodyType.ToContent(TBodyType));
            if (bodyType == BodyType.Pipe) radius = EditorGUILayout.FloatField(CRadius, radius);
            return _bodyType;
        }
        protected void DirectionField(SerializedObject _so, string directionReference)
        {
            BeginHorizontal();
            
            EditorGUILayout.PropertyField(_so.FindProperty(directionReference), CDirection.ToContent(TDirection),
                true);

            LocalField(_so.FindProperty("local"));

            EndHorizontal();
        }

        protected static void WeightField(SerializedProperty weightType, SerializedProperty _weight, SerializedProperty distance, SerializedProperty offset)
        {
            BeginVerticalBox();
            
            PropertyEnumField(weightType, 3, "Weight Type".ToContent(), new [] {"Clamp".ToContent(), CDistance.ToContent(CDistance), COffset.ToContent(COffset)});

            switch (weightType.enumValueIndex)
            {
                case 0:
                    PropertySliderField(_weight, 0, 1, CWeight.ToContent(CWeight));
                    break;
                case 1:
                    EditorGUILayout.PropertyField(distance, CDistance.ToContent(CDistance));
                    break;
                case 2:
                    EditorGUILayout.PropertyField(offset, COffset.ToContent(COffset));
                    break;
            }

            EndVertical();
        }

        internal static void InLabelWidth(Action layout, int width = 0)
        {
            var originalValue = EditorGUIUtility.labelWidth;
            
            EditorGUIUtility.labelWidth = width;
                    
            layout.Invoke();

            EditorGUIUtility.labelWidth = originalValue;
        }
        internal static void BeginVerticalBox() => EditorGUILayout.BeginVertical(RCProEditor.BoxStyle);
        internal static void BeginHorizontalBox() => EditorGUILayout.BeginHorizontal(RCProEditor.BoxStyle);
        internal static void EndVertical() => EditorGUILayout.EndVertical();
        internal static void EndHorizontal() => EditorGUILayout.EndHorizontal();
        internal static void BeginVertical() => EditorGUILayout.BeginVertical();
        internal static void BeginHorizontal() => EditorGUILayout.BeginHorizontal();

        protected const string CArrayLength = "Array Length";
        protected const string CNonAllocator = "Non Allocator";
        protected const string TArrayLength = "Non Allocator array size. Make sure that the entered size includes all Colliders even the filtered ones. (In some detectors such as Sight, the Sphere core is used, so set the size sufficiently.)";
        protected const string TNonAllocator =
            "By activating this option, the number of colliders identified in an array will be limited. While the performance is reduced to some extent, the garbage production is greatly reduced.";
        protected void DetectLayerField(SerializedObject _so) => EditorGUILayout.PropertyField(_so.FindProperty("detectLayer"), CDetectLayer.ToContent(TDetectLayer));
        protected void NonAllocatorField(SerializedObject _so, SerializedProperty arrayProp)
        {
            BeginHorizontal();
            var limited = _so.FindProperty("limited");
            var limitCount = _so.FindProperty("limitCount");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(limited,
                limited.boolValue ? CArrayLength.ToContent(TArrayLength) : CNonAllocator.ToContent(TNonAllocator));
            GUI.enabled = limited.boolValue;
            PropertyMaxIntField(limitCount, GUIContent.none);
            if (EditorGUI.EndChangeCheck() && limited.boolValue) arrayProp.arraySize = limitCount.intValue;
            GUI.enabled = true;
            EndHorizontal();
        }
        protected void NonAllocatorField<R>(SerializedObject _so, ref R[] nonAllocArray, Action<int> countSetup)
        {
            BeginHorizontal();
            var limited = _so.FindProperty("limited");
            var limitCount = _so.FindProperty("limitCount");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(limited,
                limited.boolValue ? CArrayLength.ToContent(TArrayLength) : CNonAllocator.ToContent(TNonAllocator));
            GUI.enabled = limited.boolValue;
            PropertyMaxIntField(limitCount, GUIContent.none);
            if (EditorGUI.EndChangeCheck() && limited.boolValue)
            {
                Array.Resize(ref nonAllocArray, limitCount.intValue);
                countSetup?.Invoke(limitCount.intValue);
            }

            GUI.enabled = true;
            EndHorizontal();
        }

        #region Gizmos

        internal float AlphaLifeTime => RCProPanel.gizmosOffTime;
        protected float alphaCharge = 0;
        protected float ClampedAlphaCharge => Mathf.Clamp01(alphaCharge);
        private bool LifePass {
            get
            {
                if (!IsPlaying || enabled)
                {
                    alphaCharge = 1f;
                    return true;
                }

                alphaCharge -= Time.unscaledDeltaTime;
                return alphaCharge > 0;
            }
        }
        internal void OnDrawGizmos()
        {
            if (gizmosUpdate == GizmosMode.Fix)
            {
                alphaCharge = 1;
                OnGizmos();
            }
            else if (gizmosUpdate == GizmosMode.Auto)
            {
                if (IsPlaying)
                {
                    if (Performed || Selection.activeTransform == transform)
                    {
                        if (LifePass) OnGizmos();
                    }
                }
                else if (CheckParent(transform))
                {
                    {
                        if (LifePass) OnGizmos();
                    }
                }
            }
        }

        private bool CheckParent(Transform current)
        {
            if (Selection.activeTransform == current) return true;
            
            if (current.parent) return CheckParent(current.parent);
            
            return false;
        }
        internal void OnDrawGizmosSelected()
        {
            if (!gameObject.activeInHierarchy) return;
            
            if (gizmosUpdate == GizmosMode.Select)
            {
                alphaCharge = 1;
                OnGizmos();
            }
        }

        #endregion
        
        internal abstract void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true, bool hasEvents = true, bool hasInfo = true);
#endif
    }
}