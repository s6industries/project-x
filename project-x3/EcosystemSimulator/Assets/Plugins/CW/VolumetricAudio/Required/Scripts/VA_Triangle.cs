using UnityEngine;

namespace VolumetricAudio
{
	/// <summary>This class defines a serializable triangle.</summary>
	[System.Serializable]
	public class VA_Triangle
	{
		// Points
		public Vector3  A;
		public Vector3  B;
		public Vector3  C;

		// Planes
		public VA_Plane PlaneABC;
		public VA_Plane PlaneAB;
		public VA_Plane PlaneBC;
		public VA_Plane PlaneCA;

		public Vector3 Min
		{
			get
			{
				return Vector3.Min(A, Vector3.Min(B, C));
			}
		}

		public Vector3 Max
		{
			get
			{
				return Vector3.Max(A, Vector3.Max(B, C));
			}
		}

		public float MidX
		{
			get
			{
				return (A.x + B.x + C.x) / 3.0f;
			}
		}

		public float MidY
		{
			get
			{
				return (A.y + B.y + C.y) / 3.0f;
			}
		}

		public float MidZ
		{
			get
			{
				return (A.z + B.z + C.z) / 3.0f;
			}
		}

		public void CalculatePlanes()
		{
			PlaneABC = new VA_Plane(A, B, C);
			PlaneAB  = new VA_Plane(A, B, A + PlaneABC.Normal);
			PlaneBC  = new VA_Plane(B, C, B + PlaneABC.Normal);
			PlaneCA  = new VA_Plane(C, A, C + PlaneABC.Normal);
		}

		public Vector3 ClosestTo(Vector3 p)
		{
			if (PlaneAB.SideOf(p) == true) return ClosestPointToLineSegment(A, B, p);
			if (PlaneBC.SideOf(p) == true) return ClosestPointToLineSegment(B, C, p);
			if (PlaneCA.SideOf(p) == true) return ClosestPointToLineSegment(C, A, p);

			return PlaneABC.ClosestTo(p);
		}

		private static Vector3 ClosestPointToLineSegment(Vector3 a, Vector3 b, Vector3 p)
		{
			var d  = b - a;
			var dd = Vector3.Dot(d, d);

			if (dd == 0.0f)
			{
				return a;
			}

			var dist01 = Vector3.Dot(p - a, d) / dd;

			if (dist01 < 0.0f) return a;

			if (dist01 > 1.0f) return b;

			return a + d * dist01;
		}
	}
}