namespace RaycastPro.RaySensors
{
    using System.Collections.Generic;
    using Planers;
    using UnityEngine;

#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    public abstract class RaySensor : BaseRaySensor<RaycastHit, RaycastEvent, Planar>
    {
        public override bool Performed
        {
            get => hit.transform;
            protected set { }
        }
        
        /// <summary>
        /// Check if Hit in defined "Tag".
        /// </summary>
        /// <param name="_tag"></param>
        /// <returns></returns>
        public bool HitInTag(string _tag) => hit.transform.CompareTag(_tag);
        public bool HitInLayer(LayerMask mask)
        {
            return hit.transform && mask == (mask | 1 << hit.transform.gameObject.layer);
        }
        
        internal RaySensor baseRaySensor;
        internal RaySensor cloneRaySensor;
        /// <summary>
        /// Ray direction in World space.
        /// </summary>
        public Vector3 direction = Vector3.forward;
        
        #region Lambdas

        /// <summary>
        /// The final point of the ray in terms of the presence of Hit.
        /// </summary>
        public override Vector3 TipTarget => hit.transform ? hit.point : Tip;

        /// <summary>
        /// Direct length of the Ray from base to tip. Equivalent: (direction.magnitude)
        /// </summary>
        public float DirectionLength => direction.magnitude;

        /// <summary>
        /// The distance of the ray to the hit point. returns Length If there is not Hit.
        /// </summary>
        public float HitDistance => hit.transform ? (hit.point - Base).magnitude : TipLength;
        
        /// <summary>
        /// The length traveled from Ray to reach the target point
        /// </summary>
        public virtual float HitLength => HitDistance;

        /// <summary>
        /// The direction of the ray at the Hit Point. (not Normalized)
        /// </summary>
        public virtual Vector3 HitDirection => hit.transform ? hit.point - Base : Direction;
        
        /// <summary>
        /// Ray direction in Selected Space. (not Normalized)
        /// </summary>
        public Vector3 Direction => local ? LocalDirection : direction;
        
        /// <summary>
        /// Ray direction in Selected Space with full scaling direction
        /// </summary>
        public Vector3 ScaledDirection => Vector3.Scale(transform.lossyScale, Direction);

        public float FlatScale => (transform.lossyScale.x + transform.lossyScale.y) / 2f;
        
        /// <summary>
        /// Ray direction in Local space. (not Normalized)
        /// </summary>
        public Vector3 LocalDirection => transform.TransformDirection(direction);

        /// <summary>
        /// The remaining distance from the ray trail to the Hit Point. Returns Length if there is not a hit.
        /// </summary>
        public virtual float ContinuesDistance => hit.transform ? (Tip-hit.point).magnitude : DirectionLength;

        /// <summary>
        /// In case of collision, it returns the direction of Flat normal, otherwise it returns the direction base to the tip.
        /// </summary>
        public override Vector3 TargetDirection => hit.transform ? -hit.normal : TipDirection;

        /// <summary>
        /// Calculate the angle of inclination based on the up axis
        /// </summary>
        public float EdgeSlop => Vector3.Angle(hit.normal, local ? transform.up : Vector3.up);

        public RaySensor LastClone
        {
            get
            {
                var sensor = this;
                
                while (true)
                {
                    var _clone = sensor.cloneRaySensor;

                    if (_clone)
                    {
                        sensor = _clone;
                        continue;
                    }

                    return sensor;
                }
            }
        }
        
        #endregion

        #region Public Methods
        
        public Vector3 GetPositionOnPath(float distance)
        {
            var rayPath = new List<Vector3>();
            
            GetPath(ref rayPath);
            
            for (var i = 1; i < rayPath.Count; i++)
            {
                var edgeLength = rayPath.GetEdgeLength(i);
                if (distance <= edgeLength)
                {
                    return Vector3.Lerp(rayPath[i - 1], rayPath[i], distance / edgeLength);
                }
                distance -= edgeLength;
            }
            return Base;
        }

        public Vector3 HitOffsetByNormal(float value) => hit.point + hit.normal * value;
        
        public Vector3 HitOffsetByReverseDirection(float value) => hit.point - LocalDirection * value;

        public void SetDirection(Vector3 newDirection) => direction = newDirection;

        public void AddDirection(Vector3 vector) => direction += vector;

        public void SetHitActive(bool toggle)
        {
            if (hit.transform) hit.transform.gameObject.SetActive(toggle);
        }

        public void DestroyHit(float delay)
        {
            if (hit.transform) Destroy(hit.transform.gameObject, delay);
        }

        public void SetTargetPosition(Vector3 newPosition)
        {
            if (hit.transform) hit.transform.position = newPosition;
        }

        public void TranslateTargetPosition(Vector3 vector)
        {
            if (hit.transform) hit.transform.Translate(vector);
        }

        public void InstantiateTargetObject(Vector3 location)
        {
            if (hit.transform) Instantiate(hit.transform, location, Quaternion.LookRotation(TipDirection));
        }

        public void AddForceAlongNormal(float force)
        {
            if (hit.transform.TryGetComponent(out Rigidbody body))
            {
                body.AddForce(hit.normal * force);
            }
        }

        public void AddForceAlongHitDirection(float force)
        {
            if (hit.transform.TryGetComponent(out Rigidbody body))
            {
                body.AddForce(HitDirection.normalized * force);
            }
        }

        public void AddForceAlongTipDirection(float force)
        {
            if (hit.transform.TryGetComponent(out Rigidbody body))
            {
                body.AddForce(TipDirection.normalized * force);
            }
        }

        public void AddDynamicForceAlongTipDirection(float force)
        {
            if (hit.transform.TryGetComponent(out Rigidbody body))
            {
                body.AddForce(TipDirection.normalized * ContinuesDistance * force);
            }
        }

        public void AddDynamicForceAlongNormal(float force)
        {
            if (hit.transform.TryGetComponent(out Rigidbody body))
            {
                body.AddForce(hit.normal * ContinuesDistance * force);
            }
        }

        public void AddDynamicForceAlongHitDirection(float force)
        {
            if (hit.transform.TryGetComponent(out Rigidbody body))
            {
                body.AddForce(HitDirection.normalized * ContinuesDistance * force);
            }
        }

        public void PlaySoundAtHitPoint(AudioClip clip) => AudioSource.PlayClipAtPoint(clip, hit.point, 1f);

        public T GetHitComponent<T>() => hit.transform.GetComponent<T>();

        /// <summary>
        /// Get Hit point Material Color 
        /// </summary>
        public Color HitColor => hit.GetColor();
        /// <summary>
        /// Directly get current hit material detection. #Detection
        /// </summary>
        public Material HitMaterial => hit.GetMaterial();
        /// <summary>
        /// Get Hit (Terrain) currently most alpha map value Index. (return's -1 in default)
        /// </summary>
        public int HitTerrainIndex => hit.GetTerrainIndex();
        /// <summary>
        /// Get Array of alpha map value on hit Point.
        /// </summary>
        /// <param name="alphasValues"></param>
        public void GetHitTerrainAlpha(ref float[] alphasValues) => hit.GetTerrainAlpha(ref alphasValues);
        internal static int GetSubMeshIndex(Mesh mesh, int triangleIndex)
        {
            if (!mesh.isReadable) return 0;
            var triangleCounter = 0;
            for (var subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; subMeshIndex++)
            {
                var indexCount = mesh.GetSubMesh(subMeshIndex).indexCount;
                triangleCounter += indexCount / 3;
                if (triangleIndex < triangleCounter) return subMeshIndex;
            }
            return 0;
        }
        #endregion
        public override bool ClonePerformed => CloneHit.transform;
        public RaycastHit CloneHit => cloneRaySensor ? cloneRaySensor.CloneHit : hit;
        public Vector3 Normal => hit.normal;
        public Vector3 HitPoint => hit.point;

        public virtual void GetPath(ref List<Vector3> path, bool onHit = false)
        {
            path = new List<Vector3>() {Base, onHit ? TipTarget : Tip};
        }

        /// <summary>
        /// Update attached Stamp Transform
        /// </summary>
        public override void UpdateStamp()
        {
            if (!stamp) return;
            // Stamp Planar Activate Fix
            if (cloneRaySensor && cloneRaySensor.enabled) return;
            var _st = stamp.transform;
            if (stampOnHit && hit.transform)
            {
                _st.position = TipTarget;
                if (syncStamp.syncAxis)
                {
                    switch (syncStamp.axis)
                    {
                        case Axis.X: _st.right = hit.normal * (syncStamp.flipAxis ? -1 : 1); break;
                        case Axis.Z: _st.forward = hit.normal * (syncStamp.flipAxis ? -1 : 1); break;
                        case Axis.Y: _st.up = hit.normal * (syncStamp.flipAxis ? -1 : 1); break;
                    }
                }

            }
            else
            {
                _st.position = Tip;
                if (syncStamp.syncAxis)
                {
                    switch (syncStamp.axis)
                    {
                        case Axis.X:
                            _st.right = TipDirection * (syncStamp.flipAxis ? 1 : -1);
                            break;
                        case Axis.Z:
                            _st.forward = TipDirection * (syncStamp.flipAxis ? 1 : -1);
                            break;
                        case Axis.Y:
                            _st.up = TipDirection * (syncStamp.flipAxis ? 1 : -1);
                            break;
                    }
                }
            }

            if (syncStamp.syncAxis)
            {
                _st.position += hit.normal * stampOffset;
            }
            else
            {
                _st.position -= HitDirection.normalized * stampOffset;
            }
        }
        
        /// <summary>
        /// Update attached Line Renderer
        /// </summary>
        public override void UpdateLiner() // The override works on normal rays
        {
            if (!liner) return;
            
            if (linerClamped)
            {
                liner.positionCount = 2;
                if (linerCutOnHit)
                {
                    _base = Base;
                    var hitPoint = LinerFixCut ? GetPointOnLine(_base, Tip, hit.point) : hit.point;
                    var _pos = hit.transform ? (hitPoint - _base).magnitude / RayLength : 1f;

                    if (_pos >= linerBasePosition)
                    {
                        liner.SetPosition(0, Vector3.Lerp(_base, Tip, linerBasePosition));
                        liner.SetPosition(1, _pos < linerEndPosition ? hitPoint : Vector3.Lerp(_base, Tip, linerEndPosition));
                    }
                    else liner.positionCount = 0;
                }
                else
                {
                    _tip = Tip;
                    _base = Base;
                    liner.SetPosition(0, Vector3.Lerp(_base, _tip, linerBasePosition));
                    liner.SetPosition(1, Vector3.Lerp(_base, _tip, linerEndPosition));
                }
            }
            else // USE Full Clamp
            {
                _base = Base;
                liner.positionCount = 2;
                liner.SetPosition(0, Base);
                if (linerCutOnHit)
                {
                    _tip = TipTarget;
                    liner.SetPosition(1, LinerFixCut ? _base + Vector3.Project(_tip-_base, Direction) : _tip);
                }
                else
                {
                    liner.SetPosition(1, Tip);
                }
            }
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        internal override void RuntimeUpdate()
        {
            OnCast();
            onCast?.Invoke();

            /// Liner will Going to modifiers at V2.0
            UpdateLiner();
            UpdateStamp();
            
            if (hit.transform) OnDetect();
            if (PreviousHit.transform != hit.transform)
            {
                onChange?.Invoke(hit);
                // end Event most be top of begin
                if (PreviousHit.transform)
                { OnEndDetect(); }
                if (hit.transform)
                { OnBeginDetect(); }
            }
            PreviousHit = hit;
        }
        internal override void OnDetect()
        {
            if (planarSensitive)
            {
                if (anyPlanar)
                {
                    if (!_planar) return;

                    _planar.OnReceiveRay(this);
                    _planar.onReceiveRay?.Invoke(this);
                }
                else
                {
                    foreach (var p in planers)
                    {
                        if (!p || p.transform != hit.transform) continue;

                        p.OnReceiveRay(this);
                        p.onReceiveRay?.Invoke(this);
                    }
                }
            }
            onDetect?.Invoke(hit);
        }
        internal override void OnEndDetect()
        {
            if (stampAutoHide) stamp?.gameObject.SetActive(false);
            if (planarSensitive)
            {
                if (anyPlanar)
                {
                    if (!_planar) return;
                    _planar.OnEndReceiveRay(this);
                    _planar.onEndReceiveRay?.Invoke(this);
                    _planar = null;
                }
                else
                {
                    foreach (var p in planers)
                    {
                        if (!p || p.transform != PreviousHit.transform) continue;
                        p.OnEndReceiveRay(this);
                        p.onEndReceiveRay?.Invoke(this);
                    }
                }
            }
            onEndDetect?.Invoke(PreviousHit);
        }
        internal override void OnBeginDetect()
        {
            if (stampAutoHide) stamp?.gameObject.SetActive(true);
            if (planarSensitive)
            {
                if (anyPlanar)
                {
                    _planar = hit.transform.GetComponent<Planar>();
                    if (!_planar) return;
                    _planar.OnBeginReceiveRay(this);
                    _planar.onBeginReceiveRay?.Invoke(this);
                }
                else
                {
                    foreach (var p in planers)
                    {
                        if (!p || p.transform != hit.transform) continue;
                        p.OnBeginReceiveRay(this);
                        p.onBeginReceiveRay?.Invoke(this);
                    }
                }
            }
            onBeginDetect?.Invoke(hit);
        }
        public static void CloneDestroy(RaySensor sensor)
        {
            while (true)
            {
                if (!sensor || !sensor.gameObject) return;
                if (sensor.cloneRaySensor)
                {
                    sensor = sensor.cloneRaySensor;
                    continue;
                }
                Destroy(sensor.gameObject);
                break;
            }
        }
        // This function will destroy every clone before destroy the main
        internal override void SafeRemove()
        {
            if (cloneRaySensor && cloneRaySensor.gameObject)
            {
                Destroy(cloneRaySensor.gameObject);
            }
        }

#if UNITY_EDITOR
        protected override void EditorUpdate()
        {
            if (!RCProPanel.realtimeEditor)
            {
                GizmoGate = null;
                hit = default;
                return;
            }
            if (!IsPlaying && IsSceneView)
            {
                OnCast();
                UpdateStamp();
                UpdateLiner();
            }

            GizmoGate?.Invoke();
            
            if (cloneRaySensor && cloneRaySensor.gameObject) cloneRaySensor.OnGizmos();
        }

        protected static void DrawNormal(RaycastHit hit, bool label = true, bool doubleDisc = false,
            Color color = default)
        {
            if (!hit.transform) return;
            Handles.color = color == default ? HelperColor : color;
            Handles.DrawWireDisc(hit.point, hit.normal, DiscSize);
            if (doubleDisc) Handles.DrawWireDisc(hit.point + hit.normal * DotSize, hit.normal, DiscSize);
            
            Handles.DrawLine(hit.point, hit.point + hit.normal * LineSize);
            if (RCProPanel.ShowLabels && label) Handles.Label(hit.point + hit.normal * DotSize, hit.transform.name, RCProEditor.HeaderStyle);
        }
        
        protected void GeneralField(SerializedObject _so)
        {
            DetectLayerField(_so);
            LinerField(_so);
            StampField(_so);
            PlanarField(_so);
            BaseField(_so);
        }
        
        protected void InformationField()
        {
            if (!hit.transform) return;
            InformationField(() =>
            {
                var ID = hit.transform.gameObject.GetInstanceID();
                GUILayout.Label($"Hit: {hit.transform.name}".ToContent(
                    $"Instance ID: {ID}, Located at: {hit.transform.position}, Offset from transform: {hit.transform.position - Hit.point}"));
                GUILayout.Label($"Continues Distance: {ContinuesDistance:F}".ToContent("Continues Distance"));
                if (this is PathRay pathRay)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Detect Index: ");
                    GUILayout.Label(pathRay.DetectIndex.ToString());
                    GUILayout.EndHorizontal();
                }
            });
        }
#endif
    }
}