namespace RaycastPro.RaySensors2D
{
    using Planers2D;
    using UnityEngine;
    using System.Collections.Generic;
#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    public abstract class RaySensor2D : BaseRaySensor<RaycastHit2D, RaycastEvent2D, Planar2D>
    {
        public float minDepth = .5f;
        public float maxDepth = -.5f;
        public float MinDepth => transform.position.z + minDepth;
        public float MaxDepth => transform.position.z + maxDepth;

        protected bool isDetect;
        public override bool Performed
        {
            get => isDetect;
            protected set { }
        }
        public Vector3 HitPointZ => hit.point.ToDepth(z);

        public bool normalFilter;

        public float minAngle;
        public float maxAngle = 90;
        
        internal RaySensor2D baseRaySensor;
        internal RaySensor2D cloneRaySensor;

        [SerializeField] protected bool hitBackside;

        #region Lambdas

        public override Vector3 TipTarget => hit ? hit.point.ToDepth(z) : Tip.ToDepth(z);
        
        public override Vector3 TargetDirection => hit ? -hit.normal.ToDepth(z) : HitDirection.ToDepth();
        
        public RaycastHit2D CloneHit => cloneRaySensor ? cloneRaySensor.CloneHit : hit;

        #endregion
        public Vector2 Position2D => transform.position;

        public Vector2 direction = new Vector2(1, 0);
        public virtual Vector2 HitDirection => hit ? hit.point.ToDepth(z) - Base : LocalDirection3D;
        public float Length => direction.magnitude;
        public Vector2 Direction => local ? LocalDirection : direction;
        public Vector2 ScaledDirection => Vector2.Scale(Direction, transform.lossyScale);
        public Vector2 FullDirection => scalable ? ScaledDirection : Direction;
        
        /// <summary>
        /// Direction on Depth (Z)
        /// </summary>
        public Vector3 Direction3D => Direction.ToDepth(z);
        public Vector2 LocalDirection => transform.TransformDirection(direction);
        public Vector3 LocalDirection3D => transform.TransformDirection(direction);
        public float z => transform.position.z;

        public float scaleY => transform.lossyScale.y;

        public float scaleX => transform.lossyScale.x;
        public virtual float ContinuesDistance => Length - HitDistance;
        public float HitDistance => hit ? (hit.point.ToDepth(z) - Base).magnitude : Length;
        
        /// <summary>
        /// The length traveled from Ray to reach the target point
        /// </summary>
        public virtual float HitLength => HitDistance;

        public Vector3 up2D => Vector3.ProjectOnPlane(transform.up, Vector3.forward).normalized;
        public Vector3 right2D => Vector3.ProjectOnPlane(transform.right, Vector3.forward).normalized;

        private static Vector3 Up2D(Transform t) => Vector3.ProjectOnPlane(t.up, Vector3.forward).normalized;
        private static Vector3 Right2D(Transform t) => Vector3.ProjectOnPlane(t.right, Vector3.forward).normalized;

        public override bool ClonePerformed => CloneHit;
        public RaySensor2D LastClone
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
        
        public virtual void GetPath2D(ref List<Vector2> path)
        {
            path = new List<Vector2>() {Base, Tip};
        }
        
        public virtual void GetPath(ref List<Vector3> path)
        {
            path = new List<Vector3>() {Base, Tip};
        }
        
        #region Public Methods

        public void SetDirection(Vector2 newDirection) => direction = newDirection;

        public void AddDirection(Vector2 vector) => direction += vector;

        public void SetHitActive(bool toggle)
        {
            if (hit.transform) hit.transform.gameObject.SetActive(toggle);
        }

        public void DestroyHit(float delay)
        {
            if (hit.transform) Destroy(hit.transform.gameObject, delay);
        }

        public void SetHitPosition(Vector3 newPosition)
        {
            if (hit.transform) hit.transform.position = newPosition;
        }

        public void TranslateHitPosition(Vector3 vector)
        {
            if (hit.transform) hit.transform.Translate(vector);
        }

        public void InstantiateHitObject(Vector3 location)
        {
            if (hit.transform) Instantiate(hit.transform, location, Quaternion.LookRotation(TipDirection));
        }

        public void AddForceAlongNormal(float force)
        {
            if (hit.transform.TryGetComponent(out Rigidbody2D body))
            {
                body.AddForce(hit.normal * force);
            }
        }

        public void AddForceAlongHitDirection(float force)
        {
            if (hit.transform.TryGetComponent(out Rigidbody2D body))
            {
                body.AddForce(HitDirection.normalized * force);
            }
        }

        public void AddForceAlongTipDirection(float force)
        {
            if (hit.transform.TryGetComponent(out Rigidbody2D body))
            {
                body.AddForce(TipDirection.normalized * force);
            }
        }

        public void AddDynamicForceAlongTipDirection(float force)
        {
            if (hit.transform.TryGetComponent(out Rigidbody2D body))
            {
                body.AddForce(TipDirection.normalized * ContinuesDistance * force);
            }
        }

        public void AddDynamicForceAlongNormal(float force)
        {
            if (hit.transform.TryGetComponent(out Rigidbody2D body))
            {
                body.AddForce(hit.normal * ContinuesDistance * force);
            }
        }

        public void AddDynamicForceHitDirection(float force)
        {
            if (hit.transform.TryGetComponent(out Rigidbody2D body))
            {
                body.AddForce(HitDirection.normalized * ContinuesDistance * force);
            }
        }

        #endregion

        public override void UpdateLiner()
        {
            if (!liner) return;
            
            if (linerClamped)
            {
                liner.positionCount = 2;
                if (linerCutOnHit)
                {
                    _base = Base;
                    _tip = Tip;
                    var hitPoint = linerFixCut ? GetPointOnLine(_base, _tip, HitPointZ) : HitPointZ;
                    var _pos = hit.transform ? (hitPoint - _base).magnitude / RayLength : 1f;

                    if (_pos >= linerBasePosition)
                    {
                        liner.SetPosition(0, Vector3.Lerp(_base, _tip, linerBasePosition));
                        liner.SetPosition(1, _pos < linerEndPosition ? hitPoint : Vector3.Lerp(_base, _tip, linerEndPosition));
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
        protected bool FilterCheck(RaycastHit2D _hit, Vector2 dir)
        {
            if (!_hit) return false;

            if (!normalFilter) return _hit;

            var Angle = Vector2.Angle(-_hit.normal, dir);

            if (Angle >= minAngle && Angle <= maxAngle) return _hit;

            return false;
        }
        protected bool FilterCheck(RaycastHit2D _hit)
        {
            if (!_hit) return false;
            if (!normalFilter) return _hit;
            var Angle = Vector2.Angle(-_hit.normal, HitDirection);
            if (Angle >= minAngle && Angle <= maxAngle) return _hit;
            return false;
        }
        private bool tQ, tHB;
        
        private void SolvedQueriesCast()
        {
            tHB = Physics2D.queriesStartInColliders;
            tQ = Physics2D.queriesHitTriggers;
            Physics2D.queriesStartInColliders = hitBackside;
            if (triggerInteraction != QueryTriggerInteraction.UseGlobal) Physics2D.queriesHitTriggers = triggerInteraction == QueryTriggerInteraction.Collide;
            OnCast();
            if (triggerInteraction != QueryTriggerInteraction.UseGlobal) Physics2D.queriesHitTriggers = tQ;
            Physics2D.queriesStartInColliders = Physics2D.queriesStartInColliders;
        }

        public override void UpdateStamp()
        {
            if (!stamp) return;
            // Stamp Planar Activate Fix
            if (cloneRaySensor && cloneRaySensor.enabled) return;
            var _st = stamp.transform;
            if (stampOnHit && hit.transform)
            {
                _st.position = TipTarget + hit.normal.ToDepth() * stampOffset;
                if (!syncStamp.syncAxis) return;
                switch (syncStamp.axis)
                {
                    case Axis.X: _st.right = hit.normal * (syncStamp.flipAxis ? -1 : 1); break;
                    case Axis.Z: _st.forward = hit.normal * (syncStamp.flipAxis ? -1 : 1); break;
                    case Axis.Y: _st.up = hit.normal * (syncStamp.flipAxis ? -1 : 1); break;
                }
            }
            else
            {
                _st.position = Tip + (HitDirection * stampOffset).ToDepth();
                if (!syncStamp.syncAxis) return;
                switch (syncStamp.axis)
                {
                    case Axis.X: _st.right = HitDirection * (syncStamp.flipAxis ? 1 : -1); break;
                    case Axis.Z: _st.forward = HitDirection * (syncStamp.flipAxis ? 1 : -1); break;
                    case Axis.Y: _st.up = HitDirection * (syncStamp.flipAxis ? 1 : -1); break;
                }
            }
        }

        internal override void RuntimeUpdate()
        {
            SolvedQueriesCast();
            onCast?.Invoke();
            UpdateLiner();
            UpdateStamp();
            if (hit) OnDetect();
            if (PreviousHit != hit)
            {
                onChange?.Invoke(hit);
                // end Event most be top of begin
                if (PreviousHit) OnEndDetect();
                if (hit) OnBeginDetect();
            }
            PreviousHit = hit;
        }

        internal override void OnBeginDetect()
        {
            if (stampAutoHide) stamp?.gameObject.SetActive(true);
            if (planarSensitive)
            {
                if (anyPlanar)
                {
                    _planar = hit.transform.GetComponent<Planar2D>();
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
        
        internal override void OnDetect()
        {
            onDetect?.Invoke(hit);
            // this part must translate to planers code =>
            if (!planarSensitive) return;
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
        internal override void SafeRemove()
        {
            if (!gameObject) return;

            if (cloneRaySensor && cloneRaySensor.gameObject)
            {
                cloneRaySensor.SafeRemove();
            }

            if (this) Destroy(gameObject);
        }
        public static void CloneDestroy(RaySensor2D sensor)
        {
            if (!(sensor && sensor.gameObject)) return;
            if (sensor.cloneRaySensor) CloneDestroy(sensor.cloneRaySensor);
            DestroyImmediate(sensor.gameObject);
        }
#if UNITY_EDITOR
        protected void DrawDepthLine(Vector3 p1, Vector3 p2, Color color = default)
        {
            GizmoColor = (MaxDepth > MinDepth ? HelperColor : color == default ? DefaultColor : color).Alpha(alphaCharge);
            
            Gizmos.DrawLine(new Vector3(p1.x, p1.y, MaxDepth), new Vector3(p2.x, p2.y, MaxDepth));
            Handles.DrawDottedLine(new Vector3(p1.x, p1.y, MaxDepth), new Vector3(p1.x, p1.y, MinDepth), StepSizeLine);
            Gizmos.DrawLine(new Vector3(p1.x, p1.y, MinDepth), new Vector3(p2.x, p2.y, MinDepth));
            Handles.DrawDottedLine(new Vector3(p2.x, p2.y, MaxDepth), new Vector3(p2.x, p2.y, MinDepth), StepSizeLine);
        }
        internal static void DrawDepthLine(Vector3 p1, Vector3 p2, float MinDepth, float MaxDepth,
            Color color = default)
        {
            Gizmos.color = Handles.color = MaxDepth > MinDepth ? HelperColor : color == default ? DefaultColor : color;
            Gizmos.DrawLine(new Vector3(p1.x, p1.y, MaxDepth), new Vector3(p2.x, p2.y, MaxDepth));
            Handles.DrawDottedLine(new Vector3(p1.x, p1.y, MaxDepth), new Vector3(p1.x, p1.y, MinDepth), StepSizeLine);
            Gizmos.DrawLine(new Vector3(p1.x, p1.y, MinDepth), new Vector3(p2.x, p2.y, MinDepth));
            Handles.DrawDottedLine(new Vector3(p2.x, p2.y, MaxDepth), new Vector3(p2.x, p2.y, MinDepth), StepSizeLine);
        }
        
        /// <summary>
        /// Full Fixed Box Ray
        /// </summary>
        internal static void DrawBoxRay(Transform transform, Vector3 pos, Vector3 _dir, Vector2 _size, float z, bool local)
        {
            var localDirection = (transform.rotation * _dir).ToDepth();

            Vector3 up;
            Vector3 right;

            if (local)
            {
                up = Up2D(transform) * _size.y / 2;
                right = Right2D(transform) * _size.x / 2;
            }
            else
            {
                up = Vector3.up * _size.y / 2;
                right = Vector3.right * _size.x / 2;
            }

            var p1 = (pos + up + right).ToDepth(z);
            var p2 = (pos - up + right).ToDepth(z);
            var p3 = (pos + up - right).ToDepth(z);
            var p4 = (pos - up - right).ToDepth(z);
            
            // Handles.Label(p1, "p1");
            // Handles.Label(p2, "p2");
            // Handles.Label(p3, "p3");
            // Handles.Label(p4, "p4");

            if (_dir.y < 0)
            {
                Gizmos.DrawLine(p1, p3);
                Gizmos.DrawLine(p2+localDirection, p4+localDirection);
                
                if (_dir.x < 0)
                {
                    Gizmos.DrawRay(p2, localDirection);
                    Gizmos.DrawRay(p3, localDirection);
                }
                else
                {
                    Gizmos.DrawRay(p1, localDirection);
                    Gizmos.DrawRay(p4, localDirection);
                }
            }
            else
            {
                Gizmos.DrawLine(p2, p4);
                Gizmos.DrawLine(p1+localDirection, p3+localDirection);
                
                if (_dir.x > 0)
                {
                    Gizmos.DrawRay(p2, localDirection);
                    Gizmos.DrawRay(p3, localDirection);
                }
                else
                {
                    Gizmos.DrawRay(p1, localDirection);
                    Gizmos.DrawRay(p4, localDirection);
                }
            }

            if (_dir.x > 0)
            {
                Gizmos.DrawLine(p3, p4);
                Gizmos.DrawLine(p1+localDirection, p2+localDirection);
            }
            else
            {
                Gizmos.DrawLine(p1, p2);
                Gizmos.DrawLine(p3+localDirection, p4+localDirection);
            }
        }

        protected override void EditorUpdate()
        {
            if (!RCProPanel.realtimeEditor) return;
            if (InEditMode)
            {
                GizmoGate = null;
                if (IsSceneView)
                {
                    SolvedQueriesCast();
                }
                UpdateStamp();
                UpdateLiner();
            }
            
            GizmoGate?.Invoke();

            if (cloneRaySensor && cloneRaySensor.gameObject) cloneRaySensor.OnGizmos();
        }

        protected void DrawNormalFilter()
        {
            if (!normalFilter) return;
            var hitPoint = hit.point.ToDepth(z);
            Handles.color = hit ? DetectColor : HelperColor;
            if (hit) DrawLineZTest(transform.position, hitPoint, true);
            Handles.color = Handles.color.Alpha(RCProPanel.alphaAmount);
            var radius = RCProPanel.normalFilterRadius;
            Handles.DrawSolidArc(hitPoint, Vector3.forward, hit.normal, maxAngle, radius);
            Handles.DrawSolidArc(hitPoint, Vector3.forward, hit.normal, -maxAngle, radius);
            Handles.color = BlockColor.Alpha(RCProPanel.alphaAmount);
            Handles.DrawSolidArc(hitPoint, Vector3.forward, hit.normal, minAngle, radius);
            Handles.DrawSolidArc(hitPoint, Vector3.forward, hit.normal, -minAngle, radius);
        }

        private const string CNormalFilter = "Normal Filter";
        private const string TNormalFilter = "Determination difference of normal hit angle and HitDirection. This option does not stop receiving hit and will be activate IsDetect property";
        protected void NormalFilterField(SerializedObject _so)
        {
            BeginVerticalBox();

            EditorGUILayout.PropertyField(_so.FindProperty(nameof(normalFilter)), CNormalFilter.ToContent(TNormalFilter));

            GUI.enabled = normalFilter;
            PropertyMinMaxField(_so.FindProperty(nameof(minAngle)), _so.FindProperty(nameof(maxAngle)), ref minAngle, ref maxAngle, 0, 90);
            GUI.enabled = true;

            EndVertical();
        }

        protected void DepthField(SerializedObject _so)
        {
            EditorGUILayout.PropertyField(_so.FindProperty(nameof(minDepth)), "Min Depth".ToContent());
            EditorGUILayout.PropertyField(_so.FindProperty(nameof(maxDepth)), "Max Depth".ToContent());
        }
        
        protected void GeneralField(SerializedObject _so)
        {
            DetectLayerField(_so);
            EditorGUILayout.PropertyField(_so.FindProperty(nameof(hitBackside)));
            DepthField(_so);
            LinerField(_so);
            StampField(_so);
            NormalFilterField(_so);
            PlanarField(_so);
            BaseField(_so);
        }

        protected void HitInformationField()
        {
            InformationField(() =>
            {
                if (!hit) return;
                BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.Label(hit.transform.name);
                GUILayout.Label(hit.distance.ToString());
                GUILayout.EndHorizontal();
                if (this is PathRay2D pathRay2D)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Detect Index");
                    GUILayout.Label(pathRay2D.DetectIndex.ToString());
                    GUILayout.EndHorizontal();
                }
                EndVertical();
            });
        }
#endif
    }
}