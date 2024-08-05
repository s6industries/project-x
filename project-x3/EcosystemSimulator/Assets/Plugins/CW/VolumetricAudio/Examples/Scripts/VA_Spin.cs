using UnityEngine;
using CW.Common;

namespace VolumetricAudio.Examples
{
	/// <summary>This component spins the current GameObject.</summary>
	[HelpURL(VA_Common.HelpUrlPrefix + "VA_Spin")]
	[AddComponentMenu(VA_Common.ComponentMenuPrefix + "Spin")]
	public class VA_Spin : MonoBehaviour
	{
		/// <summary>The amount of degrees this GameObject is rotated by each second in world space.</summary>
		public Vector3 DegreesPerSecond;
	
		protected virtual void Update()
		{
			transform.Rotate(DegreesPerSecond * Time.deltaTime);
		}
	}
}

#if UNITY_EDITOR
namespace VolumetricAudio.Examples
{
	using UnityEditor;
	using TARGET = VA_Spin;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class VA_Spin_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.DegreesPerSecond.sqrMagnitude == 0.0f));
				Draw("DegreesPerSecond", "The amount of degrees this GameObject is rotated by each second in world space.");
			EndError();
		}
	}
}
#endif