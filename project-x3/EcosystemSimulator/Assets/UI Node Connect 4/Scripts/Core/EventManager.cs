using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MeadowGames.UINodeConnect4
{
    public enum UICEventType
    {
        OnPointerDown, OnDrag, OnPointerUp, OnDeleteKeyPressed,
        OnPointerHoverEnter, OnPointerHoverExit,
        OnConnectionCreated, OnConnectionRemoved,
        NodeAdded, NodeRemoved, ConnectionAdded, ConnectionRemoved,
        OnElementSelected, OnElementUnselected
    }

    [System.Serializable]
    public class UICEvent<T> : UnityEvent<T> { }

    public class EventManager<T>
    {
        Dictionary<UICEventType, UICEvent<T>> _eventDictionaryElement;

        public EventManager()
        {
            if (_eventDictionaryElement == null)
            {
                _eventDictionaryElement = new Dictionary<UICEventType, UICEvent<T>>();
            }
        }

        public void StartListening(UICEventType eventName, UnityAction<T> listener)
        {
            UICEvent<T> thisEvent = null;
            if (_eventDictionaryElement.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.AddListener(listener);
            }
            else
            {
                thisEvent = new UICEvent<T>();
                thisEvent.AddListener(listener);
                _eventDictionaryElement.Add(eventName, thisEvent);
            }
        }

        public void StopListening(UICEventType eventName, UnityAction<T> listener)
        {
            UICEvent<T> thisEvent = null;
            if (_eventDictionaryElement.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.RemoveListener(listener);
            }
        }

        public void TriggerEvent(UICEventType eventName, T obj)
        {
            UICEvent<T> thisEvent = null;
            if (_eventDictionaryElement.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.Invoke(obj);
            }
        }
    }
}