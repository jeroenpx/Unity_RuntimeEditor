using Battlehub.Utils;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.UIControls.DockPanels
{
    public delegate void ResizerEventHandler(Resizer resizer, Region region);

    public class Resizer : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public static event ResizerEventHandler BeginResize;
        public static event ResizerEventHandler Resize;
        public static event ResizerEventHandler EndResize;

        [SerializeField]
        private Texture2D m_cursor;

        [SerializeField]
        private Region m_region;

        private RectTransform m_parentRT;
        private LayoutElement m_layout;
        private LayoutElement m_siblingLayout;

        private bool m_isEnabled;
        private bool m_isFree;
        //private Vector2 m_prevPoint;
        [SerializeField]
        private float m_dx;
        [SerializeField]
        private float m_dy;
        private Vector2 m_adjustment;
        private bool m_isDragging;

        [SerializeField]
        private bool m_forceRebuildLayoutImmediate = true;

        private void Awake()
        {
            if(m_region == null)
            {
                m_region = GetComponentInParent<Region>();
                m_layout = m_region.GetComponent<LayoutElement>();
            }
        }

        private void Start()
        {
            if (m_region == m_region.Root.RootRegion)
            {
                Destroy(gameObject);
            }
            else
            {
                UpdateState();
            }
        }

        public void UpdateState()
        {
            StartCoroutine(CoUpdateState());
        }

        private IEnumerator CoUpdateState()
        {
            yield return new WaitForEndOfFrame();
            m_isEnabled = m_isFree = m_region.IsFree();
            if (!m_isFree)
            {
                Canvas canvas = GetComponentInParent<Canvas>();
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, transform.position);

                RectTransform rt = (RectTransform)transform;
                Debug.Assert(RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenPoint, canvas.worldCamera, out m_adjustment));

                if (m_dx == 0 || m_dy == 0)
                {
                    int siblingIndex = m_region.transform.GetSiblingIndex();

                    HorizontalOrVerticalLayoutGroup layoutGroup = m_region.GetComponentInParent<HorizontalOrVerticalLayoutGroup>();
                    if (layoutGroup is HorizontalLayoutGroup)
                    {
                        if (siblingIndex == 0 && m_dx > 0)
                        {
                            m_isEnabled = true;
                        }
                        else if (siblingIndex == 1 && m_dx < 0)
                        {
                            m_isEnabled = true;
                        }
                    }
                    else
                    {
                        if (siblingIndex == 0 && m_dy < 0)
                        {
                            m_isEnabled = true;
                        }
                        else if (siblingIndex == 1 && m_dy > 0)
                        {
                            m_isEnabled = true;
                        }
                    }

                    if (m_isEnabled)
                    {
                        m_siblingLayout = m_region.transform.parent.GetChild((siblingIndex + 1) % 2).GetComponent<LayoutElement>();
                        m_parentRT = (RectTransform)m_region.transform.parent;
                    }
                    else
                    {
                        m_isEnabled = false;
                        m_siblingLayout = null;
                        m_parentRT = null;
                    }

                }
            }
            else
            {
                m_siblingLayout = null;
                m_parentRT = null;
            }
        }

     
      
        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (!m_isEnabled)
            {
                return;
            }
            eventData.useDragThreshold = false;
            m_region.Root.CursorHelper.SetCursor(this, m_cursor);
        }


        private HorizontalOrVerticalLayoutGroup[] m_layoutGroups;

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if(!m_isEnabled)
            {
                return;
            }

            if(m_isFree)
            {
                Vector2 position = eventData.position;
                Camera camera = eventData.pressEventCamera;

                RectTransform rt = (RectTransform)transform;
                Debug.Assert(RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, position, camera, out m_adjustment));
            }
            if (m_forceRebuildLayoutImmediate)
            {
                m_layoutGroups = m_region.Root.GetComponentsInChildren<HorizontalOrVerticalLayoutGroup>();
            }

            if (BeginResize != null)
            {
                BeginResize(this, m_region);
            }
           
            m_isDragging = true;
        }


        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!m_isEnabled)
            {
                return;
            }

            if(m_isFree)
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint((RectTransform)m_region.Root.transform, eventData.position, eventData.pressEventCamera))
                {
                    return;
                }

                RectTransform rt = (RectTransform)transform;
                RectTransform regionRT = (RectTransform)m_region.transform;
                RectTransform freePanelRt = (RectTransform)m_region.Root.Free;

                Vector2 point;
                Debug.Assert(RectTransformUtility.ScreenPointToLocalPointInRectangle(freePanelRt, eventData.position, eventData.pressEventCamera, out point));

                Vector2 pivotOffset = Vector2.Scale(freePanelRt.rect.size, freePanelRt.pivot);
                pivotOffset.y *= -1;

                point += pivotOffset;

                Vector2 offsetMin = regionRT.offsetMin;
                Vector2 offsetMax = regionRT.offsetMax;

                float size = Mathf.Min(rt.rect.width, rt.rect.height);
                float minWidth = m_layout.minWidth + size;
                float minHeight = m_layout.minHeight + size;

                if (m_dx < 0)
                {
                    offsetMin.x = point.x - m_adjustment.x;
                    if (offsetMax.x - offsetMin.x < minWidth)
                    {
                        offsetMin.x = offsetMax.x - minWidth;
                    }
                }
                else if (m_dx > 0)
                {
                    offsetMax.x = point.x - m_adjustment.x;
                    if (offsetMax.x - offsetMin.x < minWidth)
                    {
                        offsetMax.x = offsetMin.x + minWidth;
                    }
                }

                if (m_dy < 0)
                {
                    offsetMin.y = point.y - m_adjustment.y;
                    if (offsetMax.y - offsetMin.y < minHeight)
                    {
                        offsetMin.y = offsetMax.y - minHeight;
                    }
                }
                else if (m_dy > 0)
                {
                    offsetMax.y = point.y - m_adjustment.y;
                    if (offsetMax.y - offsetMin.y < minHeight)
                    {
                        offsetMax.y = offsetMin.y + minHeight;
                    }
                }

                regionRT.offsetMin = offsetMin;
                regionRT.offsetMax = offsetMax;

                //m_prevPoint = point;
            }
            else
            {
                Vector2 size = m_parentRT.rect.size - new Vector2(m_layout.minWidth + m_siblingLayout.minWidth, m_layout.minHeight + m_siblingLayout.minHeight);

                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(m_parentRT, eventData.position, eventData.pressEventCamera, out localPoint);
                
                Vector2 pivotPosition = m_parentRT.rect.size * m_parentRT.pivot;

                if(m_dx > 0 || m_dy > 0)
                {
                    localPoint = (pivotPosition + localPoint) - new Vector2(m_layout.minWidth, m_layout.minHeight);
                    
                    m_layout.flexibleWidth = localPoint.x / size.x;
                    m_layout.flexibleHeight = localPoint.y / size.y;
                    m_siblingLayout.flexibleWidth = 1 - m_layout.flexibleWidth;
                    m_siblingLayout.flexibleHeight = 1 - m_layout.flexibleHeight;
                }
                else
                {
                    localPoint = (pivotPosition + localPoint) - new Vector2(m_siblingLayout.minWidth, m_siblingLayout.minHeight);

                    m_siblingLayout.flexibleWidth = localPoint.x / size.x;
                    m_siblingLayout.flexibleHeight = localPoint.y / size.y;
                    m_layout.flexibleWidth = 1 - m_siblingLayout.flexibleWidth;
                    m_layout.flexibleHeight = 1 - m_siblingLayout.flexibleHeight;
                }
            }

            if(m_forceRebuildLayoutImmediate)
            {
                for (int i = 0; i < m_layoutGroups.Length; ++i)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)m_layoutGroups[i].transform);
                }
            }
            
            if (Resize != null)
            {
                Resize(this, m_region);
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            m_isDragging = false;
            m_layoutGroups = null;
            if (!m_isEnabled)
            {
                return;
            }
            
            m_region.Root.CursorHelper.ResetCursor(this);

            if(EndResize != null)
            {
                EndResize(this, m_region);
            }
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (!m_isEnabled)
            {
                return;
            }
            if (!m_isDragging)
            {
                m_region.Root.CursorHelper.SetCursor(this, m_cursor);
            }
            
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (!m_isEnabled)
            {
                return;
            }
            if (!m_isDragging)
            {
                m_region.Root.CursorHelper.ResetCursor(this);
            }
            
        }

       
    }

}

