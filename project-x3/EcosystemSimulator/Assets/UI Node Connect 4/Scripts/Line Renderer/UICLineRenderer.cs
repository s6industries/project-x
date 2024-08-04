/// Daniel C Menezes
/// Procedural UI Rounded Corners - https://danielcmcg.github.io/
/// 
/// Based on CiaccoDavide's Unity-UI-Polygon
/// Sourced from - https://github.com/CiaccoDavide/Unity-UI-Polygon

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MeadowGames.UINodeConnect4.GraphicRenderer
{
    [RequireComponent(typeof(CanvasRenderer))]
    [ExecuteInEditMode]
    public class UICLineRenderer : MaskableGraphic
    {
        public Sprite s1;

        [SerializeField] Texture m_Texture;

        RectTransform _rectTransform;
        RectTransform RectTransform
        {
            get
            {
                if (!_rectTransform)
                    _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }
        // linewidth varies with the rectTransform scale, following the screen scale
        public float rectScaleX { get => _rectTransform.localScale.x; }

        [SerializeField] Canvas _canvas;
        Canvas Canvas
        {
            get
            {
                if (!_canvas)
                    _canvas = GetComponentInParent<Canvas>();
                return _canvas;
            }
        }

        [SerializeField] bool _compensateScaleOfParents = false;
        List<RectTransform> _parentsToCompensateScale = new List<RectTransform>();
        public bool CompensateScaleOfParents
        {
            get => _compensateScaleOfParents;
            set
            {
                _compensateScaleOfParents = value;
                _parentsToCompensateScale.Clear();
                _parentsToCompensateScale.AddRange(GetComponentsInParent<RectTransform>());
            }
        }

        [HideInInspector]
        public override Texture mainTexture
        {
            get => m_Texture == null ? s_WhiteTexture : m_Texture;
        }
        [HideInInspector]
        public Texture texture
        {
            get => m_Texture;
            set
            {
                if (m_Texture == value) return;
                m_Texture = value;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            CompensateScaleOfParents = _compensateScaleOfParents;
        }
#endif

        protected override void OnEnable()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
            CompensateScaleOfParents = _compensateScaleOfParents;

            UICSystemManager.AddToUpdate(OnUpdate);
        }

        protected override void OnDisable()
        {
            UICSystemManager.RemoveFromUpdate(OnUpdate);
        }

        void OnUpdate()
        {
            _rectTransform.anchorMin = new Vector2(0, 0);
            _rectTransform.anchorMax = new Vector2(1, 1);
            _rectTransform.offsetMin = new Vector2(0, 0);
            _rectTransform.offsetMax = new Vector2(0, 0);

            // scales the line renderer
            // scale the line renderer for world space
            if (Canvas.renderMode != RenderMode.WorldSpace)
            {
                _rectTransform.localScale = Vector3.one / Canvas.scaleFactor;
            }
            else
            {
                _rectTransform.localScale = Vector3.one;
                _rectTransform.pivot = new Vector2(0.5f, 0.5f);
            }

            if (CompensateScaleOfParents)
            {
                if (Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    foreach (RectTransform rt in _parentsToCompensateScale)
                    {
                        if (rt.localScale.x != 0)
                            _rectTransform.localScale = _rectTransform.localScale / rt.localScale.x;
                    }
                }
                else if (Canvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    for (int i = 0; i < _parentsToCompensateScale.Count - 1; i++)
                    {
                        RectTransform rt = _parentsToCompensateScale[i];

                        if (rt.localScale.x != 0)
                            _rectTransform.localScale = _rectTransform.localScale / rt.localScale.x;
                    }
                }
            }

            SetVerticesDirty();
            SetMaterialDirty();
        }

        UIVertex[] vbo = new UIVertex[4];
        UIVertex vert = new UIVertex();

        protected UIVertex[] SetVbo(Vector2[] vertices, Vector2[] uvs, Color32 color)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vert.color = color;
                vert.position = vertices[i] - lineRendererOffset;
                vert.uv0 = uvs[i];
                vbo[i] = vert;
            }

            return vbo;
        }

        Vector2 lastPos2;
        Vector2 lastPos3;

        UnityEvent E_OnPopulateMesh = new UnityEvent();
        List<UnityAction> OnPopulateMeshActions = new List<UnityAction>();
        public void OnPopulateMeshAddListener(UnityAction action)
        {
            if (!OnPopulateMeshActions.Contains(action))
            {
                OnPopulateMeshActions.Add(action);
                E_OnPopulateMesh.AddListener(action);
            }
        }
        public void OnPopulateMeshRemoveListener(UnityAction action)
        {
            if (OnPopulateMeshActions.Contains(action))
            {
                OnPopulateMeshActions.Remove(action);
                E_OnPopulateMesh.RemoveListener(action);
            }
        }

        VertexHelper vh;
        Vector2 lineRendererOffset;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            // Correct line position when LineRenderer is offset for Canvas Overlay
            lineRendererOffset = Vector2.zero;
            if (Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                lineRendererOffset = transform.position;
            }

            if (this.vh == null)
                this.vh = vh;

            vh.Clear();

            if (vh.currentVertCount < 65000)
                E_OnPopulateMesh.Invoke();
            else
                Debug.Log("<!> Vertices limit reached");
        }

        Vector2 animPositionStart;
        Vector2 animPositionEnd;

        Vector2 uv0 = new Vector2(0, 0);
        Vector2 uv1 = new Vector2(0, 1);
        Vector2 uv2 = new Vector2(1, 1);
        Vector2 uv3 = new Vector2(1, 0);

        public Vector2[] UVs => new[] { uv0, uv1, uv2, uv3 };
        public Vector2[] UVs90 => new[] { uv3, uv0, uv1, uv2 };

        Vector2 pos0;
        Vector2 pos1;
        Vector2 pos2;
        Vector2 pos3;

        Vector2 p0;
        Vector2 p1;

	// v4.1 - UICLineRenderer.AddUIVertexQuad refactored to improved performance
        public void AddUIVertexQuad(Vector2[] vertices, Vector2[] uvs, Color32 color)
        {
            int startIndex = vh.currentVertCount;

            vert.color = color;
            vert.position = vertices[0] - lineRendererOffset;
            vert.uv0 = uvs[0];

            vh.AddVert(vert);

            vert.color = color;
            vert.position = vertices[1] - lineRendererOffset;
            vert.uv0 = uvs[1];

            vh.AddVert(vert);

            vert.color = color;
            vert.position = vertices[2] - lineRendererOffset;
            vert.uv0 = uvs[2];

            vh.AddVert(vert);

            vert.color = color;
            vert.position = vertices[3] - lineRendererOffset;
            vert.uv0 = uvs[3];

            vh.AddVert(vert);

            vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vh.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }

        public void AddUIVertexQuad(Vector2[] vertices, Vector2[] uvs, Color32 colorStart, Color32 colorEnd)
        {
            int startIndex = vh.currentVertCount;

            vert.color = colorStart;
            vert.position = vertices[0] - lineRendererOffset;
            vert.uv0 = uvs[0];

            vh.AddVert(vert);

            vert.color = colorStart;
            vert.position = vertices[1] - lineRendererOffset;
            vert.uv0 = uvs[1];

            vh.AddVert(vert);

            vert.color = colorEnd;
            vert.position = vertices[2] - lineRendererOffset;
            vert.uv0 = uvs[2];

            vh.AddVert(vert);

            vert.color = colorEnd;
            vert.position = vertices[3] - lineRendererOffset;
            vert.uv0 = uvs[3];

            vh.AddVert(vert);

            vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vh.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }

        // v4.1 - added method UICLineRenderer.AddUIVertexTriangle to improved performance
        public void AddUIVertexTriangle(Vector2[] vertices, Vector2[] uvs, Color32 color)
        {
            int startIndex = vh.currentVertCount;

            vert.color = color;
            vert.position = vertices[0] - lineRendererOffset;
            vert.uv0 = uvs[0];

            vh.AddVert(vert);

            vert.color = color;
            vert.position = vertices[1] - lineRendererOffset;
            vert.uv0 = uvs[1];

            vh.AddVert(vert);

            vert.color = color;
            vert.position = vertices[2] - lineRendererOffset;
            vert.uv0 = uvs[2];

            vh.AddVert(vert);

            vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        }
    }
}
