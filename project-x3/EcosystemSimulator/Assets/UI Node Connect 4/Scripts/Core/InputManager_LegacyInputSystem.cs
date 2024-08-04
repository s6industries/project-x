using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MeadowGames.UINodeConnect4
{
    public class InputManager_LegacyInputSystem : InputManager
    {
        public KeyCode clickKey = KeyCode.Mouse0;
        public KeyCode aux0Key = KeyCode.LeftShift;
        public KeyCode deleteKey = KeyCode.Delete;

        public override Vector3 ScreenPointerPosition => Input.mousePosition;

        public override UnityEvent e_OnPointerDown { get; set; } = new UnityEvent();
        public override UnityEvent e_OnDrag { get; set; } = new UnityEvent();
        public override UnityEvent e_OnPointerUp { get; set; } = new UnityEvent();
        public override UnityEvent e_OnDelete { get; set; } = new UnityEvent();
        public override UnityEvent e_OnPointerHover { get; set; } = new UnityEvent();
        public override bool PointerPress => Input.GetKey(clickKey);
        public override bool Aux0KeyPress => Input.GetKey(aux0Key);

        Vector3 _initialMousePos;

        void OnEnable()
        {
            UICSystemManager.AddToUpdate(OnUpdate);
        }

        void OnDisable()
        {
            UICSystemManager.RemoveFromUpdate(OnUpdate);
        }

        public override void OnUpdate()
        {
            if (gameObject.activeInHierarchy)
            {
                if (Input.GetKeyDown(clickKey))
                {
                    _initialMousePos = ScreenPointerPosition;
                    OnPointerDown();
                }

                if (Input.GetKey(clickKey))
                {
                    if(Vector3.Distance(_initialMousePos, ScreenPointerPosition) > 0.1f)
                    {
                        OnDrag();
                    }
                }

                if (Input.GetKeyUp(clickKey))
                {
                    OnPointerUp();
                }

                if (Input.GetKeyDown(deleteKey))
                {
                    OnDeleteKeyPressed();
                }

                OnPointerHover();
            }
        }

        public void OnPointerDown()
        {
            e_OnPointerDown.Invoke();
        }

        public void OnDrag()
        {
            e_OnDrag.Invoke();
        }

        public void OnPointerUp()
        {
            e_OnPointerUp.Invoke();
        }

        public void OnDeleteKeyPressed()
        {
            e_OnDelete.Invoke();
        }

        public void OnPointerHover()
        {
            e_OnPointerHover.Invoke();
        }

        public override Vector3 GetCanvasPointerPosition(GraphManager graphManager)
        {
            if (graphManager.CanvasRenderMode == RenderMode.ScreenSpaceOverlay)
            {
                return ScreenPointerPosition;
            }
            else 
            {
                Camera mainCamera = graphManager.mainCamera;
                var screenPoint = ScreenPointerPosition;
                screenPoint.z = graphManager.transform.position.z - mainCamera.transform.position.z; //distance of the plane from the camera
                return mainCamera.ScreenToWorldPoint(screenPoint);
            }
        }
    }
}