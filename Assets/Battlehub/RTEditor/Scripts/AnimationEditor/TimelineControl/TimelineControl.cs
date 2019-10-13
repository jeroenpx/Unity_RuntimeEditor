//#define USE_RTE

using Battlehub.RTCommon;
using Battlehub.UIControls;
using Battlehub.UIControls.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class TimelineControl : MonoBehaviour
    {
        [SerializeField]
        private RawImage m_output = null;
        [SerializeField]
        private TimelineTextPanel m_textPanel = null;

        private Camera m_camera;
        private RenderTextureCamera m_rtCamera;

        private ScrollRect m_scrollRect;
        private RectTransformChangeListener m_rtListener;

        [SerializeField]
        private float m_fixedHeight = -1;
        [SerializeField]
        private Color m_backgroundColor = new Color32(0x27, 0x27, 0x27, 0xFF);

        //Visible interval (seconds - x axis, units - y axis)
        private Vector2 m_interval = Vector2.one;
        
        private TimelineGrid m_timelineGrid;
        private TimelineGridParameters m_timelineGridParams;
        private DragAndDropListener m_hScrollbarListener;
        private DragAndDropListener m_vScrollbarListener;
        private bool m_hScrollValue;
        private bool m_vScrollValue;

#if USE_RTE
        private RuntimeWindow m_window;
        private IRTE m_editor;
#endif

        private void Awake()
        {
            if (m_textPanel == null)
            {
                m_textPanel = GetComponentInChildren<TimelineTextPanel>(true);
            }

            m_scrollRect = GetComponentInChildren<ScrollRect>(true);
            m_scrollRect.scrollSensitivity = 0;
            m_scrollRect.onValueChanged.AddListener(OnInitScrollRectValueChanged);

            m_hScrollbarListener = m_scrollRect.horizontalScrollbar.GetComponentInChildren<DragAndDropListener>(true);
            m_vScrollbarListener = m_scrollRect.verticalScrollbar.GetComponentInChildren<DragAndDropListener>(true);
            m_hScrollbarListener.Drop += OnHorizontalScrollbarDrop;
            m_hScrollbarListener.EndDrag += OnHorizontalScrollbarDrop;
            m_vScrollbarListener.Drop += OnVerticalScrolbarDrop;
            m_vScrollbarListener.EndDrag += OnVerticalScrolbarDrop;

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
            m_window = GetComponentInParent<RuntimeWindow>();
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
            m_timelineGridParams = new TimelineGridParameters();
            m_timelineGridParams.VerticalLinesCount = 12;
            m_timelineGridParams.HorizontalLinesCount = 2;
            m_timelineGridParams.LineColor = new Color(1, 1, 1, 0.1f);
            m_timelineGrid.SetGridParameters(m_timelineGridParams);
            m_textPanel.SetParameters(m_timelineGridParams.VerticalLinesCount, TimelineGrid.k_Lines);

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
            if(m_hScrollbarListener != null)
            {
                m_hScrollbarListener.Drop -= OnHorizontalScrollbarDrop;
                m_hScrollbarListener.EndDrag -= OnHorizontalScrollbarDrop;
            }
            
            if(m_hScrollbarListener != null)
            {
                m_vScrollbarListener.Drop -= OnVerticalScrolbarDrop;
                m_vScrollbarListener.EndDrag -= OnVerticalScrolbarDrop;
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

        private void OnVerticalScrolbarDrop(UnityEngine.EventSystems.PointerEventData eventData)
        {
        }

        private void OnHorizontalScrollbarDrop(UnityEngine.EventSystems.PointerEventData eventData)
        {
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
            #if USE_RTE
            if(m_editor.ActiveWindow != m_window)
            {
                return;
            }
            #endif
            
            bool renderGraphics = false;
            if (m_rtCamera.TryResizeRenderTexture(false))
            {
                renderGraphics = true;
            }

            Vector2 delta = ChangeInterval();
            if (delta != Vector2.zero)
            {
                Vector2 newInterval = m_interval - delta;
                float widthPerLine = m_scrollRect.viewport.rect.width / m_timelineGridParams.VerticalLinesCount;
                newInterval.x = Mathf.Clamp(newInterval.x, 1.0f, 3600.0f); //at 60 samples per second
                newInterval.y = Mathf.Clamp(newInterval.y, 1.0f, 10000.0f); //TODO: handle negative values
                if (newInterval != m_interval)
                {
                    m_interval = newInterval;
                    renderGraphics = true;
                }
            }

            if(renderGraphics)
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
            m_timelineGrid.UpdateGraphics(viewportSize, contentSize, scrollOffset, scrollSize, m_interval, verticalScale);
            m_textPanel.UpdateGraphics(viewportSize.x, contentSize.x, scrollOffset.x, scrollSize.x, m_interval.x - 1);

            m_camera.enabled = true;
            m_camera.Render();
            m_camera.enabled = false;
        }

        protected virtual Vector2 ChangeInterval()
        {
#if USE_RTE
            if(!m_window.IsPointerOver)
            {
                return Vector2.zero;
            }

            float delta = m_editor.Input.GetAxis(InputAxis.Z);
            if(m_editor.Input.GetKeyDown(KeyCode.LeftAlt))
            {
                return new Vector2(0, delta);
            }
            return new Vector2(delta, 0);
#else
            float delta = Input.GetAxis("Mouse ScrollWheel");
            if(Input.GetKey(KeyCode.LeftAlt))
            {
                return new Vector2(0, delta);
            }
            return new Vector2(delta, 0);
#endif
        }
        
    }
}

