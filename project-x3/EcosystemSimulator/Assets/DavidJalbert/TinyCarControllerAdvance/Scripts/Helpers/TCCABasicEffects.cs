using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DavidJalbert.TinyCarControllerAdvance
{
    public class TCCABasicEffects : MonoBehaviour
    {
        public TCCAPlayer playerObject;
        [Header("Engine")]
        [Tooltip("Audio source for the engine.")]
        public AudioSource audioEngine;
        [Tooltip("Pitch to which to set the engine audio when not moving.")]
        public float minEnginePitch = 0.25f;
        [Tooltip("Pitch to which to set the engine audio when moving at full speed.")]
        public float maxEnginePitch = 1f;
        [Tooltip("How much to interpolate the pitch of the engine audio.")]
        public float enginePitchInterpolation = 10f;
        [Header("Braking and skid marks")]
        [Tooltip("Audio source for the tires screeching.")]
        public AudioSource audioScreeching;
        [Tooltip("How much to interpolate the volume of the tire audio.")]
        public float screechingVolumeInterpolation = 5;
        [Tooltip("Prefab for the skid mark. Must be a Trail Renderer that automatically destroys itself when not moving.")]
        public TrailRenderer skidMarkPrefab;
        [Tooltip("Prefab for the tire smoke. Must be a Particle System that loops indefinitely.")]
        public ParticleSystem smokePrefab;
        [Tooltip("Minimum velocity at which to enable the tire screeching effect.")]
        public float minVelocity = 1;
        [Tooltip("Minimum difference between the wheel spin and the actual velocity at which to enable the tire screeching effect. Equals to the highest velocity value over the lowest velocity value.")]
        public float spinVelocityDifference = 1.5f;
        [Tooltip("Minimum lateral delta value at which to enable the tire screeching effect. Equals to the difference between the lateral and forward velocities, between 0 and 1, where 0 is completely straight and 1 is completely sideways.")]
        public float minLateralDelta = 0.5f;

        private TCCAWheel[] wheels;
        private GameObject objectRoot;
        private TrailRenderer[] skidMarkObjects;
        private ParticleSystem[] smokeObjects;
        
        private float audioScreechingVolume = 0;
        private float audioScreechingInitialVolume = 0;
        private float audioEngineDelta = 0;

        private void Start()
        {
            objectRoot = new GameObject("skid marks");
            objectRoot.transform.SetParent(transform);
            
            if (skidMarkPrefab != null)
            {
                skidMarkObjects = new TrailRenderer[getWheels().Length];
            }

            if (smokePrefab != null)
            {
                smokeObjects = new ParticleSystem[getWheels().Length];
                for (int i = 0; i < getWheels().Length; i++)
                {
                    smokeObjects[i] = Instantiate(smokePrefab);
                    smokeObjects[i].transform.SetParent(objectRoot.transform);
                }
            }

            if (audioScreeching != null)
            {
                if (!audioScreeching.isPlaying) audioScreeching.Play();
                audioScreechingInitialVolume = audioScreeching.volume;
            }

            if (audioEngine != null && !audioEngine.isPlaying)
            {
                audioEngineDelta = 0;
                audioEngine.Play();
            }
        }

        private void OnEnable()
        {
            if (audioScreeching != null || !audioScreeching.isPlaying) audioScreeching.Play();
            if (audioEngine != null && !audioEngine.isPlaying) audioEngine.Play();
        }

        private void Update()
        {
            if (audioEngine != null)
            {
                audioEngineDelta = Mathf.Lerp(audioEngineDelta, Mathf.Abs(playerObject.getWheelsMaxSpin() / playerObject.getWheelsMaxSpeed(true)), Mathf.Clamp01(enginePitchInterpolation == 0 ? 1 : Time.deltaTime * enginePitchInterpolation));
                audioEngine.pitch = Mathf.LerpUnclamped(minEnginePitch, maxEnginePitch, audioEngineDelta);
                audioEngine.transform.position = playerObject.getRigidbody().transform.position;
            }

            float maxTireVolume = 0;
            for (int i = 0; i < getWheels().Length; i++)
            {
                TCCAWheel wheel = getWheels()[i];

                float forwardSpinVelocity = wheel.getForwardSpinVelocity();
                float forwardVelocity = wheel.getForwardVelocity();
                float maxForwardVelocity = Mathf.Max(Mathf.Abs(forwardSpinVelocity), Mathf.Abs(forwardVelocity));
                float minForwardVelocity = Mathf.Min(Mathf.Abs(forwardSpinVelocity), Mathf.Abs(forwardVelocity));
                float sideVelocity = Mathf.Abs(wheel.getLateralVelocity());
                float sideDelta = Mathf.Abs((new Vector2(wheel.getLateralVelocity(), wheel.getForwardVelocity())).normalized.x);

                bool braking = false;
                if (minForwardVelocity < minVelocity)
                {
                    braking = maxForwardVelocity >= minVelocity;
                }
                else
                {
                    braking = maxForwardVelocity / minForwardVelocity >= spinVelocityDifference;
                }
                bool sliding = minForwardVelocity >= minVelocity && sideDelta >= minLateralDelta;
                bool skidding = wheel.isTouchingGround() && (sliding || braking);

                if (skidding)
                {
                    if (skidMarkObjects != null)
                    {
                        if (skidMarkObjects[i] == null)
                        {
                            skidMarkObjects[i] = Instantiate(skidMarkPrefab);
                            skidMarkObjects[i].transform.SetParent(objectRoot.transform);
                        }
                        skidMarkObjects[i].transform.position = wheel.getPosition() + Vector3.down * wheel.getColliderRadius() * 0.9f;
                    }

                    if (smokeObjects != null && !smokeObjects[i].isPlaying) smokeObjects[i].Play();

                    maxTireVolume = Mathf.Max(maxTireVolume, 1);
                }
                else
                {
                    if (skidMarkObjects != null)
                    {
                        if (skidMarkObjects[i] != null)
                        {
                            skidMarkObjects[i] = null;
                        }
                    }
                    if (smokeObjects != null && smokeObjects[i].isPlaying) smokeObjects[i].Stop();

                    maxTireVolume = Mathf.Max(maxTireVolume, 0);
                }

                if (smokeObjects != null) smokeObjects[i].transform.position = wheel.getPosition() + Vector3.down * wheel.getColliderRadius() * 0.9f;
            }

            audioScreechingVolume = Mathf.Lerp(audioScreechingVolume, maxTireVolume, Mathf.Clamp01(Time.deltaTime * screechingVolumeInterpolation));
            if (audioScreeching != null)
            {
                audioScreeching.volume = audioScreechingInitialVolume * audioScreechingVolume;
                audioScreeching.transform.position = playerObject.getRigidbody().transform.position;
            }
        }

        public TCCAPlayer getPlayer()
        {
            if (playerObject == null) playerObject = GetComponentInChildren<TCCAPlayer>();
            return playerObject;
        }

        public TCCAWheel[] getWheels()
        {
            if (wheels == null) wheels = playerObject.GetComponentsInChildren<TCCAWheel>();
            return wheels;
        }
    }
}