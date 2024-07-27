using RaycastPro.Planers;
using RaycastPro.RaySensors;
using UnityEngine.SceneManagement;

namespace RaycastPro.Planers2D
{
    using System;
    using RaySensors2D;
    using UnityEngine;
    using UnityEngine.Events;
    using System.Collections.Generic;
#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    [Serializable]
    public class RaySensor2DEvent : UnityEvent<RaySensor2D> { }

    public abstract class Planar2D : BasePlanar
    {
        public RaySensor2D outerRay;
        public override bool Performed { get; protected set; }
        
        public float z => transform.position.z;

        public RaySensor2DEvent onReceiveRay;
        public RaySensor2DEvent onCloneRay;
        public RaySensor2DEvent onBeginReceiveRay;
        public RaySensor2DEvent onEndReceiveRay;
        
        protected void ApplyLengthControl(RaySensor2D raySensor)
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
        
        protected Vector3 GetForward(RaySensor2D innerRay, Vector2 _default)
        {
            switch (baseDirection)
            {
                case DirectionOutput.NegativeHitNormal: return -innerRay.hit.normal;
                case DirectionOutput.HitDirection: return innerRay.HitDirection.normalized;
                case DirectionOutput.SensorLocal: return innerRay.LocalDirection.normalized;
                case DirectionOutput.PlanarForward: return _default;
                default: return _default;
            }
        }

#if UNITY_EDITOR

        internal override void OnGizmos()
        {
            // Planar Gizmos in Here
        }

        protected void GeneralField(SerializedObject _so, bool lengthControlField = true, bool outerField = true)
        {
            DetectLayerField(_so);
            BaseDirectionField(_so);
            PropertyMaxField(_so.FindProperty(nameof(offset)),  COffset.ToContent(COffset), 0.02f);
            if (lengthControlField) LengthControlField(_so);
            if (outerField) OuterField(_so.FindProperty(nameof(outerType)), _so.FindProperty(nameof(outerRay)));
            EditorGUILayout.PropertyField(_so.FindProperty(nameof(poolManager)), CPoolManager.ToContent(TPoolManager));
            BaseField(_so, hasInteraction: false, hasUpdateMode: false);
        }

        private readonly string[] CEventNames = {"onReceiveRay", "onCloneRay", "onBeginReceiveRay", "onEndReceiveRay"};
        protected void EventField(SerializedObject _so)
        {
            EventFoldout =
                EditorGUILayout.BeginFoldoutHeaderGroup(EventFoldout, CEvents, RCProEditor.HeaderFoldout);

            if (EventFoldout) RCProEditor.EventField(_so, CEventNames);
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
#endif
        public void CloneInstantiate(RaySensor2D raySensor)
        {
            if (!raySensor) return;
            if (!InLayer(raySensor.gameObject) || raySensor.hit.transform != transform) return;
            switch (outerType)
            {
                case OuterType.Auto:
                    
                    if (raySensor is PathRay2D)
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
        
        private void InstantiateReference(RaySensor2D raySensor)
        {
            // Double supported Clone Sensors
            raySensor.cloneRaySensor = Instantiate(outerRay ? outerRay : raySensor);
            
#if UNITY_EDITOR
           RenameClone(raySensor.cloneRaySensor);
#endif
            
            raySensor.cloneRaySensor.baseRaySensor = raySensor;
        }

        private GameObject _tObj;
        private CloneRay2D cloneRay;
        private void InstantiateClone(RaySensor2D raySensor)
        {
            _tObj = new GameObject();
            //Scene Debugging
            SceneManager.MoveGameObjectToScene(_tObj, raySensor.gameObject.scene);
            cloneRay = _tObj.AddComponent<CloneRay2D>();
            cloneRay.transform.parent = poolManager;
            
#if UNITY_EDITOR
            RenameClone(cloneRay);
#endif

            if (this is PortalPlanar2D planar)
            {
                cloneRay.CopyFrom(raySensor, planar, transform);
            }
            else
            {
                cloneRay.CopyFrom(raySensor, this, transform);
            }

            // Double supported Clone Sensors
            cloneRay.baseRaySensor = raySensor;
            raySensor.cloneRaySensor = cloneRay;
        }
        public abstract void OnReceiveRay(RaySensor2D sensor);
        public virtual void OnBeginReceiveRay(RaySensor2D sensor)
        {
            if (this is BlockPlanar2D) return;
            
            CloneInstantiate(sensor);
            sensor.cloneRaySensor?.transform.RemoveChildren();

            OnCloneRay(sensor.cloneRaySensor);
        }
        public void OnCloneRay(RaySensor2D sensor) => onCloneRay?.Invoke(sensor);
        public virtual bool OnEndReceiveRay(RaySensor2D sensor)
        {
            if (sensor.cloneRaySensor)
            {
                Destroy(sensor.cloneRaySensor.gameObject);
            }

                
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