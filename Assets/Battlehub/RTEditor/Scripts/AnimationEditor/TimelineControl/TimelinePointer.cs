using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.RTEditor
{
    public delegate void TimelinePointerEvent<T>(T sample);

    public class TimelinePointer : MonoBehaviour, IPointerDownHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IDropHandler, IEndDragHandler
    {
        public event TimelinePointerEvent<int> SampleChanged;
        public event TimelinePointerEvent<Vector2Int> PointerDown;

        [SerializeField]
        private RectTransform m_pointer = null;

        [SerializeField]
        private RectTransform m_dragArea = null;

        private TimelineGridParameters m_parameters;

        private float m_visibleColumns;
        private float m_columnWidth;
        private int m_sample;
        private Vector2 m_offset;
        private bool m_isDragInProgress;

        public int Sample
        {
            get { return m_sample; }
        }

        public void SetGridParameters(TimelineGridParameters parameters)
        {
            Vector2 maxSupportedViewportSize = new Vector2(4096, 4096);
            SetGridParameters(parameters, maxSupportedViewportSize);
        }

        public void SetGridParameters(TimelineGridParameters parameters, Vector2 viewportSize)
        {
            m_parameters = parameters;
        }

        public void UpdateGraphics(Vector2 viewportSize, Vector2 contentSize, Vector2 normalizedOffset, Vector2 normalizedSize, Vector2 interval)
        {
            if (m_parameters == null)
            {
                throw new System.InvalidOperationException("Call SetGridParameters method first");
            }

            float px = interval.x * normalizedSize.x;
            m_visibleColumns = m_parameters.VertLines * Mathf.Pow(m_parameters.VertLinesSecondary, Mathf.Log(px, m_parameters.VertLinesSecondary));
            m_columnWidth = viewportSize.x / m_visibleColumns;

            int vLinesSq = m_parameters.VertLinesSecondary * m_parameters.VertLinesSecondary;
            int vLinesCount = m_parameters.VertLines;

            m_offset.x = -(1 - 1 / normalizedSize.x) * normalizedOffset.x  * m_visibleColumns;
            m_offset.y = (1 - normalizedSize.y) * (1 - normalizedOffset.y) * (m_parameters.HorLines - 1);

            Vector3 pos = m_pointer.transform.localPosition;
            pos.x = (m_sample - m_offset.x) * m_columnWidth;
            m_pointer.transform.localPosition = pos;
        }

        private void UpdatePointerPosition(PointerEventData eventData)
        {
            Vector2Int coord;
            if (GetKeyframeCoord(eventData, out coord))
            {
                int oldSample = m_sample;

                Vector3 pos = m_pointer.transform.localPosition;
                m_sample = coord.x;
                pos.x = (m_sample - m_offset.x) * m_columnWidth;
                m_pointer.transform.localPosition = pos;

                if (oldSample != m_sample)
                {
                    if (SampleChanged != null)
                    {
                        SampleChanged(m_sample);
                    }
                }
            }
        }


        private bool GetKeyframeCoord(PointerEventData eventData, out Vector2Int coord)
        {
            Vector2 point;
            RectTransform rt = (RectTransform)transform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, eventData.pressEventCamera, out point))
            {
                coord = new Vector2Int(-1, -1);
                return false;
            }

            int sample = Mathf.RoundToInt(m_offset.x + point.x / m_columnWidth);
            if (sample < 0)
            {
                sample = 0;
            }

            int row = Mathf.FloorToInt(m_offset.y - (point.y / m_parameters.FixedHeight)) - 1;
            coord = new Vector2Int(sample, row);
            return true;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(m_dragArea, eventData.position, eventData.pressEventCamera))
            {
                UpdatePointerPosition(eventData);
            }
            else
            {
                Vector2Int coord;
                if (GetKeyframeCoord(eventData, out coord))
                {
                    if (PointerDown != null)
                    {
                        PointerDown(coord);
                    }
                }
            }
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(m_dragArea, eventData.position, eventData.pressEventCamera))
            {
                m_isDragInProgress = true;
            }
            else
            {
                m_isDragInProgress = false;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (m_isDragInProgress)
            {
                UpdatePointerPosition(eventData);
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            m_isDragInProgress = false;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            m_isDragInProgress = false;
        }
    }
}

