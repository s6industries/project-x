using UnityEngine;
using CW.Common;

namespace VolumetricAudio
{
	/// <summary>This component allows you to define a capsule shape that can emit sound.</summary>
	[ExecuteInEditMode]
	[HelpURL(VA_Common.HelpUrlPrefix + "VA_Capsule")]
	[AddComponentMenu(VA_Common.ComponentMenuPrefix + "Capsule")]
	public class VA_Capsule : VA_VolumetricShape
	{
		/// <summary>If you set this, then all shape settings will automatically be copied from the collider.</summary>
		public CapsuleCollider CapsuleCollider { set { capsuleCollider = value; } get { return capsuleCollider; } } [SerializeField] private CapsuleCollider capsuleCollider;

		/// <summary>The center of the capsule shape.</summary>
		public Vector3 Center { set { center = value; } get { return center; } } [SerializeField] private Vector3 center;

		/// <summary>The radius of the capsule shape.</summary>
		public float Radius { set { radius = value; } get { return radius; } } [SerializeField] private float radius = 1.0f;

		/// <summary>The height of the capsule shape.</summary>
		public float Height { set { height = value; } get { return height; } } [SerializeField] private float height = 2.0f;

		/// <summary>The direction of the capsule shape.</summary>
		public int Direction { set { direction = value; } get { return direction; } } [SerializeField] [VA_Popup("X-Axis", "Y-Axis", "Z-Axis")] private int direction = 1;

		private static Quaternion RotationX = Quaternion.Euler(0.0f, 0.0f, 90.0f);

		private static Quaternion RotationY = Quaternion.identity;

		private static Quaternion RotationZ = Quaternion.Euler(90.0f, 0.0f, 0.0f);

		private Matrix4x4 cachedMatrix = Matrix4x4.identity;

		public void RebuildMatrix()
		{
			var position = transform.TransformPoint(center);
			var rotation = transform.rotation;
			var scale    = transform.lossyScale;

			switch (direction)
			{
				case 0: rotation *= RotationX; break;
				case 1: rotation *= RotationY; break;
				case 2: rotation *= RotationZ; break;
			}

			scale.x = scale.y = scale.z = Mathf.Max(scale.x, scale.z);

			VA_Common.MatrixTrs(position, rotation, scale, ref cachedMatrix);
			//matrix = VA_Common.TranslationMatrix(position) * VA_Common.RotationMatrix(rotation) * matrix * VA_Common.ScalingMatrix(scale);
		}

		public override bool LocalPointInShape(Vector3 localPoint)
		{
			var halfHeight = Mathf.Max(0.0f, height * 0.5f - radius);

			return LocalPointInCapsule(localPoint, halfHeight);
		}

		protected virtual void Awake()
		{
			capsuleCollider = GetComponent<CapsuleCollider>();
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
				var scale      = transform.lossyScale;
				var squash     = CwHelper.Divide(scale.y, Mathf.Max(scale.x, scale.z));
				var halfHeight = Mathf.Max(0.0f, height * squash * 0.5f - radius);

				if (isHollow == true)
				{
					localPoint = SnapLocalPoint(localPoint, halfHeight);
					worldPoint = cachedMatrix.MultiplyPoint(localPoint);

					SetOuterPoint(worldPoint);
				}
				else
				{
					if (LocalPointInCapsule(localPoint, halfHeight) == true)
					{
						SetInnerPoint(worldPoint, true);

						localPoint = SnapLocalPoint(localPoint, halfHeight);
						worldPoint = cachedMatrix.MultiplyPoint(localPoint);

						SetOuterPoint(worldPoint);
					}
					else
					{
						localPoint = SnapLocalPoint(localPoint, halfHeight);
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

				var scale      = transform.lossyScale;
				var squash     = CwHelper.Divide(scale.y, Mathf.Max(scale.x, scale.z));
				var halfHeight = Mathf.Max(0.0f, height * squash * 0.5f - radius);
				var point1     = Vector3.up *  halfHeight;
				var point2     = Vector3.up * -halfHeight;

				Gizmos.color  = Color.red;
				Gizmos.matrix = cachedMatrix;
				Gizmos.DrawWireSphere(point1, radius);
				Gizmos.DrawWireSphere(point2, radius);
				Gizmos.DrawLine(point1 + Vector3.right   * radius, point2 + Vector3.right   * radius);
				Gizmos.DrawLine(point1 - Vector3.right   * radius, point2 - Vector3.right   * radius);
				Gizmos.DrawLine(point1 + Vector3.forward * radius, point2 + Vector3.forward * radius);
				Gizmos.DrawLine(point1 - Vector3.forward * radius, point2 - Vector3.forward * radius);
			}
		}
#endif

		private void UpdateFields()
		{
			if (capsuleCollider != null)
			{
				center    = capsuleCollider.center;
				radius    = capsuleCollider.radius;
				height    = capsuleCollider.height;
				direction = capsuleCollider.direction;
			}
		}

		private bool LocalPointInCapsule(Vector3 localPoint, float halfHeight)
		{
			// Top
			if (localPoint.y > halfHeight)
			{
				localPoint.y -= halfHeight;

				return localPoint.sqrMagnitude < radius * radius;
			}
			// Bottom
			else if (localPoint.y < -halfHeight)
			{
				localPoint.y += halfHeight;

				return localPoint.sqrMagnitude < radius * radius;
			}
			// Middle
			else
			{
				localPoint.y = 0.0f;

				return localPoint.sqrMagnitude < radius * radius;
			}
		}

		private Vector3 SnapLocalPoint(Vector3 localPoint, float halfHeight)
		{
			// Top
			if (localPoint.y > halfHeight)
			{
				localPoint.y -= halfHeight;

				localPoint = localPoint.normalized * radius;
				localPoint.y += halfHeight;
			}
			// Bottom
			else if (localPoint.y < -halfHeight)
			{
				localPoint.y += halfHeight;

				localPoint = localPoint.normalized * radius;
				localPoint.y -= halfHeight;
			}
			// Middle
			else
			{
				var oldY = localPoint.y; localPoint.y = 0.0f;

				localPoint = localPoint.normalized * radius;
				localPoint.y = oldY;
			}

			return localPoint;
		}
	}
}

#if UNITY_EDITOR
namespace VolumetricAudio
{
	using UnityEditor;
	using TARGET = VA_Capsule;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class VA_Capsule_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("capsuleCollider", "If you set this, then all shape settings will automatically be copied from the collider.");

			if (Any(tgts, t => t.CapsuleCollider == null))
			{
				Draw("center", "The center of the capsule shape.");
				Draw("radius", "The radius of the capsule shape.");
				Draw("height", "The height of the capsule shape.");
				Draw("direction", "The direction of the capsule shape.");
			}

			Draw("isHollow", "If you set this, then sound will only emit from the thin shell around the shape, else it will emit from inside too.");
		}
	}
}
#endif