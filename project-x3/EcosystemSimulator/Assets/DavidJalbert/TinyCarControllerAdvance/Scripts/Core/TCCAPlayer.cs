using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DavidJalbert.TinyCarControllerAdvance
{
    public class TCCAPlayer : MonoBehaviour
    {
        private TCCABody carBody;
        private TCCAWheel[] wheels;
        private GameObject tempContainer;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Transform currentParent;

        [Header("Behavior")]
        [Tooltip("How much torque to apply to the wheels. 1 is full speed forward, -1 is full speed backward, 0 is rest.")]
        public float motorDelta = 0;
        [Tooltip("How much steering to apply to the wheels. 1 is right, -1 is left, 0 is straight.")]
        public float steeringDelta = 0;
        [Tooltip("How much boost to apply to the wheels. 1 is full boost, 0 is no boost.")]
        public float boostDelta = 0;
        [Tooltip("Whether to apply the handbrake to the wheels.")]
        public bool applyHandbrake = false;

        [Header("Speed boost")]
        [Tooltip("Speed multiplier to apply when using the boost.")]
        public float boostMaxSpeedMultiplier = 2;
        [Tooltip("Acceleration multiplier to apply when using the boost.")]
        public float boostAccelerationMultiplier = 2;

        [Header("Position Constraints")]
        public bool freezePositionX;
        public bool freezePositionY;
        public bool freezePositionZ;

        void Awake()
        {
            carBody = GetComponentInChildren<TCCABody>();
            carBody.initialize(this);

            wheels = GetComponentsInChildren<TCCAWheel>();
            foreach (TCCAWheel wheel in wheels)
            {
                wheel.initialize(this);
            }

            tempContainer = new GameObject("temp " + gameObject.name);
            tempContainer.transform.SetParent(transform.parent);
            tempContainer.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }

        private void Update()
        {
            RigidbodyConstraints constraints = 0;
            if (freezePositionX) constraints |= RigidbodyConstraints.FreezePositionX;
            if (freezePositionY) constraints |= RigidbodyConstraints.FreezePositionY;
            if (freezePositionZ) constraints |= RigidbodyConstraints.FreezePositionZ;

            carBody.setConstraints(constraints);
            foreach (TCCAWheel wheel in wheels)
            {
                wheel.setConstraints(constraints);
            }
        }

        private void FixedUpdate()
        {
            float averageSteeringAngle = 0;
            int steeringWheels = 0;

            foreach (TCCAWheel wheel in wheels)
            {
                wheel.setAccelerationMultiplier(boostAccelerationMultiplier);
                wheel.setSpeedMultiplier(boostMaxSpeedMultiplier);
                wheel.setMotor(motorDelta);
                wheel.setSteering(steeringDelta);
                wheel.setHandbrake(applyHandbrake);

                if (wheel.steeringEnabled)
                {
                    steeringWheels++;
                    averageSteeringAngle += wheel.steeringMaxAngle;
                }
            }

            carBody.setSteering(steeringDelta, averageSteeringAngle / steeringWheels);
        }

        public Rigidbody getRigidbody()
        {
            return getCarBody()?.getRigidbody() ?? null;
        }

        public float getForwardVelocity()
        {
            return getCarBody()?.getForwardVelocity() ?? 0;
        }

        public TCCABody getCarBody()
        {
            return carBody;
        }

        public void setMotor(float d)
        {
            motorDelta = d;
        }

        public void setSteering(float d)
        {
            steeringDelta = d;
        }

        public void setHandbrake(bool e)
        {
            applyHandbrake = e;
        }

        public void setSpeedMultiplier(float m)
        {
            boostMaxSpeedMultiplier = m;
        }

        public void setAccelerationMultiplier(float m)
        {
            boostAccelerationMultiplier = m;
        }

        public float getSpeedMultiplier()
        {
            return boostMaxSpeedMultiplier;
        }

        public float getAccelerationMultiplier()
        {
            return boostAccelerationMultiplier;
        }

        public float getWheelsMaxSpin(int direction = 0)
        {
            float maxSpin = 0;
            foreach (TCCAWheel w in wheels)
            {
                float spin = w.getForwardSpinVelocity();
                if ((direction == 0 && Mathf.Abs(spin) > Mathf.Abs(maxSpin)) || (direction == 1 && spin > maxSpin) || (direction == -1 && spin < maxSpin))
                {
                    maxSpin = spin;
                }
            }
            return maxSpin;
        }

        public float getWheelsMaxSpeed(bool ignoreReverse = false)
        {
            float maxSpeed = 0;
            foreach (TCCAWheel w in wheels)
            {
                if (w.getMaxSpeed(ignoreReverse) > maxSpeed) maxSpeed = w.getMaxSpeed(ignoreReverse);
            }
            return maxSpeed;
        }

        public float getPitchAngle()
        {
            return getCarBody()?.getPitchAngle() ?? 0;
        }

        public float getRollAngle()
        {
            return getCarBody()?.getRollAngle() ?? 0;
        }

        public float getForwardVelocityDelta()
        {
            if (getWheelsMaxSpeed() == 0) return 0;
            return getForwardVelocity() / getWheelsMaxSpeed();
        }

        public float getLateralVelocity()
        {
            return getCarBody()?.getLateralVelocity() ?? 0;
        }

        public float getVerticalVelocity()
        {
            return getCarBody()?.getVerticalVelocity() ?? 0;
        }

        public bool isPartiallyGrounded()
        {
            return isGrounded() && !isFullyGrounded();
        }

        public bool isGrounded()
        {
            foreach (TCCAWheel w in wheels)
            {
                if (w.isTouchingGround()) return true;
            }
            return false;
        }

        public bool isFullyGrounded()
        {
            foreach (TCCAWheel w in wheels)
            {
                if (!w.isTouchingGround()) return false;
            }
            return true;
        }

        public void immobilize()
        {
            foreach (TCCAWheel w in wheels)
            {
                w.immobilize();
            }
            carBody.immobilize();
        }

        public void translate(Vector3 position)
        {
            setPosition(getPosition() + position);
        }

        public void rotate(Quaternion rotation)
        {
            setRotation(getRotation() * rotation);
        }

        public void recenter()
        {
            tempContainer.transform.position = carBody.getPosition();
            tempContainer.transform.rotation = carBody.getRotation();

            setParent(tempContainer.transform);

            transform.position = tempContainer.transform.position;
            transform.rotation = tempContainer.transform.rotation;

            setParent(transform);
        }

        public void setPosition(Vector3 position)
        {
            recenter();
            transform.position = position;
            recenter();
        }

        public void setRotation(Quaternion rotation)
        {
            recenter();
            transform.rotation = rotation;
            recenter();
        }

        public Vector3 getPosition()
        {
            return carBody.getPosition();
        }

        public Quaternion getRotation()
        {
            return carBody.getRotation();
        }

        public Vector3 getInitialPosition()
        {
            return initialPosition;
        }

        public Quaternion getInitialRotation()
        {
            return initialRotation;
        }

        protected void setParent(Transform parent)
        {
            currentParent = parent;
            carBody.setParent(currentParent);
            foreach (TCCAWheel w in wheels)
            {
                w.setParent(currentParent);
            }
        }
    }
}