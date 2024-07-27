using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DavidJalbert.TinyCarControllerAdvance
{
    public class TCCABody : MonoBehaviour
    {
        public enum CounterMode
        {
            Never, Always, InAir, FullyGrounded, PartiallyGrounded, PartiallyOrFullyGrounded
        }

        [Header("Body parameters")]
        [Tooltip("The mass that will be applied to the body.")]
        public float bodyMass = 10;
        [Tooltip("Whether to apply interpolation to the body.")]
        public RigidbodyInterpolation rigidbodyInterpolation;
        [Tooltip("Which collision detection mode to use on the body.")]
        public CollisionDetectionMode collisionDetectionMode;
        [Tooltip("The center of mass of the body in local space. Ideally this should be the center of the car at ground level. Change the Z value to make the car lean backward or forward when in the air.")]
        public Vector3 centerOfMass = new Vector3(0, 0, 0);

        [Header("Roll countering")]
        [Tooltip("When to apply roll countering force.")]
        public CounterMode rollCounterMode = CounterMode.Always;
        [Tooltip("The angle in degrees to which to rotate the vehicle. Set to 0 to roll perfectly upright.")]
        public float rollCounterTargetAngle = 0;
        [Tooltip("How much force to apply to rotate the vehicle upright if it rolls over.")]
        public float rollCounterForce = 1;
        [Tooltip("How fast to rotate the vehicle upright if it rolls over. Set to zero to make this instantaneous.")]
        public float rollCounterSmoothing = 5;
        [Tooltip("How much force, between 0 (none) and 1 (max), to apply relative to the vehicle's speed, between 0 (stationary) and 1 (max speed).")]
        public AnimationCurve rollCounterOverSpeed = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));

        [Header("Pitch countering")]
        [Tooltip("When to apply pitch countering force.")]
        public CounterMode pitchCounterMode = CounterMode.InAir;
        [Tooltip("The angle in degrees to which to rotate the vehicle. Set to 0 to level perfectly straight.")]
        public float pitchCounterTargetAngle = 0;
        [Tooltip("How much force to apply to level the vehicle.")]
        public float pitchCounterForce = 1;
        [Tooltip("How fast to level the vehicle. Set to zero to make this instantaneous.")]
        public float pitchCounterSmoothing = 5;
        [Tooltip("How much force, between 0 (none) and 1 (max), to apply relative to the vehicle's speed, between 0 (stationary) and 1 (max speed).")]
        public AnimationCurve pitchCounterOverSpeed = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));

        [Header("Steering stabilizer")]
        [Tooltip("When to apply steering stabilizing force.")]
        public CounterMode steeringStabilizerMode = CounterMode.PartiallyOrFullyGrounded;
        [Tooltip("How much force to apply to straighten the vehicle.")]
        public float steeringStabilizerForce = 1;
        [Tooltip("How fast to straighten the vehicle. Set to zero to make this instantaneous.")]
        public float steeringStabilizerSmoothing = 0;
        [Tooltip("How much force, between 0 (none) and 1 (max), to apply relative to the vehicle's speed, between 0 (stationary) and 1 (max speed).")]
        public AnimationCurve steeringStabilizerOverSpeed = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.2f, 1), new Keyframe(1, 1));

        private Rigidbody carBody;
        private bool wasInitialized = false;
        private TCCAPlayer parentPlayer = null;
        private TCCABodyCollider internalCollider = null;
        private float inputSteering = 0;
        private RigidbodyConstraints constraints = 0;
        private float maxSteeringAngle = 0;
        private Vector3 massOffset = Vector3.zero;

        public virtual void onCollisionStay(Collision collision) { }
        public virtual void onCollisionEnter(Collision collision) { }
        public virtual void onCollisionExit(Collision collision) { }
        public virtual void onTriggerStay(Collider other) { }
        public virtual void onTriggerEnter(Collider other) { }
        public virtual void onTriggerExit(Collider other) { }

        void FixedUpdate()
        {
            if (!wasInitialized) return;

            refresh(Time.fixedDeltaTime);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.TransformPoint(centerOfMass));
            Gizmos.DrawWireSphere(transform.TransformPoint(centerOfMass), 0.1f);
        }

        public Rigidbody getRigidbody()
        {
            if (carBody == null) carBody = GetComponentInChildren<Rigidbody>();
            return carBody;
        }

        public void setConstraints(RigidbodyConstraints c)
        {
            constraints = c;
        }

        public void initialize(TCCAPlayer parent)
        {
            parentPlayer = parent;

            if (getRigidbody() == null)
            {
                carBody = gameObject.AddComponent<Rigidbody>();
            }

            carBody.centerOfMass = centerOfMass;
            carBody.mass = bodyMass;
            carBody.drag = 0;
            carBody.angularDrag = 0;
            carBody.useGravity = true;
            carBody.isKinematic = false;
            carBody.interpolation = rigidbodyInterpolation;
            carBody.collisionDetectionMode = collisionDetectionMode;

            internalCollider = gameObject.AddComponent<TCCABodyCollider>();
            internalCollider.initialize(this);

            wasInitialized = true;
        }

        public void refresh(float deltaTime)
        {
            if (!wasInitialized) return;

            carBody.centerOfMass = centerOfMass + massOffset;
            carBody.mass = bodyMass;
            carBody.angularDrag = 0;
            carBody.interpolation = rigidbodyInterpolation;
            carBody.collisionDetectionMode = collisionDetectionMode;
            carBody.constraints = constraints;

            Vector3 localAngular = getForwardAngularVelocity();

            if (rollCounterForce != 0 && canCounterRotation(rollCounterMode))
            {
                float rollForceDelta = rollCounterOverSpeed.Evaluate(getParentPlayer().getForwardVelocityDelta());
                float maxRotation = Mathf.DeltaAngle(getRollAngle(), rollCounterTargetAngle);
                float smoothDelta = Mathf.Clamp01(rollCounterSmoothing == 0 ? 1 : rollCounterSmoothing * deltaTime);
                float counterVelocity = smoothDelta * Mathf.Sign(maxRotation) * Mathf.Min(Mathf.Abs(maxRotation), rollCounterForce) * rollForceDelta;

                localAngular.z = Mathf.Lerp(localAngular.z, counterVelocity, smoothDelta);
            }

            if (pitchCounterForce != 0 && canCounterRotation(pitchCounterMode))
            {
                float pitchForceDelta = pitchCounterOverSpeed.Evaluate(getParentPlayer().getForwardVelocityDelta());
                float maxRotation = Mathf.DeltaAngle(getPitchAngle(), pitchCounterTargetAngle);
                float smoothDelta = Mathf.Clamp01(pitchCounterSmoothing == 0 ? 1 : pitchCounterSmoothing * deltaTime);
                float counterVelocity = smoothDelta * Mathf.Sign(maxRotation) * Mathf.Min(Mathf.Abs(maxRotation), pitchCounterForce) * pitchForceDelta;

                localAngular.x = Mathf.Lerp(localAngular.x, counterVelocity, smoothDelta);
            }

            if (steeringStabilizerForce != 0 && canCounterRotation(steeringStabilizerMode))
            {
                float steeringTargetAngle = inputSteering * maxSteeringAngle;
                float steeringForceDelta = steeringStabilizerOverSpeed.Evaluate(getParentPlayer().getForwardVelocityDelta());
                float targetVelocity = steeringTargetAngle * deltaTime * steeringStabilizerForce;
                float smoothDelta = Mathf.Clamp01(steeringStabilizerSmoothing == 0 ? 1 : steeringStabilizerSmoothing * deltaTime);
                localAngular.y = Mathf.Lerp(localAngular.y, targetVelocity, smoothDelta * steeringForceDelta);
            }

            setForwardAngularVelocity(localAngular);
        }

        public void setSteering(float steering, float maxAngle)
        {
            inputSteering = steering;
            maxSteeringAngle = maxAngle;
        }

        public float getPitchAngle()
        {
            return Mathf.DeltaAngle(90, Vector3.Angle(carBody.transform.forward, Vector3.up));
        }

        public float getRollAngle()
        {
            return Mathf.DeltaAngle(90, Vector3.Angle(carBody.transform.right, Vector3.down));
        }

        public bool canCounterRotation(CounterMode m)
        {
            bool canCounter = false;
            switch (m)
            {
                case CounterMode.Never: canCounter = false; break;
                case CounterMode.Always: canCounter = true; break;
                case CounterMode.InAir: canCounter = !getParentPlayer().isGrounded(); break;
                case CounterMode.FullyGrounded: canCounter = getParentPlayer().isFullyGrounded(); break;
                case CounterMode.PartiallyGrounded: canCounter = getParentPlayer().isPartiallyGrounded(); break;
                case CounterMode.PartiallyOrFullyGrounded: canCounter = getParentPlayer().isGrounded(); break;
            }
            return canCounter;
        }

        public Vector3 getForwardAngularVelocity()
        {
            return carBody.transform.InverseTransformDirection(carBody.angularVelocity);
        }

        public void setForwardAngularVelocity(Vector3 v)
        {
            carBody.angularVelocity = carBody.transform.TransformDirection(v);
        }

        public float getForwardVelocity()
        {
            return Vector3.Dot(carBody.velocity, carBody.transform.forward);
        }

        public float getLateralVelocity()
        {
            return Vector3.Dot(carBody.velocity, carBody.transform.right);
        }

        public float getVerticalVelocity()
        {
            return Vector3.Dot(carBody.velocity, carBody.transform.up);
        }

        public TCCAPlayer getParentPlayer()
        {
            return parentPlayer;
        }

        public Vector3 getPosition()
        {
            return carBody.position;
        }

        public Quaternion getRotation()
        {
            return carBody.rotation;
        }

        public void setPosition(Vector3 position)
        {
            carBody.position = position;
        }

        public void setRotation(Quaternion rotation)
        {
            carBody.rotation = rotation;
        }

        public void translate(Vector3 offset)
        {
            setPosition(getPosition() + offset);
        }

        public void rotate(Quaternion rotation)
        {
            setRotation(getRotation() * rotation);
        }

        public void immobilize()
        {
            carBody.velocity = Vector3.zero;
            carBody.angularVelocity = Vector3.zero;
        }

        public void setParent(Transform parent)
        {
            carBody.transform.SetParent(parent, true);
        }
    }
}