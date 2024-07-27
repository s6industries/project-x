using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DavidJalbert.TinyCarControllerAdvance
{
    public class ExampleGui : MonoBehaviour
    {
        public static ExampleGui instance;
        public static List<string> additionalInfo = new List<string>();

        [System.Serializable]
        public class CarType
        {
            public TCCAPlayer carObject;
            public string description;
        }

        public Text textDebug;
        public Text textDescription;
        public CarType[] carTypes;
        public TCCACamera carCamera;
        public TCCAMobileInput mobileInput;
        public TCCAStandardInput standardInput;

        private int carIndex = 0;
        private TCCAPlayer carController;

        private void Start()
        {
            instance = this;
            changeCar(0);
        }

        void LateUpdate()
        {
            if (textDebug == null) return;

            textDebug.text = "";

            if (carController != null)
            {
                textDebug.text += "Car : " + carTypes[carIndex].description + "\n";
                textDebug.text += "Speed : " + (int)carController.getForwardVelocity() + " m/s\n";
                textDebug.text += "Drift speed : " + (int)carController.getLateralVelocity() + " m/s\n";
                textDebug.text += "Is grounded : " + (carController.isFullyGrounded() ? "true" : (carController.isGrounded() ? "partial" : "false")) + "\n";
                textDebug.text += "Pitch : " + (int)(carController.getPitchAngle()) + " degrees\n";
                textDebug.text += "Roll : " + (int)(carController.getRollAngle()) + " degrees\n";

                foreach (string line in additionalInfo)
                {
                    textDebug.text += line + "\n";
                }

                additionalInfo.Clear();
            }
        }

        public static void addInfo(string line)
        {
            additionalInfo.Add(line);
        }

        public static bool isCarSelected(TCCAPlayer p)
        {
            return instance.carController == p;
        }

        private void changeCar(int i)
        {
            if (carController != null)
            {
                carController.setAccelerationMultiplier(1);
                carController.setSpeedMultiplier(1);
                carController.setHandbrake(false);
                carController.setMotor(0);
                carController.setSteering(0);
            }

            carIndex = i % carTypes.Length;
            carController = carTypes[carIndex].carObject;

            if (carCamera != null) carCamera.carController = carController;
            if (mobileInput != null) mobileInput.carController = carController;
            if (standardInput != null) standardInput.carController = carController;

            carCamera?.resetCamera();
        }

        public void onClickChangeCar()
        {
            changeCar(carIndex + 1);
        }

        public void onClickMobileInput()
        {
            if (mobileInput != null) mobileInput.gameObject.SetActive(!mobileInput.gameObject.activeSelf);
        }

        public void onClickCameraAngle()
        {
            if (carCamera != null)
            {
                carCamera.switchCameraMode();
            }
        }

        public void onClickDescriptionText()
        {
            if (textDebug != null) textDebug.enabled = !textDebug.enabled;
            if (textDescription != null) textDescription.enabled = !textDescription.enabled;
        }
    }
}