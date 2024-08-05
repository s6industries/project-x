using UnityEngine;
using CW.Common;

namespace VolumetricAudio
{
	/// <summary>This component allows you to define a specific volume override for the current collider.</summary>
	[HelpURL(VA_Common.HelpUrlPrefix + "VA_Material")]
	[AddComponentMenu(VA_Common.ComponentMenuPrefix + "Material")]
	public class VA_Material : MonoBehaviour
	{
		/// <summary>The volume multiplier when this material is blocking the <b>VA_AudioSource</b>.</summary>
		public float OcclusionVolume { set {occlusionVolume = value;} get { return occlusionVolume; } } [SerializeField] [Range(0.0f, 1.0f)] private float occlusionVolume = 0.1f;
	}
}

#if UNITY_EDITOR
namespace VolumetricAudio
{
	using UnityEditor;
	using TARGET = VA_Material;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class VA_Material_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("occlusionVolume", "The volume multiplier when this material is blocking the VA_AudioSource.");
		}
	}
}
#endif