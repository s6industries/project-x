using System.Collections.Generic;
using UnityEngine;
using CW.Common;

namespace VolumetricAudio.Examples
{
	/// <summary>This component allows you to draw a wireframe between specified pairs of points. The points can be generated from a <b>MeshFilter</b> too.</summary>
	[ExecuteInEditMode]
	[HelpURL(VA_Common.HelpUrlPrefix + "VA_Wireframe")]
	[AddComponentMenu(VA_Common.ComponentMenuPrefix + "Wireframe")]
	public class VA_Wireframe : MonoBehaviour
	{
		[System.Serializable]
		public struct Pair
		{
			public Vector3 A;
			public Vector3 B;
		}

		/// <summary>The lines making up the wireframe.</summary>
		public List<Pair> Pairs { get { if (pairs == null) pairs = new List<Pair>(); return pairs; } } [SerializeField] private List<Pair> pairs;

		/// <summary>The material used to render the wireframe mesh.</summary>
		public Material Material { set { material = value; } get { return material; } } [SerializeField] private Material material;

		[System.NonSerialized]
		private Mesh generatedMesh;

		private static List<Vector3> positions = new List<Vector3>();

		private static List<Vector4> coords0 = new List<Vector4>();

		private static List<Vector4> coords1 = new List<Vector4>();

		private static List<int> indices = new List<int>();

		[ContextMenu("Update Mesh")]
		public void UpdateMesh()
		{
			if (generatedMesh == null)
			{
				generatedMesh = new Mesh();
				generatedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			}
			else
			{
				generatedMesh.Clear();
			}

			positions.Clear();
			coords0.Clear();
			coords1.Clear();
			indices.Clear();

			if (pairs != null)
			{
				foreach (var pair in pairs)
				{
					var i = positions.Count;

					positions.Add(pair.A);
					positions.Add(pair.A);
					positions.Add(pair.B);
					positions.Add(pair.B);

					coords0.Add(new Vector4(pair.A.x, pair.A.y, pair.A.z,  1.0f));
					coords0.Add(new Vector4(pair.A.x, pair.A.y, pair.A.z, -1.0f));
					coords0.Add(new Vector4(pair.A.x, pair.A.y, pair.A.z,  1.0f));
					coords0.Add(new Vector4(pair.A.x, pair.A.y, pair.A.z, -1.0f));

					coords1.Add(new Vector4(pair.B.x, pair.B.y, pair.B.z,  0.0f));
					coords1.Add(new Vector4(pair.B.x, pair.B.y, pair.B.z,  0.0f));
					coords1.Add(new Vector4(pair.B.x, pair.B.y, pair.B.z,  1.0f));
					coords1.Add(new Vector4(pair.B.x, pair.B.y, pair.B.z,  1.0f));

					indices.Add(i + 0);
					indices.Add(i + 1);
					indices.Add(i + 2);

					indices.Add(i + 3);
					indices.Add(i + 2);
					indices.Add(i + 1);
				}
			}

			generatedMesh.SetVertices(positions);
			generatedMesh.SetUVs(0, coords0);
			generatedMesh.SetUVs(1, coords1);
			generatedMesh.SetTriangles(indices, 0, true);
		}

		public void RemoveOne(int index)
		{
			pairs.RemoveAt(index);
		}

		public void RemoveLoop(int index, float threshold)
		{
			var pair = pairs[index];

			pairs.RemoveAt(index);

			while (true)
			{
				index = GetPairIndex(pair.B, pair.B - pair.A, threshold);

				if (index < 0)
				{
					index = GetPairIndex(pair.A, pair.A - pair.B, threshold);

					if (index < 0)
					{
						return;
					}
				}

				pair = pairs[index];

				pairs.RemoveAt(index);
			}
		}

		private int GetPairIndex(Vector3 point, Vector3 direction, float threshold)
		{
			for (var i = pairs.Count - 1; i >= 0; i--)
			{
				var pair = pairs[i];

				if (pair.A == point && Vector3.Angle(pair.B - pair.A, direction) <= threshold)
				{
					return i;
				}

				if (pair.B == point && Vector3.Angle(pair.A - pair.B, direction) <= threshold)
				{
					return i;
				}
			}

			return -1;
		}

		protected virtual void LateUpdate()
		{
			if (material != null)
			{
				if (generatedMesh == null)
				{
					UpdateMesh();
				}

				Graphics.DrawMesh(generatedMesh, transform.localToWorldMatrix, material, gameObject.layer);
			}
		}

		protected virtual void OnDestroy()
		{
			DestroyImmediate(generatedMesh);
		}
#if UNITY_EDITOR
		private static HashSet<Vector3> tempCenters = new HashSet<Vector3>();

		public void BuildFromMeshFilter(float threshold)
		{
			var meshFilter = GetComponentInParent<MeshFilter>();

			if (meshFilter != null)
			{
				var mesh = meshFilter.sharedMesh;

				if (mesh != null)
				{
					var vertices  = mesh.vertices;
					var triangles = mesh.triangles;

					Pairs.Clear();

					tempCenters.Clear();

					for (var i = 0; i < triangles.Length; i += 3)
					{
						var a = vertices[triangles[i + 0]];
						var b = vertices[triangles[i + 1]];
						var c = vertices[triangles[i + 2]];

						if (Vector3.Angle(b - a, c - a) > threshold)
						{
							AddPair(a, b);
							AddPair(a, c);
						}

						if (Vector3.Angle(a - b, c - b) > threshold)
						{
							AddPair(b, a);
							AddPair(b, c);
						}

						if (Vector3.Angle(a - c, b - c) > threshold)
						{
							AddPair(c, a);
							AddPair(c, b);
						}
					}
				}
			}
		}

		private void AddPair(Vector3 a, Vector3 b)
		{
			var center = a + b;

			if (tempCenters.Contains(center) == false)
			{
				tempCenters.Add(center);

				Pairs.Add(new Pair() { A = a, B = b });
			}
		}

		protected virtual void OnDrawGizmosSelected()
		{
			if (pairs != null)
			{
				Gizmos.matrix = transform.localToWorldMatrix;

				foreach (var pair in pairs)
				{
					Gizmos.DrawLine(pair.A, pair.B);
				}
			}
		}
#endif
	}
}

#if UNITY_EDITOR
namespace VolumetricAudio.Examples
{
	using UnityEditor;
	using TARGET = VA_Wireframe;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class VA_Wireframe_Editor : CwEditor
	{
		private static int editing;

		private static float threshold = 89.0f;

		public virtual void OnSceneGUI()
		{
			TARGET tgt = (TARGET)target;

			if (editing > 0)
			{
				var matrix = tgt.transform.localToWorldMatrix;

				Handles.BeginGUI();
				{
					var title = "";

					switch (editing)
					{
						case 1: title = "x"; break;
						case 2: title = "-"; break;
					}

					for (var i = tgt.Pairs.Count - 1; i >= 0; i--)
					{
						var pair     = tgt.Pairs[i];
						var mid      = (pair.A + pair.B) * 0.5f;
						var scrPoint = Camera.current.WorldToScreenPoint(matrix.MultiplyPoint(mid));

						if (scrPoint.z > 0.0f)
						{
							if (GUI.Button(new Rect(scrPoint.x - 5.0f, Screen.height - scrPoint.y - 45.0f, 20.0f, 20.0f), title) == true)
							{
								Undo.RecordObject(tgt, "Remove Pair");

								switch (editing)
								{
									case 1: tgt.RemoveOne(i); break;
									case 2: tgt.RemoveLoop(i, threshold); break;
								}

								tgt.UpdateMesh();

								EditorUtility.SetDirty(tgt);
							}
						}
					}
				}
				Handles.EndGUI();
			}

			if (GUI.changed == true)
			{
				EditorUtility.SetDirty(target);
			}
		}

		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.Material == null));
				Draw("material", "The material used to render the wireframe mesh.");
			EndError();

			BeginError(Any(tgts, t => t.Pairs.Count == 0));
				Draw("pairs");
			EndError();

			Separator();

			threshold = EditorGUILayout.FloatField("Threshold", threshold);

			if (GUILayout.Button("Edit Del 1") == true)
			{
				editing = editing > 0 ? 0 : 1;
			}

			if (GUILayout.Button("Edit Del N") == true)
			{
				editing = editing > 0 ? 0 : 2;
			}

			if (GUILayout.Button("Build From MeshFilter") == true)
			{
				Each(tgts, t => { t.BuildFromMeshFilter(threshold); t.UpdateMesh(); }, true, undo: "Build From MeshFilter");
			}
		}
	}
}
#endif