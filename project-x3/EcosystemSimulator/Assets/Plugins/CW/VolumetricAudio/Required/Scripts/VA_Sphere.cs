using UnityEngine;
using CW.Common;

namespace VolumetricAudio
{
	/// <summary>This component allows you to define a path shape that can emit sound.</summary>
	[ExecuteInEditMode]
	[HelpURL(VA_Common.HelpUrlPrefix + "VA_Sphere")]
	[AddComponentMenu(VA_Common.ComponentMenuPrefix + "Sphere")]
	public class VA_Sphere : VA_VolumetricShape
	{
		/// <summary>If you set this, then all shape settings will automatically be copied from the collider.</summary>
		public SphereCollider SphereCollider { set { sphereCollider = value; } get { return sphereCollider; } } [SerializeField] private SphereCollider sphereCollider;

		/// <summary>The center of the sphere shape.</summary>
		public Vector3 Center { set { center = value; } get { return center; } } [SerializeField] private Vector3 center;

		/// <summary>The radius of the sphere shape.</summary>
		public float Radius { set { radius = value; } get { return radius; } } [SerializeField] private float radius = 1.0f;

		private Matrix4x4 cachedMatrix = Matrix4x4.identity;

		public void RebuildMatrix()
		{
			var position = transform.TransformPoint(center);
			var rotation = transform.rotation;
			var scale    = transform.lossyScale;

			scale.x = scale.y = scale.z = Mathf.Max(Mathf.Max(scale.x, scale.y), scale.z);

			VA_Common.MatrixTrs(position, rotation, scale, ref cachedMatrix);
			//return VA_Common.TranslationMatrix(position) * VA_Common.RotationMatrix(rotation) * VA_Common.ScalingMatrix(scale);
		}

		public override bool LocalPointInShape(Vector3 localPoint)
		{
			return LocalPointInSphere(localPoint);
		}

		protected virtual void Awake()
		{
			sphereCollider = GetComponent<SphereCollider>();
		}
#if UNITY_EDITOR
		protected virtual void Reset()
		{
			Awake();
		}
#endif
		protected override void LateUpdate()
		{
			base.LateUpdate();

			// Make sure the listener exists
			var listenerPosition = default(Vector3);

			if (VA_Common.GetListenerPosition(ref listenerPosition) == true)
			{
				UpdateFields();
				RebuildMatrix();

				var worldPoint = listenerPosition;
				var localPoint = cachedMatrix.inverse.MultiplyPoint(worldPoint);

				if (isHollow == true)
				{
					localPoint = SnapLocalPoint(localPoint);
					worldPoint = cachedMatrix.MultiplyPoint(localPoint);

					SetOuterPoint(worldPoint);
				}
				else
				{
					if (LocalPointInSphere(localPoint) == true)
					{
						SetInnerPoint(worldPoint, true);

						localPoint = SnapLocalPoint(localPoint);
						worldPoint = cachedMatrix.MultiplyPoint(localPoint);

						SetOuterPoint(worldPoint);
					}
					else
					{
						localPoint = SnapLocalPoint(localPoint);
						worldPoint = cachedMatrix.MultiplyPoint(localPoint);

						SetInnerOuterPoint(worldPoint, false);
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
				RebuildMatrix();

				Gizmos.color  = Color.red;
				Gizmos.matrix = cachedMatrix;
				Gizmos.DrawWireSphere(Vector3.zero, radius);
			}
		}
#endif
		private void UpdateFields()
		{
			if (sphereCollider != null)
			{
				center = sphereCollider.center;
				radius = sphereCollider.radius;
			}
		}

		private bool LocalPointInSphere(Vector3 localPoint)
		{
			return localPoint.sqrMagnitude < radius * radius;
		}

		private Vector3 SnapLocalPoint(Vector3 localPoint)
		{
			return localPoint.normalized * radius;
		}
	}
}

#if UNITY_EDITOR
namespace VolumetricAudio
{
	using UnityEditor;
	using TARGET = VA_Sphere;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class VA_Sphere_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("sphereCollider", "If you set this, then all shape settings will automatically be copied from the collider.");

			if (Any(tgts, t => t.SphereCollider == null))
			{
				Draw("center", "The center of the sphere shape.");
				Draw("radius", "The radius of the sphere shape.");
			}

			Draw("isHollow", "If you set this, then sound will only emit from the thin shell around the shape, else it will emit from inside too.");
		}
	}
}
#endif