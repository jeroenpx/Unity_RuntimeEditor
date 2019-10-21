//#define USE_RTE
#define TIMELINE_CONTROL_DEBUG

using Battlehub.RTCommon;
using Battlehub.UIControls;
using Battlehub.UIControls.Common;
using Battlehub.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class TimelineControl : MonoBehaviour
    {
        [SerializeField]
        private RawImage m_output = null;
        [SerializeField]
        private TimelineTextPanel m_textPanel = null;
        [SerializeField]
        private TimelinePointer m_pointer = null;
        [SerializeField]
        private TimelineBoxSelection m_boxSelection = null;

        private Dopesheet m_dopesheet;
        private TimelineGrid m_timelineGrid;

        private Camera m_camera;
        private RenderTextureCamera m_rtCamera;

        private ScrollRect m_scrollRect;
        private RectTransformChangeListener m_rtListener;

        [SerializeField]
        private float m_fixedHeight = -1;
        [SerializeField]
        private Color m_backgroundColor = new Color32(0x27, 0x27, 0x27, 0xFF);

        private Vector2 m_interval = Vector2.one;

        private TimelineGridParameters m_timelineGridParams;
        private DragAndDropListener m_hScrollbarListener;
        private DragAndDropListener m_vScrollbarListener;
        private bool m_hScrollValue;
        private bool m_vScrollValue;
        private bool m_renderGraphics;

        public bool MultiselectMode
        {
            get;
            set;
        }

        public int CurrentSample
        {
            get
            {
                if(m_pointer != null)
                {
                    return m_pointer.Sample;
                }
                return 0;
            }
        }

        public int RowsCount
        {
            get { return m_timelineGridParams.HorLines - 1; }
            //set
            //{
            //    m_timelineGridParams.HorLines = value + 1;
            //    SetGridParameters();
            //    m_renderGraphics = true;
            //}
        }
       
        /// Dopesheet.Keyframe must be replaced with more appropriate data structure
        public Dopesheet.Keyframe[,] Keyframes
        {
            get { return m_dopesheet.Keyframes; }
            set
            {
                m_dopesheet.Keyframes = value;
                float colums = m_dopesheet.Keyframes.GetLength(1);
                m_interval.x = Mathf.Log(m_timelineGridParams.VertLinesSecondary * colums / (m_timelineGridParams.VertLines * m_timelineGridParams.VertLinesSecondary), m_timelineGridParams.VertLinesSecondary);
                ChangeInterval(Vector2.zero);
            }
        }


        private void SetGridParameters()
        {
            m_timelineGrid.SetGridParameters(m_timelineGridParams);
            m_dopesheet.SetGridParameters(m_timelineGridParams);
            m_textPanel.SetGridParameters(m_timelineGridParams.VertLines, m_timelineGridParams.VertLinesSecondary, 60);
            m_pointer.SetGridParameters(m_timelineGridParams);
        }

        private void Awake()
        {
            if (m_textPanel == null)
            {
                m_textPanel = GetComponentInChildren<TimelineTextPanel>(true);
            }

            if (m_pointer == null)
            {
                m_pointer = GetComponentInChildren<TimelinePointer>(true);
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
            IRTE editor = IOC.Resolve<IRTE>();
            cameraGo.transform.SetParent(editor, false);
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

            m_timelineGridParams = new TimelineGridParameters();
            m_timelineGridParams.VertLines = 12;
            m_timelineGridParams.VertLinesSecondary = TimelineGrid.k_Lines;
            m_timelineGridParams.HorLines = 21;
            m_timelineGridParams.HorLinesSecondary = 2;
            m_timelineGridParams.LineColor = new Color(1, 1, 1, 0.1f);
            m_timelineGridParams.FixedHeight = m_fixedHeight;

            m_timelineGrid = m_output.GetComponent<TimelineGrid>();
            if (m_timelineGrid == null)
            {
                m_timelineGrid = m_output.gameObject.AddComponent<TimelineGrid>();
            }
            m_timelineGrid.Init(m_camera);


            m_dopesheet = m_output.gameObject.GetComponent<Dopesheet>();
            if (m_dopesheet == null)
            {
                m_dopesheet = m_output.gameObject.AddComponent<Dopesheet>();
            }
            m_dopesheet.Init(m_camera);
            SetGridParameters();

#if TIMELINE_CONTROL_DEBUG
            int samplesCount = 60;// m_timelineGridParams.VertLines * m_timelineGridParams.VertLinesSecondary;

            Dopesheet.Keyframe[,] keyframes = new Dopesheet.Keyframe[m_timelineGridParams.HorLines, samplesCount];
            for (int i = 0; i < m_timelineGridParams.HorLines; ++i)
            {
                for (int j = 0; j < samplesCount; ++j)
                {
                    keyframes[i, j] = (Dopesheet.Keyframe)Random.Range(0, 3);
                }
            }
            Keyframes = keyframes;
#endif
            m_pointer.PointerDown += OnTimlineClick;
            m_pointer.Drag += OnTimelineDrag;

            if (m_boxSelection == null)
            {
                m_boxSelection = GetComponentInChildren<TimelineBoxSelection>();
            }

            if (m_boxSelection != null)
            {
                m_boxSelection.BeginSelection += OnBeginBoxSelection;
                m_boxSelection.Selection += OnBoxSelection;
                
            }

            RenderGraphics();
        }

        private void Start()
        {
            if(GetComponent<TimelineControlInput>() == null)
            {
                gameObject.AddComponent<TimelineControlInput>();
            }
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

            if(m_pointer != null)
            {
                m_pointer.PointerDown -= OnTimlineClick;
                m_pointer.Drag -= OnTimelineDrag;
            }

            if (m_boxSelection != null)
            {
                m_boxSelection.BeginSelection -= OnBeginBoxSelection;
                m_boxSelection.Selection -= OnBoxSelection;
            }
        }

        private void OnTimlineClick(Vector2Int coordinate)
        {
            if(!IsSelected(coordinate) && !MultiselectMode)
            {
                UnselectAll();
            }
            
            Select(coordinate, coordinate);
        }

        private void OnTimelineDrag(int delta)
        {
            Debug.Log("TimelineDrag " + delta);

            Dopesheet.Keyframe[,] keyframes = m_dopesheet.Keyframes;
            int rows = keyframes.GetLength(0);
            int cols = keyframes.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                if (delta < 0)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        if (keyframes[i, j] == Dopesheet.Keyframe.Selected)
                        {
                            if(j - 1 > 0)
                            {
                                keyframes[i, j - 1] = Dopesheet.Keyframe.Selected;
                            }
                            keyframes[i, j] = Dopesheet.Keyframe.Empty;
                        }
                    }
                }
                else
                {
                    for (int j = cols - 1; j >= 0; j--)
                    {
                        if (keyframes[i, j] == Dopesheet.Keyframe.Selected)
                        {
                            if(j + 1 < cols)
                            {
                                keyframes[i, j + 1] = Dopesheet.Keyframe.Selected;
                                keyframes[i, j] = Dopesheet.Keyframe.Empty;
                            }
                        }
                    }
                }
            }
            m_dopesheet.Keyframes = keyframes;
            m_renderGraphics = true;
        }

        private void OnBeginBoxSelection(TimelineBoxSelectionCancelArgs args)
        {
            Vector2Int coord;
            if(m_pointer.GetKeyframeCoordinate(args.LocalPoint, true, false, out coord))
            {
                coord.y++;
                if(IsSelected(coord))
                {
                    args.Cancel = true;
                }
            }
        }

        private void OnBoxSelection(Vector2Int min, Vector2Int max)
        {
            Select(min, max);
        }

        private void UnselectAll()
        {
            Dopesheet.Keyframe[,] keyframes = m_dopesheet.Keyframes;
            int rows = keyframes.GetLength(0);
            int cols = keyframes.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (keyframes[i, j] == Dopesheet.Keyframe.Selected)
                    {
                        keyframes[i, j] = Dopesheet.Keyframe.Normal;
                    }
                }
            }
            m_dopesheet.Keyframes = keyframes;
        }

        private bool IsSelected(Vector2Int coord)
        {
            Dopesheet.Keyframe[,] keyframes = m_dopesheet.Keyframes;
            if(coord.x >= 0 && coord.x < keyframes.GetLength(1) && coord.y >= 0 && coord.y < keyframes.GetLength(0))
            {
                return keyframes[coord.y, coord.x] == Dopesheet.Keyframe.Selected;
            }
            return false;
        }

        private void Select(Vector2Int min, Vector2Int max)
        {
            Dopesheet.Keyframe[,] keyframes = m_dopesheet.Keyframes;
            int rows = keyframes.GetLength(0);
            int cols = keyframes.GetLength(1);

            min.y = Mathf.Max(0, min.y);
            min.x = Mathf.Max(0, min.x);
            max.y = Mathf.Min(rows - 1, max.y);
            max.x = Mathf.Min(cols - 1, max.x);

            for (int i = min.y; i <= max.y; i++)
            {
                for (int j = min.x; j <= max.x; j++)
                {
                    if (keyframes[i, j] == Dopesheet.Keyframe.Normal)
                    {
                        keyframes[i, j] = Dopesheet.Keyframe.Selected;
                    }
                }
            }
            m_dopesheet.Keyframes = keyframes;
            RenderGraphics();
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
            Vector2 viewportSize = m_scrollRect.viewport.rect.size;

            if (m_timelineGridParams.FixedHeight > -1)
            {
                m_scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, m_timelineGridParams.FixedHeight * (m_timelineGridParams.HorLines - 1));
            }

            if (viewportSize != m_output.rectTransform.sizeDelta)
            {
                m_output.rectTransform.sizeDelta = viewportSize;
            }
        }

        public void ChangeInterval(Vector2 delta)
        {
            Vector2 newInterval = m_interval - delta;
            float widthPerLine = m_scrollRect.viewport.rect.width / m_timelineGridParams.VertLines;
            newInterval.x = Mathf.Clamp(newInterval.x, 1.0f, Mathf.Log(3600 * 24, m_timelineGridParams.VertLinesSecondary)); //at 60 samples per second
            newInterval.y = Mathf.Clamp(newInterval.y, 1.0f, 10000.0f); //TODO: handle negative values

            if (newInterval != m_interval)
            {
                m_interval = newInterval;
                m_renderGraphics = true;
            }
        }

        public void AddKeyframe(int sample, int row)
        {
            if(sample < 0 || row < 0)
            {
                return;
            }

            Dopesheet.Keyframe[,] keyframes = m_dopesheet.Keyframes;
            int rows = keyframes.GetLength(0);
            int cols = keyframes.GetLength(1);

            if(sample >= cols || row >= rows)
            {
                cols = Mathf.Max(sample + 1, cols);
                rows = Mathf.Max(row + 1, rows);

                ArrayHelper.ResizeArray(ref keyframes, rows, cols);
            }

            if(keyframes[row, sample] == Dopesheet.Keyframe.Empty)
            {
                keyframes[row, sample] = Dopesheet.Keyframe.Normal;
            }

            m_dopesheet.Keyframes = keyframes;

            m_renderGraphics = true;
        }

        public void DeleteSelectedKeyframes()
        {
            Dopesheet.Keyframe[,] keyframes = m_dopesheet.Keyframes;
            int rows = keyframes.GetLength(0);
            int cols = keyframes.GetLength(1);
            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    if (keyframes[i, j] == Dopesheet.Keyframe.Selected)
                    {
                        keyframes[i, j] = Dopesheet.Keyframe.Empty;
                    }
                }
            }
            RenderGraphics();
        }

        private void LateUpdate()
        {
            if (m_rtCamera.TryResizeRenderTexture(false))
            {
                m_renderGraphics = true;
            }

            if(m_renderGraphics)
            {
                RenderGraphics();
                m_renderGraphics = false;
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

            Vector2 interval = m_interval;
            
            interval.x = Mathf.Pow(m_timelineGridParams.VertLinesSecondary, interval.x);
            interval.y = Mathf.Pow(m_timelineGridParams.HorLinesSecondary, interval.y);

            m_textPanel.UpdateGraphics(viewportSize.x, contentSize.x, scrollOffset.x, scrollSize.x, interval.x);
            m_timelineGrid.UpdateGraphics(viewportSize, contentSize, scrollOffset, scrollSize, interval);
            m_dopesheet.UpdateGraphics(viewportSize, contentSize, scrollOffset, scrollSize, interval);
            m_pointer.UpdateGraphics(viewportSize, contentSize, scrollOffset, scrollSize, interval);

            m_camera.enabled = true;
            m_camera.Render();
            m_camera.enabled = false;
        }   
    }
}

