using UnityEngine;
using CW.Common;

namespace VolumetricAudio.Examples
{
	/// <summary>This component animates the <b>VA_Spiral</b> component's <b>AngleStep</b> setting.</summary>
	[RequireComponent(typeof(VA_Spiral))]
	[HelpURL(VA_Common.HelpUrlPrefix + "VA_ChangeSpiral")]
	[AddComponentMenu(VA_Common.ComponentMenuPrefix + "Change Spiral")]
	public class VA_ChangeSpiral : MonoBehaviour
	{
		/// <summary>The minimum AngleStep value.</summary>
		public float AngleStepA = 10.0f;

		/// <summary>The maximum AngleStep value.</summary>
		public float AngleStepB = -10.0f;

		/// <summary>The amount of seconds it takes to go from AngleStep A and B.</summary>
		public float Interval = 5.0f;

		// Current interpolation position
		[SerializeField]
		private float position;

		[System.NonSerialized]
		private VA_Spiral spiral;

		protected virtual void Update()
		{
			if (Interval > 0.0f)
			{
				position += Time.deltaTime;

				if (spiral == null) spiral = GetComponent<VA_Spiral>();

				spiral.AngleStep = Mathf.Lerp(AngleStepA, AngleStepB, Mathf.PingPong(position, Interval) / Interval);

				spiral.Regenerate();
			}
		}
	}
}

#if UNITY_EDITOR
namespace VolumetricAudio.Examples
{
	using UnityEditor;
	using TARGET = VA_ChangeSpiral;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class VA_ChangeSpiral_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("AngleStepA", "The minimum AngleStep value.");
			Draw("AngleStepB", "The maximum AngleStep value.");
			BeginError(Any(tgts, t => t.Interval <= 0.0f));
				Draw("Interval", "The amount of seconds it takes to go from AngleStep A and B.");
			EndError();
		}
	}
}
#endif