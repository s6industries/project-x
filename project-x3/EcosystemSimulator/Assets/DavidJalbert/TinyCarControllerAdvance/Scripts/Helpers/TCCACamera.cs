using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DavidJalbert.TinyCarControllerAdvance
{
    public class TCCACamera : MonoBehaviour
    {
        public enum CameraMode
        {
            TopDown = 0, ThirdPerson = 1
        }

        public static int TotalCameraModes = Enum.GetNames(typeof(CameraMode)).Length;

        [Tooltip("Which player object the camera should track.")]
        public TCCAPlayer carController;
        [Tooltip("Top Down: Only change the camera's position, keep rotation fixed.\nThird Person: Change both the position and rotation relative to the vehicle.")]
        public CameraMode viewMode = CameraMode.TopDown;

        [Header("Top Down parameters")]
        [Tooltip("Distance of the camera from the target.")]
        public float topDownDistance = 8;
        [Tooltip("Rotation of the camera.")]
        public Vector3 topDownAngle = new Vector3(45, 0, 0);
        [Tooltip("Speed at which the camera will move to its target")]
        public float topDownSpeed = 50;

        [Header("Third Person parameters")]
        [Tooltip("Position of the target relative to the car.")]
        public Vector3 thirdPersonOffsetStart = new Vector3(0, 0.5f, 0);
        [Tooltip("Position of the camera relative to the car.")]
        public Vector3 thirdPersonOffsetEnd = new Vector3(0, 1, -3);
        [Tooltip("Rotation of the camera relative to the target.")]
        public Vector3 thirdPersonAngle = new Vector3(10, 0, 0);
        [Tooltip("The minimum distance to keep when an obstacle is in the way of the camera.")]
        public float thirdPersonSkinWidth = 0.1f;
        [Tooltip("Lowers the camera's rotation if the velocity of the rigidbody is below this value. Set to 0 to disable.")]
        public float interpolationUpToSpeed = 50;
        [Tooltip("Speed at which the camera will move to its target")]
        public float thirdPersonPositionSpeed = 50;
        [Tooltip("Speed at which the camera will rotate to its target")]
        public float thirdPersonRotationSpeed = 50;

        private Rigidbody cameraBody;

        private void Start()
        {
            cameraBody = GetComponentInChildren<Rigidbody>();
            if (cameraBody == null)
            {
                cameraBody = gameObject.AddComponent<Rigidbody>();
                cameraBody.hideFlags = HideFlags.NotEditable;
            }

            cameraBody.isKinematic = false;
            cameraBody.useGravity = false;
            cameraBody.drag = 0;
            cameraBody.angularDrag = 0;
        }

        void FixedUpdate()
        {
            if (carController == null) return;

            cameraBody.interpolation = carController.getRigidbody().interpolation;

            Vector3 previousPosition = transform.position;
            Quaternion previousRotation = transform.rotation;

            getTargetTransforms(out Vector3 targetPosition, out Quaternion targetRotation);

            float lerpPositionSpeed = 0;
            float lerpRotationSpeed = 0;

            switch (viewMode)
            {
                case CameraMode.ThirdPerson:
                    lerpPositionSpeed = thirdPersonPositionSpeed;
                    lerpRotationSpeed = thirdPersonRotationSpeed;
                    break;

                case CameraMode.TopDown:
                    lerpPositionSpeed = topDownSpeed;
                    lerpRotationSpeed = topDownSpeed;
                    break;
            }

            lerpPositionSpeed = Mathf.Clamp(lerpPositionSpeed, 0, 1f / Time.fixedDeltaTime);
            lerpRotationSpeed = Mathf.Clamp(lerpRotationSpeed, 0, 1f / Time.fixedDeltaTime);
            Quaternion rotationDifference = getShortestRotation(targetRotation, previousRotation);

            float angleInDegrees;
            Vector3 rotationAxis;
            rotationDifference.ToAngleAxis(out angleInDegrees, out rotationAxis);

            Vector3 angularDisplacement = rotationAxis * angleInDegrees * Mathf.Deg2Rad;

            cameraBody.velocity = (targetPosition - previousPosition) * lerpPositionSpeed;
            cameraBody.angularVelocity = angularDisplacement * lerpRotationSpeed;
        }

        private void getTargetTransforms(out Vector3 targetPosition, out Quaternion targetRotation, bool ignoreSpeed = false)
        {
            Vector3 followPosition = carController.getRigidbody().transform.position;
            Quaternion followRotation = carController.getRigidbody().transform.rotation;

            targetPosition = transform.position;
            targetRotation = transform.rotation;

            switch (viewMode)
            {
                case CameraMode.ThirdPerson:
                    float interpolationMultiplier = 1;

                    Rigidbody body = carController.getRigidbody();
                    if (body != null)
                    {
                        float forwardVelocity = carController.getForwardVelocity();
                        interpolationMultiplier = interpolationUpToSpeed > 0 && !ignoreSpeed ? Mathf.Clamp01(Mathf.Abs(forwardVelocity) / interpolationUpToSpeed) : 1f;
                        Vector3 rotationDirection = Vector3.Lerp(body.transform.forward, body.velocity.normalized, Mathf.Clamp01(forwardVelocity));
                        followRotation = Quaternion.LookRotation(rotationDirection, Vector3.back);
                    }

                    Vector3 rotationEuler = thirdPersonAngle + Vector3.up * followRotation.eulerAngles.y;

                    Quaternion xzRotation = Quaternion.Euler(new Vector3(rotationEuler.x, targetRotation.eulerAngles.y, rotationEuler.z));
                    Quaternion lerpedRotation = Quaternion.Euler(rotationEuler);
                    targetRotation = Quaternion.Lerp(xzRotation, lerpedRotation, interpolationMultiplier);

                    Vector3 forwardDirection = targetRotation * Vector3.forward;
                    Vector3 rightDirection = targetRotation * Vector3.right;
                    Vector3 directionVector = forwardDirection * thirdPersonOffsetEnd.z + Vector3.up * thirdPersonOffsetEnd.y + rightDirection * thirdPersonOffsetEnd.x;

                    Vector3 directionVectorNormal = directionVector.normalized;
                    float directionMagnitude = directionVector.magnitude;

                    Vector3 cameraWorldDirection = directionVectorNormal;
                    Vector3 startCast = followPosition + thirdPersonOffsetStart;
                    RaycastHit[] hits = Physics.RaycastAll(startCast, cameraWorldDirection, directionMagnitude);
                    float hitDistance = -1;
                    foreach (RaycastHit hit in hits)
                    {
                        if (!isChildOf(hit.transform, carController.transform)) hitDistance = hitDistance >= 0 ? Mathf.Min(hitDistance, hit.distance) : hit.distance;
                    }
                    if (hitDistance >= 0)
                    {
                        targetPosition = followPosition + thirdPersonOffsetStart + directionVectorNormal * Mathf.Max(thirdPersonSkinWidth, hitDistance - thirdPersonSkinWidth);
                    }
                    else
                    {
                        targetPosition = directionVector + thirdPersonOffsetStart + followPosition;
                    }
                    break;

                case CameraMode.TopDown:
                    targetRotation = Quaternion.Euler(topDownAngle);
                    targetPosition = followPosition + targetRotation * Vector3.back * topDownDistance;
                    break;
            }
        }

        private bool isChildOf(Transform source, Transform target)
        {
            Transform child = source;
            while (child != null)
            {
                if (child == target) return true;
                child = child.parent;
            }
            return false;
        }

        private static Quaternion getShortestRotation(Quaternion a, Quaternion b)
        {
            if (Quaternion.Dot(a, b) < 0)
            {
                return a * Quaternion.Inverse(multiplyQuaternion(b, -1));
            }
            else return a * Quaternion.Inverse(b);
        }

        private static Quaternion multiplyQuaternion(Quaternion input, float scalar)
        {
            return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
        }

        public void resetCamera()
        {
            if (cameraBody == null) return;
            getTargetTransforms(out Vector3 targetPosition, out Quaternion targetRotation, true);
            cameraBody.position = targetPosition;
            cameraBody.rotation = targetRotation;
        }

        public void switchCameraMode()
        {
            switchCameraMode(-1);
        }

        public void switchCameraMode(int mode)
        {
            if (mode < 0) viewMode = (CameraMode)(((int)viewMode + 1) % TotalCameraModes);
            else viewMode = (CameraMode)mode;
            resetCamera();
        }
    }
}