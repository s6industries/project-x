using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace VolumetricAudio
{
	/// <summary>This component can be added to any sound to make it volumetric.</summary>
	[ExecuteInEditMode]
	[HelpURL(VA_Common.HelpUrlPrefix + "VA_AudioSource")]
	[AddComponentMenu(VA_Common.ComponentMenuPrefix + "Audio Source")]
	public class VA_AudioSource : MonoBehaviour
	{
		public enum OccludeType
		{
			Raycast,
			RaycastAll,
		}

		[System.Serializable]
		public class OccludeGroup
		{
			public LayerMask Layers;

			[Range(0.0f, 1.0f)]
			public float Volume;
		}

		/// <summary>Should this sound have its position update?</summary>
		public bool Position { set { position = value; } get { return position; } } [SerializeField] private bool position = true;

		/// <summary>The speed at which the sound position changes.
		/// 0 = Instant.</summary>
		public float PositionDamping { set { positionDamping = value; } get { return positionDamping; } } [SerializeField] private float positionDamping;

		/// <summary>The shapes you want the sound to emit from (e.g. <b>VA_Mesh</b>).</summary>
		public List<VA_Shape> Shapes { get { if (shapes == null) shapes = new List<VA_Shape>(); return shapes; } } [SerializeField] private List<VA_Shape> shapes;

		/// <summary>The shapes you want the audio source to be excluded from.</summary>
		public List<VA_VolumetricShape> ExcludedShapes { get { if (excludedShapes == null) excludedShapes = new List<VA_VolumetricShape>(); return excludedShapes; } } [SerializeField] private List<VA_VolumetricShape> excludedShapes;

		/// <summary>Should this sound have its Spatial Blend update?</summary>
		public bool Blend { set { blend = value; } get { return blend; } } [SerializeField] private bool blend;

		/// <summary>The distance at which the sound becomes fully mono.</summary>
		public float BlendMinDistance { set { blendMinDistance = value; } get { return blendMinDistance; } } [SerializeField] private float blendMinDistance = 0.0f;

		/// <summary>The distance at which the sound becomes fully stereo.</summary>
		public float BlendMaxDistance { set { blendMaxDistance = value; } get { return blendMaxDistance; } } [SerializeField] private float blendMaxDistance = 5.0f;

		/// <summary>The distribution of the mono to stereo ratio.</summary>
		public AnimationCurve BlendCurve { set { blendCurve = value; } get { return blendCurve; } } [SerializeField] private AnimationCurve blendCurve;

		/// <summary>Should this sound have its volume update?</summary>
		public bool Volume { set { volume = value; } get { return volume; } } [SerializeField] private bool volume = true;

		/// <summary>The base volume of the audio source.</summary>
		public float BaseVolume { set { baseVolume = value; } get { return baseVolume; } } [Range(0.0f, 1.0f)] [SerializeField] private float baseVolume = 1.0f;

		/// <summary>The zone this sound is associated with.</summary>
		public VA_Zone Zone { set { zone = value; } get { return zone; } } [SerializeField] private VA_Zone zone;

		/// <summary>Should the volume fade based on distance?</summary>
		public bool Fade { set { fade = value; } get { return fade; } } [SerializeField] private bool fade;

		/// <summary>The distance at which the sound fades to maximum volume.</summary>
		public float FadeMinDistance { set { fadeMinDistance = value; } get { return fadeMinDistance; } } [SerializeField] private float fadeMinDistance = 0.0f;

		/// <summary>The distance at which the sound fades to minimum volume.</summary>
		public float FadeMaxDistance { set { fadeMaxDistance = value; } get { return fadeMaxDistance; } } [SerializeField] private float fadeMaxDistance = 5.0f;

		/// <summary>The distribution of volume based on its scaled distance.</summary>
		public AnimationCurve FadeCurve { set { fadeCurve = value; } get { return fadeCurve; } } [SerializeField] private AnimationCurve fadeCurve;

		/// <summary>Should the volume fade cap to the closest distance to simulate a bullet whizzing past?</summary>
		public bool FadeWhizz { set { fadeWhizz = value; } get { return fadeWhizz; } } [SerializeField] private bool fadeWhizz;

		/// <summary>The sound volume will be calculated based on this distance rather than the actual distance. If the actual distance is lower than this, then this value will be reduced to match.</summary>
		public float FadeWhizzClosestDistance { set { fadeWhizzClosestDistance = value; } get { return fadeWhizzClosestDistance; } } [SerializeField] private float fadeWhizzClosestDistance = 5.0f;

		/// <summary>Should this sound be blocked when behind other objects?</summary>
		public bool Occlude { set { occlude = value; } get { return occlude; } } [SerializeField] private bool occlude;

		/// <summary>The raycast style against the occlusion groups.</summary>
		public OccludeType OccludeMethod { set { occludeMethod = value; } get { return occludeMethod; } } [SerializeField] private OccludeType occludeMethod;

		/// <summary>Check for <b>VA_Material</b> instances attached to the occlusion object?</summary>
		public bool OccludeMaterial { set { occludeMaterial = value; } get { return occludeMaterial; } } [SerializeField] private bool occludeMaterial;

		/// <summary>How quickly the sound fades in/out when behind an object.</summary>
		public float OccludeDamping { set { occludeDamping = value; } get { return occludeDamping; } } [SerializeField] private float occludeDamping = 5.0f;

		/// <summary>The amount of occlusion checks.</summary>
		public List<OccludeGroup> OccludeGroups { set { occludeGroups = value; } get { return occludeGroups; } } [SerializeField] private List<OccludeGroup> occludeGroups;

		/// <summary>Should this sound's dynamics change based on its distance to the camera?</summary>
		public bool Distant { set { distant = value; } get { return distant; } } [SerializeField] private bool distant;

		/// <summary>The camera distance where the minimum sound dynamics will be used.</summary>
		public float DistantMinDistance { set { distantMinDistance = value; } get { return distantMinDistance; } } [SerializeField] private float distantMinDistance = 10.0f;

		/// <summary>The camera distance where the maximum sound dynamics will be used.</summary>
		public float DistantMaxDistance { set { distantMaxDistance = value; } get { return distantMaxDistance; } } [SerializeField] private float distantMaxDistance = 100.0f;

		/// <summary>The sound dynamics used when the camera is at or below the minimum distance.</summary>
		public AudioReverbFilter DistantMinDynamics { set { distantMinDynamics = value; } get { return distantMinDynamics; } } [SerializeField] private AudioReverbFilter distantMinDynamics;

		/// <summary>The sound dynamics used when the camera is at or above the minimum distance.</summary>
		public AudioReverbFilter DistantMaxDynamics { set { distantMaxDynamics = value; } get { return distantMaxDynamics; } } [SerializeField] private AudioReverbFilter distantMaxDynamics;

		[SerializeField]
		private float currentOccludeAmount = 1.0f;

		[System.NonSerialized]
		private AudioSource cachedAudioSource;

		[System.NonSerialized]
		private AudioReverbFilter cachedReverbFilter;

		private static Keyframe[] defaultBlendCurveKeys = new Keyframe[] { new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f) };

		private static Keyframe[] defaultVolumeCurveKeys = new Keyframe[] { new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 0.0f) };

		public bool HasVolumetricShape
		{
			get
			{
				for (var i = shapes.Count - 1; i >= 0; i--)
				{
					var shape = shapes[i];

					if (shape != null)
					{
						var  sphereShape = shape as VA_Sphere;  if ( sphereShape != null &&  sphereShape.IsHollow == false) return true;
						var     boxShape = shape as VA_Box;     if (    boxShape != null &&     boxShape.IsHollow == false) return true;
						var capsuleShape = shape as VA_Capsule; if (capsuleShape != null && capsuleShape.IsHollow == false) return true;
						var    meshShape = shape as VA_Mesh;    if (   meshShape != null &&    meshShape.IsHollow == false) return true;
					}
				}

				return false;
			}
		}

		protected virtual void Start()
		{
			if (blendCurve == null)
			{
				blendCurve = new AnimationCurve(defaultBlendCurveKeys);
			}

			if (fadeCurve == null)
			{
				fadeCurve = new AnimationCurve(defaultVolumeCurveKeys);
			}
		}

		protected virtual void LateUpdate()
		{
			// Make sure the listener exists
			var listenerPosition = default(Vector3);

			if (VA_Common.GetListenerPosition(ref listenerPosition) == true)
			{
				if (position == true)
				{
					var closestDistance = float.PositiveInfinity;
					var closestShape    = default(VA_Shape);
					var closestPoint    = default(Vector3);

					// Find closest point to all shapes
					if (shapes != null)
					{
						for (var i = shapes.Count - 1; i >= 0; i--)
						{
							var shape = shapes[i];

							if (CwHelper.Enabled(shape) == true && shape.FinalPointSet == true && shape.FinalPointDistance < closestDistance)
							{
								closestDistance = shape.FinalPointDistance;
								closestPoint    = shape.FinalPoint;
								closestShape    = shape;
							}
						}
					}

					// If the closest point is closer than the excluded point, then make the excluded point the closest
					if (excludedShapes != null)
					{
						for (var i = excludedShapes.Count - 1; i >= 0; i--)
						{
							var excludedShape = excludedShapes[i];

							if (CwHelper.Enabled(excludedShape) == true && excludedShape.IsHollow == false && excludedShape.InnerPointInside == true)
							{
								if (excludedShape.OuterPointSet == true && excludedShape.OuterPointDistance > closestDistance)
								{
									closestDistance = excludedShape.OuterPointDistance;
									closestPoint    = excludedShape.OuterPoint;
									closestShape    = excludedShape;

									break;
								}
							}
						}
					}

					if (closestShape != null)
					{
						if (positionDamping <= 0.0f)
						{
							transform.position = closestPoint;
						}
						else
						{
							var factor = CwHelper.DampenFactor(positionDamping, Time.deltaTime);

							transform.position = Vector3.Lerp(transform.position, closestPoint, factor);
						}
					}
					else
					{
						closestPoint    = transform.position;
						closestDistance = Vector3.Distance(closestPoint, listenerPosition);
					}
				}

				// Modify the blend?
				if (blend == true)
				{
					var distance   = Vector3.Distance(transform.position, listenerPosition);
					var distance01 = Mathf.InverseLerp(blendMinDistance, blendMaxDistance, distance);

					SetPanLevel(blendCurve.Evaluate(distance01));
				}

				// Modify the volume?
				if (volume == true)
				{
					var finalVolume = baseVolume;

					// Modify via zone?
					if (zone != null)
					{
						finalVolume *= zone.CurrentVolume;
					}

					// Modify via distance?
					if (fade == true)
					{
						var distance = Vector3.Distance(transform.position, listenerPosition);

						if (fadeWhizz == true)
						{
							if (distance < fadeWhizzClosestDistance)
							{
								fadeWhizzClosestDistance = distance;
							}

							distance = fadeWhizzClosestDistance;
						}

						var distance01 = Mathf.InverseLerp(fadeMinDistance, fadeMaxDistance, distance);

						finalVolume *= fadeCurve.Evaluate(distance01);
					}

					// Modify via occlusion?
					if (occlude == true)
					{
						var direction    = listenerPosition - transform.position;
						var targetAmount = 1.0f;

						if (occludeGroups != null)
						{
							for (var i = occludeGroups.Count - 1; i >= 0; i--)
							{
								var group = occludeGroups[i];

								switch (occludeMethod)
								{
									case OccludeType.Raycast:
									{
										var hit = default(RaycastHit);

										if (Physics.Raycast(transform.position, direction, out hit, direction.magnitude, group.Layers) == true)
										{
											targetAmount *= GetOcclusionVolume(group, hit);
										}
									}
									break;

									case OccludeType.RaycastAll:
									{
										var hits = Physics.RaycastAll(transform.position, direction, direction.magnitude, group.Layers);

										for (var j = hits.Length - 1; j >= 0; j--)
										{
											targetAmount *= GetOcclusionVolume(group, hits[j]);
										}
									}
									break;
								}
							}
						}

						var factor = CwHelper.DampenFactor(occludeDamping, Time.deltaTime);

						currentOccludeAmount = Mathf.Lerp(currentOccludeAmount, targetAmount, factor);
						currentOccludeAmount = Mathf.MoveTowards(currentOccludeAmount, targetAmount, Time.deltaTime * 0.1f);

						finalVolume *= currentOccludeAmount;
					}

					SetVolume(finalVolume);
				}

				// Modify the sound dynamics?
				if (distant == true)
				{
					var distance   = Vector3.Distance(transform.position, listenerPosition);
					var distance01 = Mathf.InverseLerp(blendMinDistance, blendMaxDistance, distance);

					SetDynamics(distance01);
				}
			}
		}

		private float GetOcclusionVolume(OccludeGroup group, RaycastHit hit)
		{
			if (occludeMaterial == true)
			{
				var material = hit.collider.GetComponentInParent<VA_Material>();

				if (material != null)
				{
					return material.OcclusionVolume;
				}
			}

			return group.Volume;
		}

		// If you're not using Unity's built-in audio system, then modify the code below, or make a new component that inherits VA_AudioSource and overrides this method
		protected virtual void SetPanLevel(float newPanLevel)
		{
			if (cachedAudioSource == null) cachedAudioSource = GetComponent<AudioSource>();

			if (cachedAudioSource != null)
			{
				cachedAudioSource.spatialBlend = newPanLevel;
			}
		}

		// If you're not using Unity's built-in audio system, then modify the code below, or make a new component that inherits VA_AudioSource and overrides this method
		protected virtual void SetVolume(float newVolume)
		{
			if (cachedAudioSource == null) cachedAudioSource = GetComponent<AudioSource>();

			if (cachedAudioSource != null)
			{
				cachedAudioSource.volume = newVolume;
			}
		}

		protected virtual void SetDynamics(float blend)
		{
			if (distantMinDynamics != null && distantMaxDynamics != null)
			{
				if (cachedReverbFilter == null) cachedReverbFilter = GetComponent<AudioReverbFilter>();
				if (cachedReverbFilter == null) cachedReverbFilter = gameObject.AddComponent<AudioReverbFilter>();

				cachedReverbFilter.roomLF            = Mathf.Lerp(distantMinDynamics.roomLF           , distantMaxDynamics.roomLF           , blend);
				cachedReverbFilter.hfReference       = Mathf.Lerp(distantMinDynamics.hfReference      , distantMaxDynamics.hfReference      , blend);
				cachedReverbFilter.density           = Mathf.Lerp(distantMinDynamics.density          , distantMaxDynamics.density          , blend);
				cachedReverbFilter.diffusion         = Mathf.Lerp(distantMinDynamics.diffusion        , distantMaxDynamics.diffusion        , blend);
				cachedReverbFilter.reverbDelay       = Mathf.Lerp(distantMinDynamics.reverbDelay      , distantMaxDynamics.reverbDelay      , blend);
				cachedReverbFilter.reverbLevel       = Mathf.Lerp(distantMinDynamics.reverbLevel      , distantMaxDynamics.reverbLevel      , blend);
				cachedReverbFilter.reflectionsDelay  = Mathf.Lerp(distantMinDynamics.reflectionsDelay , distantMaxDynamics.reflectionsDelay , blend);
				cachedReverbFilter.reflectionsLevel  = Mathf.Lerp(distantMinDynamics.reflectionsLevel , distantMaxDynamics.reflectionsLevel , blend);
				cachedReverbFilter.decayHFRatio      = Mathf.Lerp(distantMinDynamics.decayHFRatio     , distantMaxDynamics.decayHFRatio     , blend);
				cachedReverbFilter.decayTime         = Mathf.Lerp(distantMinDynamics.decayTime        , distantMaxDynamics.decayTime        , blend);
				//cachedReverbFilter.roomRolloffFactor = Mathf.Lerp(distantMinDynamics.roomRolloffFactor, distantMaxDynamics.roomRolloffFactor, blend);
				cachedReverbFilter.roomHF            = Mathf.Lerp(distantMinDynamics.roomHF           , distantMaxDynamics.roomHF           , blend);
				cachedReverbFilter.room              = Mathf.Lerp(distantMinDynamics.room             , distantMaxDynamics.room             , blend);
				cachedReverbFilter.dryLevel          = Mathf.Lerp(distantMinDynamics.dryLevel         , distantMaxDynamics.dryLevel         , blend);
				cachedReverbFilter.lfReference       = Mathf.Lerp(distantMinDynamics.lfReference      , distantMaxDynamics.lfReference      , blend);
				//cachedReverbFilter.lFReference       = Mathf.Lerp(distantMinDynamics.lFReference      , distantMaxDynamics.lFReference      , blend);
			}
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			if (CwHelper.Enabled(this) == true)
			{
				// Draw red lines from this audio source to all shapes
				Gizmos.color = Color.red;

				if (shapes != null)
				{
					for (var i = shapes.Count - 1; i >= 0; i--)
					{
						var shape = shapes[i];

						if (CwHelper.Enabled(shape) == true && shape.FinalPointSet == true)
						{
							Gizmos.DrawLine(transform.position, shape.FinalPoint);
						}
					}
				}

				// Draw green spheres for blend distances
				if (blend == true)
				{
					for (var i = 0; i <= 50; i++)
					{
						var frac = i * 0.02f;

						Gizmos.color = new Color(0.0f, 1.0f, 0.0f, blendCurve.Evaluate(frac));

						Gizmos.DrawWireSphere(transform.position, Mathf.Lerp(blendMinDistance, blendMaxDistance, frac));
					}
				}

				// Draw blue spheres for volume distances
				if (fade == true)
				{
					for (var i = 0; i <= 50; i++)
					{
						var frac = i * 0.02f;

						Gizmos.color = new Color(0.0f, 0.0f, 1.0f, blendCurve.Evaluate(frac));

						Gizmos.DrawWireSphere(transform.position, Mathf.Lerp(fadeMinDistance, fadeMaxDistance, frac));
					}
				}
			}
		}
#endif
	}
}

#if UNITY_EDITOR
namespace VolumetricAudio
{
	using UnityEditor;
	using TARGET = VA_AudioSource;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET), true)]
	public class VA_AudioSource_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("position", "Should this sound have its position update?");
		
			if (Any(tgts, t => t.Position == true))
			{
				BeginIndent();
					BeginError(Any(tgts, t => t.PositionDamping < 0.0f));
						Draw("positionDamping", "The speed at which the sound position changes.\n\n0 = Instant.");
					EndError();
					BeginError(Any(tgts, t => t.Shapes.Count == 0 || t.Shapes.Exists(s => s == null)));
						Draw("shapes", "The shapes you want the sound to emit from (e.g. VA_Mesh).");
					EndError();
					BeginError(Any(tgts, t => t.ExcludedShapes.Exists(s => s == null)));
						Draw("excludedShapes", "The shapes you want the audio source to be excluded from.");
					EndError();
				EndIndent();
			}

			Separator();

			Draw("blend", "Should this sound have its Spatial Blend update?");

			if (Any(tgts, t => t.Blend == true))
			{
				BeginIndent();
					BeginError(Any(tgts, t => t.BlendMinDistance < 0.0f || t.BlendMinDistance > t.BlendMaxDistance));
						Draw("blendMinDistance", "The distance at which the sound becomes fully mono.");
					EndError();
					BeginError(Any(tgts, t => t.BlendMaxDistance < 0.0f || t.BlendMinDistance > t.BlendMaxDistance));
						Draw("blendMaxDistance", "The distance at which the sound becomes fully stereo.");
					EndError();
					Draw("blendCurve", "The distribution of the mono to stereo ratio.");
				EndIndent();
			}

			Separator();

			Draw("volume", "Should this sound have its volume update?");

			if (Any(tgts, t => t.Volume == true))
			{
				BeginIndent();
					Draw("baseVolume", "The base volume of the audio source.");
					BeginDisabled();
						Draw("zone", "The zone this sound is associated with.");
					EndDisabled();

					Separator();

					Draw("fade", "Should the volume fade based on distance?");

					if (Any(tgts, t => t.Fade == true))
					{
						BeginIndent();
							BeginError(Any(tgts, t => t.FadeMinDistance < 0.0f || t.FadeMinDistance > t.FadeMaxDistance));
								Draw("fadeMinDistance", "The distance at which the sound fades to maximum volume.");
							EndError();
							BeginError(Any(tgts, t => t.FadeMaxDistance < 0.0f || t.FadeMinDistance > t.FadeMaxDistance));
								Draw("fadeMaxDistance", "The distance at which the sound fades to minimum volume.");
							EndError();
							Draw("fadeCurve", "The distribution of volume based on its scaled distance.");
							Draw("fadeWhizz", "Should the volume fade cap to the closest distance to simulate a bullet whizzing past?");
							if (Any(tgts, t => t.FadeWhizz == true))
							{
								BeginIndent();
									Draw("fadeWhizzClosestDistance", "The sound volume will be calculated based on this distance rather than the actual distance. If the actual distance is lower than this, then this value will be reduced to match.");
								EndIndent();
							}
						EndIndent();
					}

					Separator();

					Draw("occlude", "Should this sound be blocked when behind other objects?");

					if (Any(tgts, t => t.Occlude == true))
					{
						BeginIndent();
							Draw("occludeMethod", "The raycast style against the occlusion groups.");
							Draw("occludeMaterial", "Check for VA_Material instances attached to the occlusion object?");
							BeginError(Any(tgts, t => t.OccludeDamping <= 0.0f));
								Draw("occludeDamping", "How quickly the sound fades in/out when behind an object.");
							EndError();
							BeginError(Any(tgts, t => t.OccludeGroups == null || t.OccludeGroups.Count == 0));
								Draw("occludeGroups", "The amount of occlusion checks.");
							EndError();
						EndIndent();
					}
				EndIndent();
			}

			Separator();

			Draw("distant", "Should this sound's dynamics change based on its distance to the camera?");

			if (Any(tgts, t => t.Distant == true))
			{
				BeginIndent();
					Draw("distantMinDistance", "The camera distance where the minimum sound dynamics will be used.");
					Draw("distantMaxDistance", "The camera distance where the maximum sound dynamics will be used.");
					Draw("distantMinDynamics", "The sound dynamics used when the camera is at or below the minimum distance.");
					Draw("distantMaxDynamics", "The sound dynamics used when the camera is at or above the minimum distance.");
				EndIndent();
			}

			if (Any(tgts, t => IsSoundWrong(t)))
			{
				Warning("This sound's Spatial Blend isn't set to 3D, which is required if you're not using the Volume or Blend settings.");
			}
		}

		private bool IsSoundWrong(VA_AudioSource a)
		{
			if (a.Fade == false && a.Blend == false)
			{
				var s = a.GetComponent<AudioSource>();

				if (s != null && s.spatialBlend != 1.0f)
				{
					return true;
				}
			}

			return false;
		}
	}
}
#endif