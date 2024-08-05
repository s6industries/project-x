using UnityEngine;
using CW.Common;

namespace VolumetricAudio.Examples
{
	/// <summary>This component allows you to create a bullet spawner that sprays bullets forward.</summary>
	[HelpURL(VA_Common.HelpUrlPrefix + "VA_BulletSpawner")]
	[AddComponentMenu(VA_Common.ComponentMenuPrefix + "Bullet Spawner")]
	public class VA_BulletSpawner : MonoBehaviour
	{
		/// <summary>The bullet prefab.</summary>
		public Rigidbody Prefab;

		/// <summary>The spray angle in degrees.</summary>
		public float Spray = 10.0f;

		/// <summary>The speed of the bullets in units per second.</summary>
		public float Speed = 10.0f;

		/// <summary>How long the bullet stays alive in seconds.</summary>
		public float Age = 5.0f;

		/// <summary>The time between each shot in seconds.</summary>
		public float Delay = 0.1f;

		[System.NonSerialized]
		private float cooldown;

		protected virtual void FixedUpdate()
		{
			if (Delay > 0.0f)
			{
				cooldown -= Time.deltaTime;

				while (cooldown < 0)
				{
					cooldown += Delay;

					Shoot();
				}
			}
		}

		private void Shoot()
		{
			if (Prefab != null)
			{
				var rotation  = transform.rotation * Quaternion.Euler(Random.Range(-Spray, Spray), Random.Range(-Spray, Spray), 0.0f);
				var clone     = Instantiate(Prefab, transform.position, rotation);

				clone.gameObject.SetActive(true);

				clone.velocity = clone.transform.forward * Speed;

				Destroy(clone.gameObject, Age);
			}
		}
	}
}

#if UNITY_EDITOR
namespace VolumetricAudio.Examples
{
	using UnityEditor;
	using TARGET = VA_BulletSpawner;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class VA_BulletSpawner_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.Prefab == null));
				Draw("Prefab", "The bullet prefab.");
			EndError();
			Draw("Spray", "The spray angle in degrees.");
			Draw("Speed", "The speed of the bullets in units per second.");
			BeginError(Any(tgts, t => t.Age <= 0.0f));
				Draw("Age", "How long the bullet stays alive in seconds.");
			EndError();
			BeginError(Any(tgts, t => t.Delay <= 0.0f));
				Draw("Delay", "The time between each shot in seconds.");
			EndError();
		}
	}
}
#endif