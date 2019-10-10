#define USE_RTE

using Battlehub.RTCommon;
using Battlehub.UIControls;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class TimelineControl : MonoBehaviour
    {
        [SerializeField]
        private RawImage m_output = null;
        private Camera m_camera;
        private RenderTextureCamera m_rtCamera;

        private ScrollRect m_scrollRect;
        private RectTransformChangeListener m_rtListener;

        [SerializeField]
        private float m_fixedHeight = -1;
        [SerializeField]
        private Color m_backgroundColor = new Color32(0x27, 0x27, 0x27, 0xFF);

        private TimelineGrid m_timelineGrid;
#if USE_RTE
        private IRTE m_editor;
#endif

        private void Awake()
        {
            m_scrollRect = GetComponentInChildren<ScrollRect>(true);
            m_scrollRect.scrollSensitivity = 0;
            m_scrollRect.onValueChanged.AddListener(OnInitScrollRectValueChanged);

            if (m_fixedHeight > -1)
            {
                ScrollbarResizer[] resizers = m_scrollRect.verticalScrollbar.GetComponentsInChildren<ScrollbarResizer>(true);
                for (int i = 0; i < resizers.Length; ++i)
                {
                    resizers[i].gameObject.SetActive(false);
                }
            }

            m_rtListener = m_scrollRect.gameObject.AddComponent<RectTransformChangeListener>();
            m_rtListener.RectTransformChanged += OnRectTransformChanged;

            if (m_output == null)
            {
                m_output = m_scrollRect.content.GetComponentInChildren<RawImage>(true);
            }

            GameObject cameraGo = new GameObject("TimelineGraphicsCamera");
            cameraGo.SetActive(false);

#if USE_RTE
            m_editor = IOC.Resolve<IRTE>();
            cameraGo.transform.SetParent(m_editor.Root, false);
#endif

            m_camera = cameraGo.AddComponent<Camera>();
            m_camera.enabled = false;
            m_camera.orthographic = true;
            m_camera.orthographicSize = 0.5f;
            m_camera.clearFlags = CameraClearFlags.SolidColor;
            m_camera.backgroundColor = m_backgroundColor;
            m_camera.cullingMask = 0;

            m_rtCamera = cameraGo.AddComponent<RenderTextureCamera>();
            m_rtCamera.Fullscreen = false;
            m_rtCamera.Output = m_output;

            cameraGo.SetActive(true);
            m_rtCamera.enabled = false;

            m_timelineGrid = m_output.gameObject.AddComponent<TimelineGrid>();
            m_timelineGrid.Init(m_camera);

            RenderGraphics();
        }

        private void OnDestroy()
        {
            if (m_rtListener != null)
            {
                m_rtListener.RectTransformChanged -= OnRectTransformChanged;
            }

            if (m_scrollRect != null)
            {
                m_scrollRect.onValueChanged.AddListener(OnInitScrollRectValueChanged);
                m_scrollRect.onValueChanged.RemoveListener(OnScrollRectValueChanged);
            }

            if (m_camera != null)
            {
                Destroy(m_camera.gameObject);
            }
        }

        private void OnInitScrollRectValueChanged(Vector2 value)
        {
            //This required to skip first scroll rect value change
            m_scrollRect.onValueChanged.RemoveListener(OnInitScrollRectValueChanged);
            m_scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);
        }

        private void OnScrollRectValueChanged(Vector2 value)
        {
            RenderGraphics();
        }

        private void OnRectTransformChanged()
        {
            Vector2 contentSize = m_scrollRect.content.sizeDelta;
            Vector2 viewportSize = m_scrollRect.viewport.rect.size;

            if(m_fixedHeight > -1)
            {
                viewportSize.y = m_fixedHeight;
            }

            if (viewportSize != contentSize)
            {
                m_scrollRect.content.sizeDelta = viewportSize;
                m_output.rectTransform.sizeDelta = viewportSize;
            }
        }
        
        private void LateUpdate()
        {
            if (m_rtCamera.TryResizeRenderTexture(false))
            {
                RenderGraphics();
            }
        }

        private void RenderGraphics()
        {
            Vector2 viewportSize = m_scrollRect.viewport.rect.size;
            viewportSize.y = Mathf.Max(viewportSize.y, Mathf.Epsilon);

            Vector2 scrollOffset = new Vector2(
                    m_scrollRect.horizontalScrollbar.value,
                    m_scrollRect.verticalScrollbar.value);

            Vector2 scrollSize =  new Vector2(
                    m_scrollRect.horizontalScrollbar.size,
                    m_scrollRect.verticalScrollbar.size);
            
            Vector2 contentSize = m_scrollRect.content.sizeDelta;
            contentSize.y = Mathf.Max(contentSize.y, Mathf.Epsilon);

            float verticalScale = (m_fixedHeight > -1) ? m_fixedHeight / contentSize.y : 1;

            m_timelineGrid.UpdateGraphics(viewportSize, contentSize, scrollOffset, scrollSize, verticalScale);

            m_camera.enabled = true;
            m_camera.Render();
            m_camera.enabled = false;
        }

        
    }
}

