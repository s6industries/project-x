using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeadowGames.UINodeConnect4.UICContextMenu
{
    public abstract class ContextItem : MonoBehaviour, IContextItem, IElement
    {
        ContextMenuManager _contextMenu;
        public virtual ContextMenuManager ContextMenu
        {
            get
            {
                if (!_contextMenu)
                    _contextMenu = GetComponentInParent<ContextMenuManager>();
                return _contextMenu;
            }
            set => _contextMenu = value;
        }

        public virtual int Priority => -10;

        protected virtual void Awake()
        {
            ContextMenu = GetComponentInParent<ContextMenuManager>();
        }

        public virtual void OnChangeSelection()
        {

        }

        public virtual void Remove()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
#endif
                Destroy(gameObject);
#if UNITY_EDITOR
            }
            else
            {
                DestroyImmediate(gameObject);
            }
#endif
        }
    }
}