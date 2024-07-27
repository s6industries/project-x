namespace RaycastPro.Planers
{
    using System;
    using RaySensors;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.SceneManagement;
#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    [Serializable]
    public class RaySensorEvent : UnityEvent<RaySensor> { }
    public abstract class Planar : BasePlanar
    {
        public RaySensor outerRay;
        public override bool Performed { get; protected set; }
        
        /// <summary>
        /// 
        /// </summary>
        public RaySensorEvent onReceiveRay;

        /// <summary>
        /// 
        /// </summary>
        public RaySensorEvent onCloneRay;

        /// <summary>
        /// 
        /// </summary>
        public RaySensorEvent onBeginReceiveRay;

        /// <summary>
        /// 
        /// </summary>
        public RaySensorEvent onEndReceiveRay;

        public abstract void GetForward(RaySensor raySensor, out Vector3 forward);
        protected void ApplyLengthControl(RaySensor raySensor)
        {
            switch (lengthControl)
            {
                case LengthControl.Constant:
                    raySensor.cloneRaySensor.direction = raySensor.direction.normalized * length;
                    break;
                case LengthControl.Sync:
                    raySensor.cloneRaySensor.direction = raySensor.direction * length;
                    break;
                case LengthControl.Continues:
                    raySensor.cloneRaySensor.direction =
                        raySensor.direction.normalized * (raySensor.ContinuesDistance * length);
                    break;
            }
        }
        /// <summary>
        /// It will instantiate a ray Clone
        /// </summary>
        /// <param name="raySensor"></param>
        private void CloneInstantiate(RaySensor raySensor)
        {
            if (!raySensor) return;
            if (!InLayer(raySensor.gameObject) || raySensor.hit.transform != transform) return;
            switch (outerType)
            {
                case OuterType.Auto:
                    if (raySensor is PathRay)
                    {
                        InstantiateClone(raySensor);
                    }
                    else
                    {
                        InstantiateReference(raySensor);
                    }

                    break;
                case OuterType.Reference:
                    InstantiateReference(raySensor);
                    break;

                case OuterType.Clone:
                    InstantiateClone(raySensor);
                    break;
            }
            OnReceiveRay(raySensor);
        }
        private void InstantiateReference(RaySensor raySensor)
        {
            // Double supported Clone Sensors
            raySensor.cloneRaySensor = Instantiate(outerRay ? outerRay : raySensor, poolManager);
#if UNITY_EDITOR
            RenameClone(raySensor.cloneRaySensor);
#endif
            raySensor.cloneRaySensor.baseRaySensor = raySensor;
        } 
        private GameObject _tObj;
        protected CloneRay cloneRay;
        private void InstantiateClone(RaySensor raySensor)
        {
            _tObj = new GameObject();
            //Scene Debugging
            SceneManager.MoveGameObjectToScene(_tObj, raySensor.gameObject.scene);
            cloneRay = _tObj.AddComponent<CloneRay>();
            cloneRay.transform.parent = poolManager;
            
#if UNITY_EDITOR
            RenameClone(cloneRay);
#endif
            cloneRay.CopyFrom(raySensor, this, (this is PortalPlanar planar) ? planar.outer : transform);
            // Double supported Clone Sensors
            
            cloneRay.baseRaySensor = raySensor;
            raySensor.cloneRaySensor = cloneRay;
        }
#if UNITY_EDITOR
        
        private Collider validateCollideGizmo;
        private void OnValidate()
        {
            if (transform.TryGetComponent(out Collider collider))
            {
                validateCollideGizmo = collider;
            }
        }
        protected void DrawPlanar()
        {
            Gizmos.color = DefaultColor.Alpha(.5f);
            
            if (validateCollideGizmo is MeshCollider _mc)
            {
                Gizmos.DrawWireMesh(_mc.sharedMesh, transform.position, transform.rotation, transform.lossyScale);
            }
            else if (validateCollideGizmo is SphereCollider _sc)
            {
                Gizmos.DrawSphere(_sc.center, _sc.radius);
            }
            else
            {
                Gizmos.matrix = transform.localToWorldMatrix;

                Gizmos.DrawWireCube(Vector3.zero, Vector2.one);
            }
        }
        internal override void OnGizmos()
        {
            var matrix = Gizmos.matrix;
            Gizmos.DrawRay(transform.position, transform.forward * offset);

            Gizmos.color = HelperColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector2.one);



            Gizmos.matrix = matrix;
        }
        protected void GeneralField(SerializedObject _so, bool lengthControlField = true, bool outerField = true)
        {
            EditorGUILayout.PropertyField(_so.FindProperty(nameof(poolManager)), CPoolManager.ToContent(TPoolManager));
            PlanarBaseField(_so);
            DetectLayerField(_so);
            BaseDirectionField(_so);
            PropertyMaxField(_so.FindProperty(nameof(offset)),  COffset.ToContent());
            if (lengthControlField) LengthControlField(_so);
            if (outerField) OuterField(_so.FindProperty(nameof(outerType)), _so.FindProperty(nameof(outerRay)));
            BaseField(_so, hasInfluence: true, hasInteraction: false, hasUpdateMode: false);
        }

        protected void EventField(SerializedObject _so)
        {
            EventFoldout =
                EditorGUILayout.BeginFoldoutHeaderGroup(EventFoldout, CEvents, RCProEditor.HeaderFoldout);

            if (EventFoldout)  RCProEditor.EventField(_so, new[] {nameof(onReceiveRay), nameof(onCloneRay), nameof(onBeginReceiveRay), nameof(onEndReceiveRay)});
        }
#endif
        internal abstract void OnReceiveRay(RaySensor sensor);

        internal virtual void OnBeginReceiveRay(RaySensor sensor)
        {
            if (this is BlockPlanar) return;
            
            CloneInstantiate(sensor);
            sensor.cloneRaySensor?.transform.RemoveChildren();
            OnCloneRay(sensor.cloneRaySensor);
        }

        /// <summary>
        /// Call OnCloneRay Event
        /// </summary>
        /// <param name="sensor">Need Clone Ray Sensor</param>
        internal void OnCloneRay(RaySensor sensor) => onCloneRay?.Invoke(sensor);

        internal virtual bool OnEndReceiveRay(RaySensor sensor)
        {
            Destroy(sensor.cloneRaySensor.gameObject);
            
            return true;
        }

        void CloneDestroy(RaySensor sensor)
        {
            if (sensor.cloneRaySensor)
            {
                CloneDestroy(sensor.cloneRaySensor);
            }
            Destroy(sensor.gameObject);
        }
    }
}