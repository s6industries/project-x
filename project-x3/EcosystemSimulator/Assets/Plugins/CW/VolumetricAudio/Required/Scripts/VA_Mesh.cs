using UnityEngine;
using CW.Common;

namespace VolumetricAudio
{
	/// <summary>This component allows you to define a mesh shape that can emit sound.</summary>
	[ExecuteInEditMode]
	[HelpURL(VA_Common.HelpUrlPrefix + "VA_Mesh")]
	[AddComponentMenu(VA_Common.ComponentMenuPrefix + "Mesh")]
	public class VA_Mesh : VA_VolumetricShape
	{
		/// <summary>If you set this, then all shape settings will automatically be copied from the collider.</summary>
		public MeshCollider MeshCollider { set { meshCollider = value; } get { return meshCollider; } } [SerializeField] private MeshCollider meshCollider;

		/// <summary>If you set this, then all shape settings will automatically be copied from the filter.</summary>
		public MeshFilter MeshFilter { set { meshFilter = value; } get { return meshFilter; } } [SerializeField] private MeshFilter meshFilter;

		/// <summary>The mesh of the mesh shape.</summary>
		public Mesh Mesh { set { mesh = value; } get { return mesh; } } [SerializeField] private Mesh mesh;

		/// <summary>The interval between each mesh update in seconds
		/// -1 = No updates.</summary>
		public float MeshUpdateInterval { set { meshUpdateInterval = value; } get { return meshUpdateInterval; } } [SerializeField] private float meshUpdateInterval = -1.0f;

		/// <summary>How far apart each volume checking ray should be separated to avoid miscalculations. This value should be based on the size of your mesh, but be kept quite low.</summary>
		public float RaySeparation { set { raySeparation = value; } get { return raySeparation; } } [SerializeField] private float raySeparation = 0.1f;

		[SerializeField]
		private VA_MeshTree tree;

		[System.NonSerialized]
		private VA_MeshLinear linear;

		[System.NonSerialized]
		private float meshUpdateCooldown;

		public bool IsBaked
		{
			get
			{
				return tree != null && tree.Nodes != null && tree.Nodes.Count > 0;
			}
		}

		public void ClearBake()
		{
			if (tree != null)
			{
				tree.Clear();
			}
		}

		public void Bake()
		{
			if (tree == null) tree = new VA_MeshTree();

			tree.Update(mesh);

			if (linear != null)
			{
				linear.Clear();
			}
		}

		public override bool LocalPointInShape(Vector3 localPoint)
		{
			var worldPoint = transform.TransformPoint(localPoint);

			return PointInMesh(localPoint, worldPoint);
		}

		protected virtual void Reset()
		{
			isHollow     = true; // NOTE: This is left as true by default to prevent applying volume to meshes with holes
			meshCollider = GetComponent<MeshCollider>();
			meshFilter   = GetComponent<MeshFilter>();
		}

		protected override void LateUpdate()
		{
			base.LateUpdate();

			// Update mesh?
			if (meshUpdateCooldown > meshUpdateInterval)
			{
				meshUpdateCooldown = meshUpdateInterval;
			}

			if (meshUpdateInterval >= 0.0f)
			{
				meshUpdateCooldown -= Time.deltaTime;

				if (meshUpdateCooldown <= 0.0f)
				{
					meshUpdateCooldown = meshUpdateInterval;

					if (IsBaked == true)
					{
						if (tree != null)
						{
							tree.Update(mesh);
						}
					}
					else
					{
						if (linear != null)
						{
							linear.Update(mesh);
						}
					}
				}
			}

			// Make sure the listener exists
			var listenerPosition = default(Vector3);

			if (VA_Common.GetListenerPosition(ref listenerPosition) == true)
			{
				UpdateFields();

				var worldPoint = listenerPosition;
				var localPoint = transform.InverseTransformPoint(worldPoint);

				if (mesh != null)
				{
					if (isHollow == true)
					{
						localPoint = SnapLocalPoint(localPoint);
						worldPoint = transform.TransformPoint(localPoint);

						SetOuterPoint(worldPoint);
					}
					else
					{
						if (PointInMesh(localPoint, worldPoint) == true)
						{
							SetInnerPoint(worldPoint, true);

							localPoint = SnapLocalPoint(localPoint);
							worldPoint = transform.TransformPoint(localPoint);

							SetOuterPoint(worldPoint);
						}
						else
						{
							localPoint = SnapLocalPoint(localPoint);
							worldPoint = transform.TransformPoint(localPoint);

							SetInnerOuterPoint(worldPoint, false);
						}
					}
				}
			}
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			if (isActiveAndEnabled == true)
			{
				UpdateFields();

				Gizmos.color  = Color.red;
				Gizmos.matrix = transform.localToWorldMatrix;

				if (IsBaked == true)
				{
					for (var i = tree.Triangles.Count - 1; i >= 0; i--)
					{
						var triangle = tree.Triangles[i];

						Gizmos.DrawLine(triangle.A, triangle.B);
						Gizmos.DrawLine(triangle.B, triangle.C);
						Gizmos.DrawLine(triangle.C, triangle.A);
					}
				}
				else
				{
					if (mesh != null)
					{
						var positions = mesh.vertices;

						for (var i = 0; i < mesh.subMeshCount; i++)
						{
							switch (mesh.GetTopology(i))
							{
								case MeshTopology.Triangles:
								{
									var indices = mesh.GetTriangles(i);

									for (var j = 0; j < indices.Length; j += 3)
									{
										var point1 = positions[indices[j + 0]];
										var point2 = positions[indices[j + 1]];
										var point3 = positions[indices[j + 2]];

										Gizmos.DrawLine(point1, point2);
										Gizmos.DrawLine(point2, point3);
										Gizmos.DrawLine(point3, point1);
									}
								}
								break;
							}
						}
					}
				}
			}
		}
#endif

		private Vector3 FindClosestLocalPoint(Vector3 localPoint)
		{
			// Tree search?
			if (IsBaked == true)
			{
				return tree.FindClosestPoint(localPoint);
			}
			// Linear search?
			else
			{
				if (linear == null)
				{
					linear = new VA_MeshLinear();
				}

				if (linear.HasTriangles == false)
				{
					linear.Update(mesh);
				}

				return linear.FindClosestPoint(localPoint);
			}
		}

		private void UpdateFields()
		{
			if (meshCollider != null)
			{
				mesh = meshCollider.sharedMesh;
			}
			else if (meshFilter != null)
			{
				mesh = meshFilter.sharedMesh;
			}
		}

		private int RaycastHitCount(Vector3 origin, Vector3 direction, float separation)
		{
			var hitCount = 0;

			if (meshCollider != null && separation > 0.0f)
			{
				var meshSize = Vector3.Magnitude(meshCollider.bounds.size);
				var lengthA  = meshSize;
				var lengthB  = meshSize;
				var rayA     = new Ray(origin, direction);
				var rayB     = new Ray(origin + direction * meshSize, -direction);
				var hit      = default(RaycastHit);

				for (var i = 0; i < 50; i++)
				{
					if (meshCollider.Raycast(rayA, out hit, lengthA) == true)
					{
						lengthA -= hit.distance + separation;

						rayA.origin = hit.point + rayA.direction * separation; hitCount += 1;
					}
					else
					{
						break;
					}
				}

				for (var i = 0; i < 50; i++)
				{
					if (meshCollider.Raycast(rayB, out hit, lengthB) == true)
					{
						lengthB -= hit.distance + separation;

						rayB.origin = hit.point + rayB.direction * separation; hitCount += 1;
					}
					else
					{
						break;
					}
				}
			}

			return hitCount;
		}

		private bool PointInMesh(Vector3 localPoint, Vector3 worldPoint)
		{
			if (mesh.bounds.Contains(localPoint) == false) return false;

			var hitCount = RaycastHitCount(worldPoint, Vector3.up, raySeparation);

			if (hitCount == 0 || hitCount % 2 == 0) return false;

			return true;
		}

		private Vector3 SnapLocalPoint(Vector3 localPoint)
		{
			return FindClosestLocalPoint(localPoint);
		}
	}
}

#if UNITY_EDITOR
namespace VolumetricAudio
{
	using UnityEditor;
	using TARGET = VA_Mesh;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class VA_Mesh_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.MeshCollider == null && t.IsHollow == false));
				Draw("meshCollider", "If you set this, then all shape settings will automatically be copied from the collider.");
			EndError();
			Draw("meshFilter", "If you set this, then all shape settings will automatically be copied from the filter.");
			BeginError(Any(tgts, t => t.Mesh == null));
				Draw("mesh", "The mesh of the mesh shape.");
			EndError();

			if (Any(tgts, t => t.Mesh != null && t.Mesh.isReadable == false))
			{
				EditorGUILayout.HelpBox("This mesh is not readable.", MessageType.Error);
			}

			if (Any(tgts, t => t.Mesh != null && t.Mesh.vertexCount > 2000 && t.IsBaked == false))
			{
				EditorGUILayout.HelpBox("This mesh has a lot of vertices, so it may run slowly. If this mesh isn't dynamic then click Bake below.", MessageType.Warning);
			}

			Draw("meshUpdateInterval", "The interval between each mesh update in seconds\n\n-1 = No updates.");

			Draw("isHollow", "If you set this, then sound will only emit from the thin shell around the shape, else it will emit from inside too.");

			if (Any(tgts, t => t.IsHollow == false && t.MeshCollider == null))
			{
				EditorGUILayout.HelpBox("Non hollow meshes require a MeshCollider to be set.", MessageType.Error);
			}

			if (Any(tgts, t => t.IsHollow == false))
			{
				BeginError(Any(tgts, t => t.RaySeparation <= 0.0f));
					Draw("raySeparation", "How far apart each volume checking ray should be separated to avoid miscalculations. This value should be based on the size of your mesh, but be kept quite low.");
				EndError();
			}

			EditorGUILayout.Separator();

			if (Any(tgts, t => t.Mesh != null))
			{
				if (GUILayout.Button("Bake Mesh") == true)
				{
					Each(tgts, t => t.Bake(), true, undo: "Bake Mesh");
				}
			}

			if (Any(tgts, t => t.IsBaked == true))
			{
				if (GUILayout.Button("Clear Baked Mesh") == true)
				{
					Each(tgts, t => t.ClearBake(), true, undo: "Clear Baked Mesh");
				}

				EditorGUILayout.HelpBox("This mesh has been baked for faster computation. If your mesh has been modified then press 'Bake Mesh' again.", MessageType.Info);
			}
		}
	}
}
#endif