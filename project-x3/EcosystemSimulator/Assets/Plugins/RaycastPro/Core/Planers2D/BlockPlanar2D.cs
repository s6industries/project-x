namespace RaycastPro.Planers2D
{
    using RaySensors2D;
    using UnityEngine;

#if UNITY_EDITOR
    using Editor;
    using UnityEditor;
#endif

    [AddComponentMenu("RaycastPro/Planers/" + nameof(BlockPlanar2D))]
    public sealed class BlockPlanar2D : Planar2D
    {
        public override void OnReceiveRay(RaySensor2D sensor) { }
#if UNITY_EDITOR
#pragma warning disable CS0414
        private static string Info = "Receiving and blocking impact from any Planar Sensitive 2D Ray";
#pragma warning restore CS0414
        internal override void EditorPanel(SerializedObject _so, bool hasMain = true, bool hasGeneral = true,
            bool hasEvents = true,
            bool hasInfo = true)
        {
            if (hasGeneral) GeneralField(_so);

            if (hasEvents)
            {
                EventFoldout =
                    EditorGUILayout.BeginFoldoutHeaderGroup(EventFoldout, CEvents, RCProEditor.HeaderFoldout);

                if (EventFoldout)
                {
                    RCProEditor.EventField(_so, new[]
                    {
                        nameof(onReceiveRay), nameof(onBeginReceiveRay), nameof(onEndReceiveRay)
                    });
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            if (hasInfo) InformationField();
        }
#endif
    }
}