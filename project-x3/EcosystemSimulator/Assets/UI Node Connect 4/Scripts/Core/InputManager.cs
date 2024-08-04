using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MeadowGames.UINodeConnect4
{
    public abstract class InputManager : MonoBehaviour
    {
        static InputManager _instance;
        public static InputManager Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = FindObjectOfType<InputManager>();
                }
                return _instance;
            }
        }

        public abstract Vector3 ScreenPointerPosition { get; }
        public abstract Vector3 GetCanvasPointerPosition(GraphManager graphManager);
        public abstract bool PointerPress { get; }
        public abstract bool Aux0KeyPress { get; }
        public abstract void OnUpdate();
        public abstract UnityEvent e_OnPointerDown { get; set; }
        public abstract UnityEvent e_OnDrag { get; set; }
        public abstract UnityEvent e_OnPointerUp { get; set; }
        public abstract UnityEvent e_OnDelete { get; set; }
        public abstract UnityEvent e_OnPointerHover { get; set; }
    }
}