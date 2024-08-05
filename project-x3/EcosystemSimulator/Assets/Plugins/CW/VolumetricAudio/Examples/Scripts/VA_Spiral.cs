using UnityEngine;
using CW.Common;

namespace VolumetricAudio.Examples
{
	/// <summary>This component procedurally generates a flat spiral mesh.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter))]
	[HelpURL(VA_Common.HelpUrlPrefix + "VA_Spiral")]
	[AddComponentMenu(VA_Common.ComponentMenuPrefix + "Spiral")]
	public class VA_Spiral : MonoBehaviour
	{
		/// <summary>Amount of segments in the spiral.</summary>
		public int SegmentCount = 100;

		/// <summary>Thickness of the spiral in local space.</summary>
		public float SegmentThickness = 1.0f;

		/// <summary>Initial angle of the spiral edge in degrees.</summary>
		public float InitialAngle;

		/// <summary>Initial distance of the spiral inner edge in local space.</summary>
		public float InitialDistance = 1.0f;

		/// <summary>Angle increment of each spiral segment in degrees.</summary>
		public float AngleStep = 10.0f;

		/// <summary>Distance increment of each spiral segment in local space.</summary>
		public float DistanceStep = 0.1f;

		[System.NonSerialized]
		private Mesh generatedMesh;

		[System.NonSerialized]
		private Vector3[] positions;

		[System.NonSerialized]
		private Vector2[] uvs;

		[System.NonSerialized]
		private int[] indices;

		[System.NonSerialized]
		private MeshFilter cachedMeshFilter;

		public void Regenerate()
		{
			// Create or clear mesh
			if (generatedMesh == null)
			{
				generatedMesh = new Mesh();

				generatedMesh.name      = "Spiral";
				generatedMesh.hideFlags = HideFlags.DontSave;
			}
			else
			{
				generatedMesh.Clear();
			}

			// Apply mesh to filter
			if (cachedMeshFilter == null) cachedMeshFilter = GetComponent<MeshFilter>();

			cachedMeshFilter.sharedMesh = generatedMesh;

			// More than one segment?
			if (SegmentCount > 0)
			{
				// Allocate arrays?
				var vertexCount = SegmentCount * 2 + 2;

				if (positions == null || positions.Length != vertexCount)
				{
					positions = new Vector3[vertexCount];
				}

				if (uvs == null || uvs.Length != vertexCount)
				{
					uvs = new Vector2[vertexCount];
				}

				// Generate indices?
				if (indices == null || indices.Length != SegmentCount * 6)
				{
					indices = new int[SegmentCount * 6];

					for (var i = 0; i < SegmentCount; i++)
					{
						indices[i * 6 + 0] = i * 2 + 0;
						indices[i * 6 + 1] = i * 2 + 1;
						indices[i * 6 + 2] = i * 2 + 2;
						indices[i * 6 + 3] = i * 2 + 3;
						indices[i * 6 + 4] = i * 2 + 2;
						indices[i * 6 + 5] = i * 2 + 1;
					}
				}

				var angle    = InitialAngle;
				var distance = InitialDistance;

				for (var i = 0; i <= SegmentCount; i++)
				{
					var vertex = i * 2;

					positions[vertex + 0] = VA_Common.SinCos(angle * Mathf.Deg2Rad) *  distance;
					positions[vertex + 1] = VA_Common.SinCos(angle * Mathf.Deg2Rad) * (distance + SegmentThickness);

					uvs[vertex + 0] = Vector2.zero;
					uvs[vertex + 1] = Vector2.one;

					angle    += AngleStep;
					distance += DistanceStep;
				}

				// Update mesh
				generatedMesh.indexFormat = positions.Length >= 65535 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
				generatedMesh.vertices    = positions;
				generatedMesh.triangles   = indices;
				generatedMesh.uv          = uvs;

				generatedMesh.RecalculateBounds();
				generatedMesh.RecalculateNormals();
			}
		}

		protected virtual void Awake()
		{
			Regenerate();
		}

		protected virtual void OnValidate()
		{
			Regenerate();
		}

		protected virtual void Update()
		{
			Regenerate();
		}

		protected virtual void OnDestroy()
		{
			CwHelper.Destroy(generatedMesh);
		}
	}
}

#if UNITY_EDITOR
namespace VolumetricAudio.Examples
{
	using UnityEditor;
	using TARGET = VA_Spiral;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class VA_Spiral_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.SegmentCount < 1));
				Draw("SegmentCount", "Amount of segments in the spiral.");
			EndError();
			BeginError(Any(tgts, t => t.SegmentThickness == 0.0f));
				Draw("SegmentThickness", "Thickness of the spiral in local space.");
			EndError();
			Draw("InitialAngle", "Initial angle of the spiral edge in degrees.");
			Draw("InitialDistance", "Initial distance of the spiral inner edge in local space.");
			BeginError(Any(tgts, t => t.AngleStep == 0.0f));
				Draw("AngleStep", "Angle increment of each spiral segment in degrees.");
			EndError();
			Draw("DistanceStep", "Distance increment of each spiral segment in local space.");
		}
	}
}
#endif