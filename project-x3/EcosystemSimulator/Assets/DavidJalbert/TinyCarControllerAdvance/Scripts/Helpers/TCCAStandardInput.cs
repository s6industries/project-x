using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DavidJalbert.TinyCarControllerAdvance
{
    public class TCCAStandardInput : MonoBehaviour
    {
        public enum InputType
        {
            None, Axis, RawAxis, Key, Button
        }

        [System.Serializable]
        public struct InputValue
        {
            [Tooltip("Type of input.")]
            public InputType type;
            [Tooltip("Name of the input entry.")]
            public string name;
            [Tooltip("Returns the negative value when using an axis type.")]
            public bool invert;
        }

        public TCCAPlayer carController;

        [Header("Input")]
        [Tooltip("Whether to let this script control the vehicle.")]
        public bool enableInput = true;
        [Tooltip("Input type to check to make the vehicle move forward.")]
        public InputValue forwardInput = new InputValue() { type = InputType.RawAxis, name = "Vertical", invert = false };
        [Tooltip("Input type to check to make the vehicle move backward.")]
        public InputValue reverseInput = new InputValue() { type = InputType.RawAxis, name = "Vertical", invert = true };
        [Tooltip("Input type to check to make the vehicle turn right.")]
        public InputValue steerRightInput = new InputValue() { type = InputType.RawAxis, name = "Horizontal", invert = false };
        [Tooltip("Input type to check to make the vehicle turn left.")]
        public InputValue steerLeftInput = new InputValue() { type = InputType.RawAxis, name = "Horizontal", invert = true };
        [Tooltip("Input type to check to apply the handbrake.")]
        public InputValue handbrakeInput = new InputValue() { type = InputType.Key, name = "space", invert = false };
        [Tooltip("Input type to check to give the vehicle a speed boost.")]
        public InputValue boostInput = new InputValue() { type = InputType.Key, name = "left shift", invert = false };
        [Tooltip("Input type to reset the vehicle to its original position.")]
        public InputValue respawnInput = new InputValue() { type = InputType.Key, name = "r", invert = false };
        [Header("Speed boost")]
        [Tooltip("Acceleration multiplier to apply when using the boost.")]
        public float accelerationBoostMultiplier = 2;
        [Tooltip("Max speed multiplier to apply when using the boost.")]
        public float speedBoostMultiplier = 2;

        private float respawnPreviousValue = 0;

        void Update()
        {
            if (enableInput && carController != null)
            {
                float motorDelta = getInput(forwardInput) - getInput(reverseInput);
                float steeringDelta = getInput(steerRightInput) - getInput(steerLeftInput);
                bool handbrake = getInput(handbrakeInput) >= 0.5f;
                float boostDelta = Mathf.Abs(getInput(boostInput));
                float respawnValue = getInput(respawnInput);
                bool respawn = respawnValue >= 0.5f && respawnPreviousValue < 0.5f;
                respawnPreviousValue = respawnValue;

                if (respawn)
                {
                    carController.immobilize();
                    carController.setPosition(carController.getInitialPosition());
                    carController.setRotation(carController.getInitialRotation());

                    foreach (TrailRenderer t in carController.GetComponentsInChildren<TrailRenderer>())
                    {
                        t.Clear();
                    }
                }

                carController.setMotor(motorDelta);
                carController.setSteering(steeringDelta);
                carController.setHandbrake(handbrake);
                carController.setAccelerationMultiplier(Mathf.Lerp(1, accelerationBoostMultiplier, Mathf.Abs(getInput(boostInput))));
                carController.setSpeedMultiplier(Mathf.Lerp(1, speedBoostMultiplier, Mathf.Abs(getInput(boostInput))));
            }
        }

        public float getInput(InputValue v)
        {
            float value = 0;
            switch (v.type)
            {
                case InputType.Axis: value = Input.GetAxis(v.name); break;
                case InputType.RawAxis: value = Input.GetAxisRaw(v.name); break;
                case InputType.Key: value = Input.GetKey(v.name) ? 1 : 0; break;
                case InputType.Button: value = Input.GetButton(v.name) ? 1 : 0; break;
            }
            if (v.invert) value *= -1;
            return Mathf.Clamp01(value);
        }
    }
}