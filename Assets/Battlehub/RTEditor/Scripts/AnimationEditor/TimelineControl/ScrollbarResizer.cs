using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class ScrollbarResizer : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IDropHandler, IEndDragHandler
    {
        private Scrollbar m_scrollbar;
        private RectTransform m_scrollbarRect;
        private ScrollbarResizer m_other;
        private ScrollRect m_scrollView;

        private Vector2 Position
        {
            get { return m_scrollbarRect.InverseTransformPoint(transform.TransformPoint(Vector3.zero)); }
        }

        private Vector2 m_beginDragPoint;
        private Vector2 m_beginDragPosition;
        private bool m_isDragging;

        
        private bool IsEnd
        {
            get
            {
                if(m_scrollbar.direction == Scrollbar.Direction.BottomToTop)
                {
                    return Position.y < m_other.Position.y;
                }
                else if(m_scrollbar.direction == Scrollbar.Direction.LeftToRight)
                {
                    return Position.x > m_other.Position.x;
                }
                else
                {
                    throw new System.NotSupportedException();
                }
            }
        }

        private void Awake()
        {
            m_scrollbar = GetComponentInParent<Scrollbar>();

            
            m_scrollbarRect = m_scrollbar.GetComponent<RectTransform>();
            m_other = m_scrollbar.GetComponentsInChildren<ScrollbarResizer>(true).Where(r => r != this).First();
            m_scrollView = GetComponentInParent<ScrollRect>();
            
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            m_isDragging = RectTransformUtility.ScreenPointToLocalPointInRectangle(m_scrollbarRect, eventData.position, eventData.pressEventCamera, out m_beginDragPoint);
            
            m_beginDragPosition = Position;
                       
        }

        public void OnDrag(PointerEventData eventData)
        {
            if(!m_isDragging)
            {
                return;
            }

            Vector2 point;
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(m_scrollbarRect, eventData.position, eventData.pressEventCamera, out point))
            {
                RectTransform.Axis axis = RectTransform.Axis.Vertical;
                float viewportSize = m_scrollView.viewport.rect.height;

                RectTransform slidingArea = (RectTransform)m_scrollbar.handleRect.parent;
                float slidingAreaSize = slidingArea.rect.height;
                float slidingAreaSizeDelta = slidingArea.sizeDelta.y;

                point.x = m_beginDragPoint.x;
                

                if(IsEnd)
                {
                    float offset = (m_beginDragPoint - point).y + slidingAreaSizeDelta;
                    float handleSize = (m_beginDragPosition - m_other.Position).magnitude + offset;

                    float sizeRatio = handleSize / slidingAreaSize;
                    sizeRatio = Mathf.Min(1, Mathf.Max(0.001f, sizeRatio));
                    
                    handleSize = slidingAreaSize * sizeRatio;
                    float newValue = 1 + m_other.Position.y / (slidingAreaSize - handleSize);
                    

                    m_scrollView.content.SetSizeWithCurrentAnchors(axis, viewportSize / sizeRatio);
                    m_scrollbar.value = newValue;
                }
                else
                {
                    float offset = (point - m_beginDragPoint).y + slidingAreaSizeDelta;

                    float handleSize = (m_beginDragPosition - m_other.Position).magnitude + offset;
                    float sizeRatio = handleSize / slidingAreaSize;
                    sizeRatio = Mathf.Min(1, Mathf.Max(0.001f, sizeRatio));
                    
                    handleSize = slidingAreaSize * sizeRatio;
                    float newValue = 1 + (m_beginDragPosition.y - (m_beginDragPoint - point).y) / (slidingAreaSize - handleSize);
                    
                    m_scrollView.content.SetSizeWithCurrentAnchors(axis, viewportSize / sizeRatio);
                    m_scrollbar.value = newValue;

                }
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!m_isDragging)
            {
                return;
            }

            Debug.Log("Drop");
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!m_isDragging)
            {
                return;
            }

            Debug.Log("EndDrag");
        }

     
    }
}

