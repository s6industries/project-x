namespace RaycastPro.RaySensors
{
    using UnityEngine;

#if UNITY_EDITOR
    using System.Threading.Tasks;
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Rey Sensors/" + nameof(BoxRay))]
    public sealed class BoxRay : RaySensor
    {
#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info =
            "Emit a box ray in the specified direction with defined extents and return the Hit information." +
            HAccurate + HDirectional + HScalable;
#pragma warning restore CS0414
#endif
        
        public Vector3 extents = new Vector3(.4f, .4f, .1f);

        private Vector3 _dir, sExtents;
        protected override void OnCast()
        {
            if (scalable)
            {
                _dir = ScaledDirection;
                sExtents = Vector3.Scale(transform.lossyScale, extents);
            }
            else
            {
                _dir = Direction;
                sExtents = extents; 
            }
            Physics.BoxCast(transform.position, sExtents / 2, _dir, out hit, transform.rotation,
                _dir.magnitude, detectLayer.value, triggerInteraction);
        }

#if UNITY_EDITOR
        
        /// <summary>
        /// Hint: This command will make your references missing.
        /// </summary>
        [ContextMenu("Convert To PipeRay")]
        private async void ConvertToPipeRay()
        {
            var _ray = Undo.AddComponent<PipeRay>(gameObject);
            
            _ray.direction = direction;
            _ray.Radius = extents.x;
            _ray.Height = extents.y;

            await Task.Delay(1);
            Undo.DestroyObjectImmediate (this);
        }
        /// <summary>
        /// Hint: This command will make your references missing.
        /// </summary>
        [ContextMenu("Convert To BaseRay")]
        private async void ConvertToBasicRay()
        {
            var _ray = Undo.AddComponent<BasicRay>(gameObject);
            
            _ray.direction = direction;

            await Task.Delay(1);
            
            Undo.DestroyObjectImmediate (this);
        }

        internal override void OnGizmos()
        {
            EditorUpdate();
            GizmoColor = Performed ? DetectColor : DefaultColor;
            sExtents = scalable ? Vector3.Scale(transform.lossyScale, extents) : extents;
            DrawBoxLine(transform.position, transform.position + (scalable ? ScaledDirection : Direction), sExtents, true);
            DrawNormal(hit);
        }

        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasMain)
            {
                DirectionField(_so);
                BeginHorizontal();
                EditorGUILayout.PropertyField(_so.FindProperty(nameof(extents)));
                ScaleField(_so.FindProperty(nameof(scalable)));
                EndHorizontal();
            }
            if (hasGeneral) GeneralField(_so);
            if (hasEvents) EventField(_so);
            if (hasInfo) InformationField();
        }
#endif

        private Vector3 ExtentVector => LocalDirection.normalized * extents.z / 2;
        public override Vector3 Tip => RawTip + ExtentVector;
        
        public override Vector3 RawTip => transform.position + Direction;
        public override float RayLength => direction.magnitude + extents.z;
        public override Vector3 Base => transform.position;
        public Vector3 ExtentBase => Base - ExtentVector;
        public Vector3 ExtentTip => Tip + ExtentVector;
    }
}