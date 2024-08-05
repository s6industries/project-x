using UnityEngine;

namespace VolumetricAudio
{
	/// <summary>This is the base class for all shapes (e.g. <b>VA_Path</b>).</summary>
	public abstract class VA_Shape : MonoBehaviour
	{
		// Does this shape have an outer point? (the closest point between this shape's outer shell and the audio listener)
		public bool OuterPointSet;

		// The position of the outer point
		public Vector3 OuterPoint;

		// The distance between the outer point and the audio listener
		public float OuterPointDistance;

		public virtual bool FinalPointSet
		{
			get
			{
				return OuterPointSet;
			}
		}

		public virtual Vector3 FinalPoint
		{
			get
			{
				return OuterPoint;
			}
		}

		public virtual float FinalPointDistance
		{
			get
			{
				return OuterPointDistance;
			}
		}

		public void SetOuterPoint(Vector3 newOuterPoint)
		{
			// Make sure the listener exists
			var listenerPosition = default(Vector3);

			if (VA_Common.GetListenerPosition(ref listenerPosition) == true)
			{
				OuterPointSet      = true;
				OuterPoint         = newOuterPoint;
				OuterPointDistance = Vector3.Distance(listenerPosition, newOuterPoint);
			}
		}

		protected virtual void LateUpdate()
		{
			OuterPointSet = false;
		}
	}
}