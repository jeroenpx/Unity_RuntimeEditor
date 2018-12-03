using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.UIControls.DockPanels
{
    public enum RegionSplitType
    {
        None,
        Left,
        Top,
        Right,
        Bottom
    }

    public delegate void RegionEventArgs(Region sender);

    public class Region : MonoBehaviour, IPointerDownHandler
    {
        private static int m_regionDebugId;
        public static event RegionEventArgs Selected;
        public static event RegionEventArgs Unselected;

        [SerializeField]
        private LayoutElement m_layoutElement;

        [SerializeField]
        private ToggleGroup m_tabPanel;
        
        [SerializeField]
        private Transform m_contentPanel;

        [SerializeField]
        private RectTransform m_childrenPanel;

        [SerializeField]
        private RectTransform m_previewPanel;
        public RectTransform PreviewPanel
        {
            get { return m_previewPanel; }
        }

        [SerializeField]
        private Tab m_tabPrefab;

        [SerializeField]
        private DockPanelsRoot m_root;
        
        public DockPanelsRoot Root
        {
            get { return m_root; }
        }

        private Tab m_activeTab;
        public int ActiveTabIndex
        {
            get
            {
                if(m_activeTab != null)
                {
                    return m_activeTab.Index;
                }
                return -1;
            }   
        }

        private bool m_isSelected;
        public bool IsSelected
        {
            get { return m_isSelected; }
            private set
            {
                if(m_isSelected != value)
                {
                    m_isSelected = value;
                    if(m_isSelected)
                    {
                        if(Selected != null)
                        {
                            Selected(this);
                        }
                    }
                    else
                    {
                        if(Unselected != null)
                        {
                            Unselected(this);
                        }
                    }
                }
            }
        }

        private bool m_isDraggingOutside;
        private Vector2 m_beginDragTabPos;
        private RectTransform m_pointerOverTab;
        private Region m_pointerOverRegion;
        private Region m_beginDragRegion;
        private bool m_isFree = false;
        private RegionSplitType m_splitType;

        protected virtual void Awake()
        {
            if(m_root == null)
            {
                m_root = GetComponentInParent<DockPanelsRoot>();
            }
        }

        protected virtual void OnDestroy()
        {
            Region parent = GetComponentInParent<Region>();
            if (parent != null)
            {
                parent.DestroyChildRegion(transform.GetSiblingIndex());
            }
        }

        private void Subscribe(Tab tab, Region region)
        {
            tab.Toggle += region.OnTabToggle;
            tab.PointerDown += region.OnTabPointerDown;
            tab.Close += region.OnTabClose;
            tab.BeginDrag += region.OnTabBeginDrag;
            tab.Drag += region.OnTabDrag;
            tab.EndDrag += region.OnTabEndDrag;
        }

        private void Unsubscribe(Tab tab, Region region)
        {
            tab.Toggle -= region.OnTabToggle;
            tab.PointerDown -= region.OnTabPointerDown;
            tab.Close -= region.OnTabClose;
            tab.BeginDrag -= region.OnTabBeginDrag;
            tab.Drag -= region.OnTabDrag;
            tab.EndDrag -= region.OnTabEndDrag;

            if (tab == m_activeTab)
            {
                m_activeTab = null;
            }
        }

        public void Add(Sprite icon, string header, Transform content, RegionSplitType splitType = RegionSplitType.None)
        {
            if(m_childrenPanel.childCount > 0)
            {
                throw new InvalidOperationException("Unable to Add content. Region has children and is not a \"leaf\" region.");
            }

            Tab tab = Instantiate(m_tabPrefab);
            tab.name = "Tab " + header;
            tab.Icon = icon;
            tab.Text = header;
            Insert(m_tabPanel.transform.childCount, tab, content, splitType);
        }

        public void RemoveAt(int index)
        {
            if(index < 0 || m_tabPanel.transform.childCount <= index)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (m_tabPanel.transform.childCount == 1)
            {
                Destroy(gameObject);
            }
            else
            {
                Tab tab = m_tabPanel.transform.GetChild(index).GetComponent<Tab>();
                if(index == ActiveTabIndex)
                {
                    Tab nextTab;
                    if (index < m_tabPanel.transform.childCount - 1)
                    {
                        nextTab = m_tabPanel.transform.GetChild(index + 1).GetComponent<Tab>();
                    }
                    else
                    {
                        nextTab = m_tabPanel.transform.GetChild(index - 1).GetComponent<Tab>();   
                    }
                    nextTab.IsOn = true;
                }

                Unsubscribe(tab, this);
              
                Transform content = m_contentPanel.transform.GetChild(index);

                Destroy(tab.gameObject);
                Destroy(content.gameObject);
            }
        }

        public void Move(int index, int targetIndex, Region targetRegion, RegionSplitType targetSplitType = RegionSplitType.None)
        {
            if (m_childrenPanel.childCount > 0)
            {
                throw new InvalidOperationException("Unable to Remove content. Region has children and is not a \"leaf\" region.");
            }

            if (index < 0 || m_tabPanel.transform.childCount <= index)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            Tab tab = m_tabPanel.transform.GetChild(index).GetComponent<Tab>();
            Unsubscribe(tab, this);

            bool destroy = m_tabPanel.transform.childCount == 1 && targetRegion != this;

            Transform content = m_contentPanel.transform.GetChild(index);
            targetRegion.Insert(targetIndex, tab, content, targetSplitType);

            if(destroy)
            {
                Destroy(gameObject);
            }
        }

        private void DestroyChildRegion(int index)
        {
            if(m_childrenPanel == null)
            {
                return;
            }
            
            Destroy(m_childrenPanel.GetChild(index).gameObject);
            index = (index + 1) % 2;

            Region childRegion = m_childrenPanel.GetChild(index).GetComponent<Region>();
            if(childRegion != null && childRegion.m_contentPanel != null && childRegion.m_tabPanel != null && childRegion.m_tabPanel.transform != null)
            {
                Transform[] contents = childRegion.m_contentPanel.OfType<Transform>().ToArray();
                Tab[] tabs = childRegion.m_tabPanel.transform.OfType<Transform>().Select(t => t.GetComponent<Tab>()).ToArray();
                for (int i = 0; i < tabs.Length; ++i)
                {
                    Tab tab = tabs[i];
                    if(tab != null && m_tabPanel != null && m_tabPanel.transform != null)
                    {
                        Unsubscribe(tab, childRegion);
                        Subscribe(tab, this);
                        
                        tab.transform.SetParent(m_tabPanel.transform, false);
                        tab.ToggleGroup = m_tabPanel;
                    }

                    Transform content = contents[i];
                    if(content != null)
                    {
                        content.transform.SetParent(m_contentPanel, false);
                    }
                }
            }

            HorizontalOrVerticalLayoutGroup layoutGroup = m_childrenPanel.GetComponent<HorizontalOrVerticalLayoutGroup>();
            if(layoutGroup != null)
            {
                Destroy(layoutGroup);
            }
        }

        private void Insert(int index, Tab tab, Transform content, RegionSplitType splitType = RegionSplitType.None)
        {
            if (m_childrenPanel.childCount > 0)
            {
                throw new InvalidOperationException("Unable to Add content. Region has children and is not a \"leaf\" region.");
            }

            switch (splitType)
            {
                case RegionSplitType.None:
                    Insert(index, tab, content);
                    break;
                case RegionSplitType.Left:
                    SplitLeft(tab, content);
                    break;
                case RegionSplitType.Top:
                    SplitTop(tab, content);
                    break;
                case RegionSplitType.Right:
                    SplitRight(tab, content);
                    break;
                case RegionSplitType.Bottom:
                    SplitBottom(tab, content);
                    break;
            }
        }

        private void Insert(int index, Tab tab, Transform content)
        {
            content.SetParent(m_contentPanel, false);

            tab.transform.SetParent(m_tabPanel.transform, false);
            tab.transform.SetSiblingIndex(index);
            tab.ToggleGroup = m_tabPanel;

            Subscribe(tab, this);
            tab.IsOn = true;
        }

        private void SplitTop(Tab tab, Transform content)
        {
            CreateVerticalLayoutGroup();
            CreateHorizontalRegion(tab, content);
            MoveContentsToChildRegion(tab);
        }

        private void SplitBottom(Tab tab, Transform content)
        {
            CreateVerticalLayoutGroup();
            MoveContentsToChildRegion(tab);
            CreateHorizontalRegion(tab, content);
        }

        private void SplitLeft(Tab tab, Transform content)
        {
            CreateHorizontalLayoutGroup();
            CreateVerticalRegion(tab, content);
            MoveContentsToChildRegion(tab);
        }

        private void SplitRight(Tab tab, Transform content)
        {
            CreateHorizontalLayoutGroup();
            MoveContentsToChildRegion(tab);
            CreateVerticalRegion(tab, content);
        }

        private void CreateVerticalLayoutGroup()
        {
            VerticalLayoutGroup lg = m_childrenPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            lg.childControlHeight = true;
            lg.childControlWidth = true;
            lg.childForceExpandHeight = false;
            lg.childForceExpandWidth = true;
        }

        private void CreateHorizontalLayoutGroup()
        {
            HorizontalLayoutGroup lg = m_childrenPanel.gameObject.AddComponent<HorizontalLayoutGroup>();
            lg.childControlHeight = true;
            lg.childControlWidth = true;
            lg.childForceExpandHeight = true;
            lg.childForceExpandWidth = false;
        }

        private void CreateHorizontalRegion(Tab tab, Transform content)
        {
            Rect rect = m_childrenPanel.rect;
            Region region = Instantiate(m_root.RegionPrefab, m_childrenPanel, false);
            region.name = "Region " + m_regionDebugId++;
         
            region.m_layoutElement.preferredHeight = rect.height / 3;
            region.m_layoutElement.preferredWidth = -1;
            region.m_layoutElement.flexibleHeight = 0;
            region.m_layoutElement.flexibleWidth = -1;
            region.Insert(0, tab, content);
        }

        private void CreateVerticalRegion(Tab tab, Transform content)
        {
            Rect rect = m_childrenPanel.rect;
            Region region = Instantiate(m_root.RegionPrefab, m_childrenPanel, false);
            region.name = "Region " + m_regionDebugId++;

            region.m_layoutElement.preferredWidth = rect.width / 3;
            region.m_layoutElement.preferredHeight = -1;
            region.m_layoutElement.flexibleWidth = 0;
            region.m_layoutElement.flexibleHeight = -1;
            region.Insert(0, tab, content);
        }

        private void MoveContentsToChildRegion(Tab exceptTab)
        {
            Region childRegion = Instantiate(m_root.RegionPrefab, m_childrenPanel, false);
            childRegion.name = "Region " + m_regionDebugId++;
            childRegion.m_layoutElement.preferredWidth = -1;
            childRegion.m_layoutElement.preferredHeight = -1;
            childRegion.m_layoutElement.flexibleWidth = 1;
            childRegion.m_layoutElement.flexibleHeight = 1;
            
            Transform[] contents = m_contentPanel.OfType<Transform>().ToArray();
            Tab[] tabs = m_tabPanel.transform.OfType<Transform>().Select(t => t.GetComponent<Tab>()).ToArray();
            for (int i = 0; i < tabs.Length; ++i)
            {                
                Tab tab = tabs[i];
                if(tab == exceptTab)
                {
                    continue;
                }
                Unsubscribe(tab, this);
                Subscribe(tab, childRegion);
                
                tab.transform.SetParent(childRegion.m_tabPanel.transform, false);
                tab.ToggleGroup = childRegion.m_tabPanel;

                contents[i].transform.SetParent(childRegion.m_contentPanel, false);
            }

            Tab selectTab = tabs.Where(tab => tab != exceptTab).OrderBy(t => t.Index).FirstOrDefault();
            if(selectTab != null)
            {
                selectTab.IsOn = true;
            }
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            Debug.Log("Region OnPointerDown Handler");
            IsSelected = true;
        }

        private void OnTabToggle(Tab sender, bool isOn)
        {
            Transform content = m_contentPanel.GetChild(sender.Index);
            content.gameObject.SetActive(isOn);
            m_activeTab = sender;
        }

        private void OnTabPointerDown(Tab sender, PointerEventData args)
        {
            IsSelected = true;
        }

        private void OnTabClose(Tab sender)
        {
            RemoveAt(sender.Index);
        }

        private void OnTabBeginDrag(Tab tab, PointerEventData args)
        {
            m_pointerOverTab = null;
            m_isFree = false;
            m_splitType = RegionSplitType.None;
            m_isDraggingOutside = false;
            m_beginDragRegion = m_pointerOverRegion = tab.GetComponentInParent<Region>();

            BeginDragInsideOfTabPanel(this, tab, args);
        }

        private void OnTabDrag(Tab tab, PointerEventData args)
        {
            Region region = GetRegion(args);
            bool isRegionChanged = false;
            if(region != m_pointerOverRegion)
            {
                isRegionChanged = true;
                m_pointerOverRegion = region;
                tab.transform.SetParent(region.m_tabPanel.transform, false);
            }

            Vector2 localPoint;
            RectTransform tabPanelRT = (RectTransform)region.m_tabPanel.transform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(tabPanelRT, args.position, args.pressEventCamera, out localPoint))
            {
                Rect tabPanelRect = tabPanelRT.rect;
                tabPanelRect.yMax *= 2.0f;
                tabPanelRect.yMin *= 2.0f;
                if (tabPanelRect.Contains(localPoint))
                {
                    if (m_isDraggingOutside || isRegionChanged)
                    {
                        m_isDraggingOutside = false;
                        BeginDragInsideOfTabPanel(region, tab, args);
                    }

                    DragInsideOfTabPanel(region, tab, args, localPoint, tabPanelRT);
                }
                else
                {
                    if(!m_isDraggingOutside)
                    {
                        m_isDraggingOutside = true;
                        BeginDragOutsideOfTabPanel(tab, args);
                        SetMaxTabSiblingIndex(tab);
                    }

                    
                    DragOutsideOfTabPanel(region, tab, args, isRegionChanged);
                }
            }
            else
            {
                if (!m_isDraggingOutside)
                {
                    m_isDraggingOutside = true;
                    BeginDragOutsideOfTabPanel(tab, args);
                    SetMaxTabSiblingIndex(tab);
                }

                DragOutsideOfTabPanel(region, tab, args, isRegionChanged);
            }
        }

        private void BeginDragInsideOfTabPanel(Region region, Tab tab, PointerEventData args)
        {
            Vector2 tabScreenPos = RectTransformUtility.WorldToScreenPoint(args.pressEventCamera, tab.transform.position);
            RectTransform tabPanelRT = (RectTransform)region.m_tabPanel.transform;
            Debug.Assert(RectTransformUtility.ScreenPointToLocalPointInRectangle(tabPanelRT, tabScreenPos, args.pressEventCamera, out m_beginDragTabPos));

            //Region oldRegion = tab.GetComponentInParent<Region>();

            //Transform content = oldRegion.m_contentPanel.GetChild(tab.Index);
            //content.SetParent(region.m_contentPanel, false);

            tab.transform.SetParent(region.m_tabPanel.transform, false);
        }

        private void DragInsideOfTabPanel(Region region, Tab tab, PointerEventData args, Vector2 localPoint, RectTransform tabPanelRT)
        {
            localPoint.y = m_beginDragTabPos.y;
            tab.PreviewPosition = tabPanelRT.TransformPoint(localPoint);

            RectTransform tabTransform = (RectTransform)tab.transform;
            tab.PreviewContentSize = tabTransform.rect.size;

            SetTabSiblingIndex(region, tab, args, localPoint);

            m_isFree = false;
            m_splitType = RegionSplitType.None;
            tab.IsPreviewContentActive = false;
        }

        private void BeginDragOutsideOfTabPanel(Tab tab, PointerEventData args)
        {

        }

        private void DragOutsideOfTabPanel(Region region, Tab tab, PointerEventData args, bool isRegionChanged)
        {
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(region.m_previewPanel, args.position, args.pressEventCamera, out localPoint))
            {
                RegionSplitType splitType = RegionSplitType.None;
                bool isFree = false;

                float w = region.m_previewPanel.rect.width;
                float h = region.m_previewPanel.rect.height;

                localPoint.y = -localPoint.y;

                if (w / 3 <= localPoint.x && localPoint.x <= 2 * w / 3 && 
                    h / 3 <= localPoint.y && localPoint.y <= 2 * h / 3 ||
                    m_beginDragRegion == region && m_beginDragRegion.m_contentPanel.transform.childCount == 1)
                {
                    Vector3 worldPoint;
                    if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_root.RootRegion.m_previewPanel, args.position, args.pressEventCamera, out worldPoint))
                    {
                        //floating window 
                        tab.PreviewPosition = worldPoint;
                        tab.IsPreviewContentActive = true;

                        isFree = true;
                        splitType = RegionSplitType.None;
                    }
                }
                else
                {
                    isFree = false;

                    float x = localPoint.x;
                    float y = (w / h) * localPoint.y;
                    float wy = w - y;

                    if(x > y && x > wy)
                    {
                        splitType = RegionSplitType.Right;
                    }
                    else if(x < y && x < wy)
                    {
                        splitType = RegionSplitType.Left;
                    }
                    else if (x < y && x > wy)
                    {
                        splitType = RegionSplitType.Bottom;
                    }
                    else
                    {
                        splitType = RegionSplitType.Top;
                    }
                }

                if(m_isFree != isFree || m_splitType != splitType || isRegionChanged)
                {
                    tab.IsPreviewContentActive = true;
                    m_isFree = isFree;
                    m_splitType = splitType;

                    if(m_isFree)
                    {
                        tab.PreviewContentSize = new Vector2(w / 3, h / 3);
                    }
                    else
                    {
                        RectTransform tabTransform = (RectTransform)tab.transform;
                        Vector2 pivot =
                            (splitType == RegionSplitType.Left || splitType == RegionSplitType.Right) ?
                                Vector2.Scale(new Vector2(Mathf.Min(w / 3, tabTransform.rect.width), -tabTransform.rect.height), tabTransform.pivot) :
                                Vector2.Scale(new Vector2(Mathf.Min(w, tabTransform.rect.width), -tabTransform.rect.height), tabTransform.pivot);
                        switch (splitType)
                        {
                            case RegionSplitType.Top:
                                tab.PreviewPosition = region.m_previewPanel.TransformPoint(pivot);
                                tab.PreviewContentSize = new Vector2(w, h / 3);
                                break;
                            case RegionSplitType.Bottom:
                                tab.PreviewPosition = region.m_previewPanel.TransformPoint(pivot - new Vector2(0, 2 * h / 3));
                                tab.PreviewContentSize = new Vector2(w, h / 3);
                                break;
                            case RegionSplitType.Left:
                                tab.PreviewPosition = region.m_previewPanel.TransformPoint(pivot + new Vector2(0, 0));
                                tab.PreviewContentSize = new Vector2(w / 3, h);
                                break;
                            case RegionSplitType.Right:
                                tab.PreviewPosition = region.m_previewPanel.TransformPoint(pivot + new Vector2(2 * w / 3, 0));
                                tab.PreviewContentSize = new Vector2(w / 3, h);
                                break;
                        }
                    }
                }
            }
        }

        private void SetTabSiblingIndex(Region region, Tab tab, PointerEventData args, Vector2 localPoint)
        {
            //Transform contentTransform = region.m_contentPanel.GetChild(tab.Index);
            foreach (RectTransform childRT in region.m_tabPanel.transform)
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(childRT, args.position, args.pressEventCamera, out localPoint))
                {
                    if (childRT.rect.Contains(localPoint))
                    {
                        if (childRT != m_pointerOverTab)
                        {
                            m_pointerOverTab = childRT;

                            Tab pointerOverTab = m_pointerOverTab.GetComponent<Tab>();
                            int index = pointerOverTab.Index;
                            tab.Index = index;
                            tab.IsPreviewContentActive = false;

                            //contentTransform.SetSiblingIndex(index);
                        }
                    }
                }
            }
        }

        private void SetMaxTabSiblingIndex(Tab tab)
        {
            Region region = tab.GetComponentInParent<Region>();

            //Transform contentTransform = region.m_contentPanel.GetChild(tab.Index);
            tab.transform.SetSiblingIndex(region.m_tabPanel.transform.childCount - 1);
            //contentTransform.SetSiblingIndex(region.m_tabPanel.transform.childCount - 1);
            m_pointerOverTab = null;
        }

        private void OnTabEndDrag(Tab sender, PointerEventData args)
        {
            if (m_isFree)
            {
                Debug.Log("Set free");
            }
            else
            {
                if(m_splitType != RegionSplitType.None)
                {
                    Move(sender.Index, 0, this, m_splitType);
                }
            }

            m_beginDragRegion = null;
            m_pointerOverTab = null;
            m_isFree = false;
            m_splitType = RegionSplitType.None;
            m_isDraggingOutside = false;
        }

        private List<RaycastResult> m_raycastResults = new List<RaycastResult>();
        private Region GetRegion(PointerEventData args)
        {
            m_raycastResults.Clear();
            m_root.Raycaster.Raycast(args, m_raycastResults);
            Region region = null;
            for(int i = 0; i < m_raycastResults.Count; ++i)
            {
                RaycastResult result = m_raycastResults[i];
                region = result.gameObject.GetComponent<Region>();
                if(region != null)
                {
                    break;
                }
            }

            if(region == null)
            {
                region = m_root.RootRegion;
                Debug.Log("Root Region");
            }
            
            return region;
        }
    }
}

