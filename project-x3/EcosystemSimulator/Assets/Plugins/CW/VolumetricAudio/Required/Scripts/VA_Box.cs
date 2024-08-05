using UnityEngine;
using CW.Common;

namespace VolumetricAudio
{
	/// <summary>This component allows you to define a box shape that can emit sound.</summary>
	[ExecuteInEditMode]
	[HelpURL(VA_Common.HelpUrlPrefix + "VA_Box")]
	[AddComponentMenu(VA_Common.ComponentMenuPrefix + "Box")]
	public class VA_Box : VA_VolumetricShape
	{
		/// <summary>If you set this, then all shape settings will automatically be copied from the collider.</summary>
		public BoxCollider BoxCollider { set { boxCollider = value; } get { return boxCollider; } } [SerializeField] private BoxCollider boxCollider;

		/// <summary>The center of the box shape.</summary>
		public Vector3 Center { set { center = value; } get { return center; } } [SerializeField] private Vector3 center;

		/// <summary>The size of the box shape.</summary>
		public Vector3 Size { set { size = value; } get { return size; } } [SerializeField] private Vector3 size = Vector3.one;

		private Matrix4x4 cachedMatrix = Matrix4x4.identity;

		public void RebuildMatrix()
		{
			var position = transform.TransformPoint(center);
			var rotation = transform.rotation;
			var scale    = transform.lossyScale;

			scale.x *= size.x;
			scale.y *= size.y;
			scale.z *= size.z;

			VA_Common.MatrixTrs(position, rotation, scale, ref cachedMatrix);
			//return VA_Common.TranslationMatrix(position) * VA_Common.RotationMatrix(rotation) * VA_Common.ScalingMatrix(scale);
		}

		public override bool LocalPointInShape(Vector3 localPoint)
		{
			return LocalPointInBox(localPoint);
		}

		protected virtual void Reset()
		{
			boxCollider = GetComponent<BoxCollider>();
		}

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
					if (LocalPointInBox(localPoint) == true)
					{
						SetInnerPoint(worldPoint, true);

						localPoint = SnapLocalPoint(localPoint);
						worldPoint = cachedMatrix.MultiplyPoint(localPoint);

						SetOuterPoint(worldPoint);
					}
					else
					{
						localPoint = ClipLocalPoint(localPoint);
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
				Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
			}
		}
#endif

		private void UpdateFields()
		{
			if (boxCollider != null)
			{
				center = boxCollider.center;
				size   = boxCollider.size;
			}
		}

		private bool LocalPointInBox(Vector3 localPoint)
		{
			if (localPoint.x < -0.5f) return false;
			if (localPoint.x >  0.5f) return false;

			if (localPoint.y < -0.5f) return false;
			if (localPoint.y >  0.5f) return false;

			if (localPoint.z < -0.5f) return false;
			if (localPoint.z >  0.5f) return false;

			return true;
		}

		private Vector3 SnapLocalPoint(Vector3 localPoint)
		{
			var x = Mathf.Abs(localPoint.x);
			var y = Mathf.Abs(localPoint.y);
			var z = Mathf.Abs(localPoint.z);

			// X largest?
			if (x > y && x > z)
			{
				localPoint *= CwHelper.Reciprocal(x * 2.0f);
			}
			// Y largest?
			else if (y > x && y > z)
			{
				localPoint *= CwHelper.Reciprocal(y * 2.0f);
			}
			// Z largest?
			else
			{
				localPoint *= CwHelper.Reciprocal(z * 2.0f);
			}

			return localPoint;
		}

		private Vector3 ClipLocalPoint(Vector3 localPoint)
		{
			if (localPoint.x < -0.5f) localPoint.x = -0.5f;
			if (localPoint.x >  0.5f) localPoint.x =  0.5f;

			if (localPoint.y < -0.5f) localPoint.y = -0.5f;
			if (localPoint.y >  0.5f) localPoint.y =  0.5f;

			if (localPoint.z < -0.5f) localPoint.z = -0.5f;
			if (localPoint.z >  0.5f) localPoint.z =  0.5f;

			return localPoint;
		}
	}
}

#if UNITY_EDITOR
namespace VolumetricAudio
{
	using UnityEditor;
	using TARGET = VA_Box;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class VA_Box_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("boxCollider", "If you set this, then all shape settings will automatically be copied from the collider.");

			if (Any(tgts, t => t.BoxCollider == null))
			{
				Draw("center", "The center of the box shape.");
				Draw("size", "The size of the box shape.");
			}

			Draw("isHollow", "If you set this, then sound will only emit from the thin shell around the shape, else it will emit from inside too.");
		}
	}
}
#endif