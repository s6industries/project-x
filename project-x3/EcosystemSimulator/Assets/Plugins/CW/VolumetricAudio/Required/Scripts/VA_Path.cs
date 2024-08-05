using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace VolumetricAudio
{
	/// <summary>This component allows you to define a path shape that can emit sound.
	/// If you select a GameObject with this component then you can edit the points in the <b>Scene</b> view.</summary>
	[ExecuteInEditMode]
	[HelpURL(VA_Common.HelpUrlPrefix + "VA_Path")]
	[AddComponentMenu(VA_Common.ComponentMenuPrefix + "Path")]
	public class VA_Path : VA_Shape
	{
		/// <summary>The local space points for the path.</summary>
		public List<Vector3> Points { get { if (points == null) points = new List<Vector3>(); return points; } } [SerializeField] private List<Vector3> points;

		protected override void LateUpdate()
		{
			base.LateUpdate();

			// Make sure the listener exists
			var listenerPosition = default(Vector3);

			if (VA_Common.GetListenerPosition(ref listenerPosition) == true)
			{
				if (points != null && points.Count > 1)
				{
					var worldPoint        = listenerPosition;
					var localPoint        = transform.InverseTransformPoint(worldPoint);
					var closestDistanceSq = float.PositiveInfinity;
					var closestPoint      = Vector3.zero;

					for (var i = 1; i < points.Count; i++)
					{
						var closePoint      = VA_Common.ClosestPointToLineSegment(points[i - 1], points[i], localPoint);
						var closeDistanceSq = (closePoint - localPoint).sqrMagnitude;

						if (closeDistanceSq < closestDistanceSq)
						{
							closestDistanceSq = closeDistanceSq;
							closestPoint      = closePoint;
						}
					}

					worldPoint = transform.TransformPoint(closestPoint);

					SetOuterPoint(worldPoint);
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace VolumetricAudio
{
	using UnityEditor;
	using TARGET = VA_Path;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class VA_Path_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			serializedObject.Update();
			BeginError(Any(tgts, t => t.Points.Count < 2));
				Draw("points", "The local space points for the path.");
			EndError();
		}

		public virtual void OnSceneGUI()
		{
			TARGET tgt = (TARGET)target;

			var matrix  = tgt.transform.localToWorldMatrix;
			var inverse = tgt.transform.worldToLocalMatrix;

			for (var i = 0; i < tgt.Points.Count; i++)
			{
				var oldPoint = matrix.MultiplyPoint(tgt.Points[i]);

				Handles.matrix = Matrix4x4.Translate(oldPoint) * Matrix4x4.Scale(Vector3.one * 0.8f) * Matrix4x4.Translate(oldPoint * -1.0f);

				var newPoint = Handles.PositionHandle(oldPoint, Quaternion.identity);

				if (oldPoint != newPoint)
				{
					Undo.RecordObject(tgt, "Move Path Point");
					tgt.Points[i] = inverse.MultiplyPoint(newPoint);

					EditorUtility.SetDirty(tgt);
				}
			}

			Handles.color  = Color.red;
			Handles.matrix = tgt.transform.localToWorldMatrix;

			for (var i = 1; i < tgt.Points.Count; i++)
			{
				Handles.DrawLine(tgt.Points[i - 1], tgt.Points[i]);
			}

			Handles.BeginGUI();
			{
				for (var i = 0; i < tgt.Points.Count; i++)
				{
					var point     = tgt.Points[i];
					var pointName = "Point " + i;
					var scrPoint  = Camera.current.WorldToScreenPoint(matrix.MultiplyPoint(point));
					var rect      = new Rect(0.0f, 0.0f, 50.0f, 20.0f); rect.center = new Vector2(scrPoint.x, Screen.height - scrPoint.y - 35.0f);
					var rect1     = rect; rect.x += 1.0f;
					var rect2     = rect; rect.x -= 1.0f;
					var rect3     = rect; rect.y += 1.0f;
					var rect4     = rect; rect.y -= 1.0f;

					GUI.Label(rect1, pointName, EditorStyles.miniBoldLabel);
					GUI.Label(rect2, pointName, EditorStyles.miniBoldLabel);
					GUI.Label(rect3, pointName, EditorStyles.miniBoldLabel);
					GUI.Label(rect4, pointName, EditorStyles.miniBoldLabel);
					GUI.Label(rect, pointName, EditorStyles.whiteMiniLabel);
				}

				for (var i = 1; i < tgt.Points.Count; i++)
				{
					var pointA   = tgt.Points[i - 1];
					var pointB   = tgt.Points[i];
					var midPoint = (pointA + pointB) * 0.5f;
					var scrPoint = Camera.current.WorldToScreenPoint(matrix.MultiplyPoint(midPoint));

					if (GUI.Button(new Rect(scrPoint.x - 5.0f, Screen.height - scrPoint.y - 45.0f, 20.0f, 20.0f), "+") == true)
					{
						Undo.RecordObject(tgt, "Split Path");
						tgt.Points.Insert(i, midPoint); GUI.changed = true;
					}
				}
			}
			Handles.EndGUI();

			if (GUI.changed == true)
			{
				EditorUtility.SetDirty(target);
			}
		}
	}
}
#endif