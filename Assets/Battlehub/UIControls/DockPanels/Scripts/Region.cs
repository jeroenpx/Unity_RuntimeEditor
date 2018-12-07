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

    public class Region : MonoBehaviour, IPointerDownHandler, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private static int m_regionDebugId;
        public static event RegionEventArgs Selected;
        public static event RegionEventArgs Unselected;

        [SerializeField]
        private LayoutElement m_layoutElement;

        [SerializeField]
        private ToggleGroup m_tabPanel;

        [SerializeField]
        private RectTransform m_content;

        [SerializeField]
        private Transform m_contentPanel;

        [SerializeField]
        private RectTransform m_childrenPanel;

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
            set
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
        private Transform m_dragContent;
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
            if(transform.parent != null)
            {
                Region parent = transform.parent.GetComponentInParent<Region>();
                if (parent != null)
                {
                    parent.DestroyChildRegion(transform.GetSiblingIndex());
                    UpdateResizers();
                }
            }

            if(m_root != null)
            {
                m_root.CursorHelper.ResetCursor(this);
            }
        }

        private void Subscribe(Tab tab, Region region)
        {
            tab.Toggle += region.OnTabToggle;
            tab.PointerDown += region.OnTabPointerDown;
            tab.Close += region.OnTabClose;
            tab.InitializePotentialDrag += region.OnTabInitializePotentialDrag;
            tab.BeginDrag += region.OnTabBeginDrag;
            tab.Drag += region.OnTabDrag;
            tab.EndDrag += region.OnTabEndDrag;
        }

        private void Unsubscribe(Tab tab, Region region)
        {
            tab.Toggle -= region.OnTabToggle;
            tab.PointerDown -= region.OnTabPointerDown;
            tab.Close -= region.OnTabClose;
            tab.InitializePotentialDrag -= region.OnTabInitializePotentialDrag;
            tab.BeginDrag -= region.OnTabBeginDrag;
            tab.Drag -= region.OnTabDrag;
            tab.EndDrag -= region.OnTabEndDrag;

            if (tab == m_activeTab)
            {
                m_activeTab = null;
            }
        }

        public Transform GetDragRegion()
        {
            Transform parent = transform;

            while (parent != null)
            {
                if (parent.parent == m_root.Free)
                {
                    return parent;
                }

                parent = parent.parent;
            }
            return null;
        }

        public bool IsFree()
        {
            if(transform.parent != null)
            {
                if(transform.parent.GetComponentInParent<Region>() != null)
                {
                    return false;
                }
            }

            Transform parent = transform;
            while (parent != null)
            {
                if (parent.parent == m_root.Free)
                {
                    return true;
                }

                parent = parent.parent;
            }

            return false;
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
            UpdateResizers();
        }

        public void RemoveAt(int index)
        {
            if(index < 0 || m_tabPanel.transform.childCount <= index)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (m_tabPanel.transform.childCount == 1 && this != m_root.RootRegion)
            {
                Destroy(gameObject);
            }
            else
            {
                Tab tab = m_tabPanel.transform.GetChild(index).GetComponent<Tab>();
                if(index == ActiveTabIndex)
                {
                    if(m_tabPanel.transform.childCount > 1)
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
                }

                Unsubscribe(tab, this);
              
                Transform content = m_contentPanel.transform.GetChild(index);

                Destroy(tab.gameObject);
                Destroy(content.gameObject);
            }

            UpdateResizers();
            m_root.CursorHelper.ResetCursor(this);
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
            Transform content = m_contentPanel.transform.GetChild(index);
            Move(tab, content, targetIndex, targetRegion, targetSplitType);
            UpdateResizers();
        }

        private void Move(Tab tab, Transform content, int targetIndex, Region targetRegion, RegionSplitType targetSplitType = RegionSplitType.None)
        {
            if (m_childrenPanel.childCount > 0)
            {
                throw new InvalidOperationException("Unable to Remove content. Region has children and is not a \"leaf\" region.");
            }

            Debug.Assert(content.parent == m_contentPanel);

            Unsubscribe(tab, this);
            
            bool destroy = m_contentPanel.childCount == 1 && targetRegion != this;
            
            targetRegion.Insert(targetIndex, tab, content, targetSplitType);

            if (destroy)
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

            if (m_childrenPanel.childCount == 2)
            {
                index = (index + 1) % 2;
                Region childRegion = m_childrenPanel.GetChild(index).GetComponent<Region>();
                if (childRegion != null && childRegion.m_contentPanel != null && childRegion.m_tabPanel != null && childRegion.m_tabPanel.transform != null)
                {
                    if(childRegion.m_contentPanel.childCount == 0)
                    {
                        childRegion.MoveChildrenToParentRegion(this);

                    }
                    else
                    {
                        childRegion.MoveContentsToRegion(this);

                        HorizontalOrVerticalLayoutGroup layoutGroup = m_childrenPanel.GetComponent<HorizontalOrVerticalLayoutGroup>();
                        if (layoutGroup != null)
                        {
                            Destroy(layoutGroup);
                        }
                    }
                    
                    Destroy(childRegion.gameObject);
                }
            }
        }

        private void Insert(int index, Tab tab, Transform content, RegionSplitType splitType = RegionSplitType.None)
        {
            switch (splitType)
            {
                case RegionSplitType.None:
                    if (m_childrenPanel.childCount > 0)
                    {
                        throw new InvalidOperationException("Unable to Add content. Region has children and is not a \"leaf\" region.");
                    }
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
            content.SetSiblingIndex(index);

            tab.transform.SetParent(m_tabPanel.transform, false);
            tab.transform.SetSiblingIndex(index);
            tab.ToggleGroup = m_tabPanel;

            Subscribe(tab, this);
            Tab[] tabs = m_tabPanel.transform.OfType<Transform>().Select(t => t.GetComponent<Tab>()).ToArray();
            for(int i = 0; i < tabs.Length; ++i)
            {
                tabs[i].IsOn = false;
            }
            tab.IsOn = true;
        }

        private void SplitTop(Tab tab, Transform content)
        {
            tab.transform.SetParent(m_root.transform);
            content.transform.SetParent(m_root.transform);

            MoveContentsToChildRegion();
            Region region = CreateHorizontalRegion(tab, content);

            CreateVerticalLayoutGroup(this);

            region.transform.SetSiblingIndex(0);

            Stretch(content);
        }

        private void SplitBottom(Tab tab, Transform content)
        {
            tab.transform.SetParent(m_root.transform);
            content.transform.SetParent(m_root.transform);

            MoveContentsToChildRegion();

            Region region = CreateHorizontalRegion(tab, content);

            CreateVerticalLayoutGroup(this);

            region.transform.SetSiblingIndex(1);

            Stretch(content);
        }

        private void SplitLeft(Tab tab, Transform content)
        {
            tab.transform.SetParent(m_root.transform);
            content.transform.SetParent(m_root.transform);

            MoveContentsToChildRegion();

            Region region = CreateVerticalRegion(tab, content);

            CreateHorizontalLayoutGroup(this);

            region.transform.SetSiblingIndex(0);

            Stretch(content);
        }

        private void SplitRight(Tab tab, Transform content)
        {
            tab.transform.SetParent(m_root.transform);
            content.transform.SetParent(m_root.transform);

            MoveContentsToChildRegion();

            Region region = CreateVerticalRegion(tab, content);

            CreateHorizontalLayoutGroup(this);

            region.transform.SetSiblingIndex(1);

            Stretch(content);
        }

        private static void CreateVerticalLayoutGroup(Region region)
        {
            HorizontalLayoutGroup horizontalLg = region.m_childrenPanel.GetComponent<HorizontalLayoutGroup>();
            if(horizontalLg != null)
            {
                DestroyImmediate(horizontalLg);
            }

            VerticalLayoutGroup lg = region.m_childrenPanel.GetComponent<VerticalLayoutGroup>();
            if(lg == null)
            {
                lg = region.m_childrenPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            }
             
            lg.childControlHeight = true;
            lg.childControlWidth = true;
            lg.childForceExpandHeight = false;
            lg.childForceExpandWidth = true;
        }

        private static void CreateHorizontalLayoutGroup(Region region)
        {
            VerticalLayoutGroup verticalLg = region.m_childrenPanel.GetComponent<VerticalLayoutGroup>();
            if (verticalLg != null)
            {
                DestroyImmediate(verticalLg);
            }

            HorizontalLayoutGroup lg = region.m_childrenPanel.GetComponent<HorizontalLayoutGroup>();
            if (lg == null)
            {
                lg = region.m_childrenPanel.gameObject.AddComponent<HorizontalLayoutGroup>();
            }

            lg.childControlHeight = true;
            lg.childControlWidth = true;
            lg.childForceExpandHeight = true;
            lg.childForceExpandWidth = false;
        }

        private Region CreateHorizontalRegion(Tab tab, Transform content)
        {
            Rect rect = m_childrenPanel.rect;
            Region region = Instantiate(m_root.RegionPrefab, m_childrenPanel, false);
            region.name = "Region " + m_regionDebugId++;

            region.m_layoutElement.preferredHeight = -1;// rect.height / 3;
            region.m_layoutElement.preferredWidth = -1;
            region.m_layoutElement.flexibleHeight = 0.3f;
            region.m_layoutElement.flexibleWidth = -1;
            region.Insert(0, tab, content);
            return region;
        }

        private Region CreateVerticalRegion(Tab tab, Transform content)
        {
            Rect rect = m_childrenPanel.rect;
            Region region = Instantiate(m_root.RegionPrefab, m_childrenPanel, false);
            region.name = "Region " + m_regionDebugId++;

            region.m_layoutElement.preferredWidth = -1;// rect.width / 3;
            region.m_layoutElement.preferredHeight = -1;
            region.m_layoutElement.flexibleWidth = 0.3f;
            region.m_layoutElement.flexibleHeight = -1;
            region.Insert(0, tab, content);
            return region;
        }

        private void MoveContentsToChildRegion()
        {
            Region region = Instantiate(m_root.RegionPrefab, m_childrenPanel, false);
            region.name = "Region " + m_regionDebugId++;
            region.m_layoutElement.preferredWidth = -1;
            region.m_layoutElement.preferredHeight = -1;
            region.m_layoutElement.flexibleWidth = 0.7f;
            region.m_layoutElement.flexibleHeight = 0.7f;

            if (m_contentPanel.childCount == 0)
            {
                if (m_childrenPanel.GetComponent<HorizontalLayoutGroup>())
                {
                    CreateHorizontalLayoutGroup(region);
                }
                else
                {
                    CreateVerticalLayoutGroup(region);
                }

                MoveChildrenToRegion(region);
            }
            else
            {
                Tab[] tabs = MoveContentsToRegion(region);

                Tab selectTab = tabs.OrderBy(t => t.Index).FirstOrDefault();
                if (selectTab != null)
                {
                    selectTab.IsOn = true;
                }
            }
        }

        private void MoveChildrenToParentRegion(Region parentRegion)
        {
            MoveChildrenToRegion(parentRegion);

            bool isHorizontalLayout = false;
            if (m_childrenPanel.GetComponent<HorizontalLayoutGroup>())
            {
                isHorizontalLayout = true;
            }

            bool isParentHorizontalLayout = false;
            if (parentRegion.m_childrenPanel.GetComponent<HorizontalLayoutGroup>())
            {
                isParentHorizontalLayout = true;
            }

            if (isHorizontalLayout != isParentHorizontalLayout)
            {
                HorizontalOrVerticalLayoutGroup layoutGroup = parentRegion.m_childrenPanel.GetComponent<HorizontalOrVerticalLayoutGroup>();
                if (layoutGroup != null)
                {
                    DestroyImmediate(layoutGroup);
                }

                if (isHorizontalLayout)
                {
                    CreateHorizontalLayoutGroup(parentRegion);
                }
                else
                {
                    CreateVerticalLayoutGroup(parentRegion);
                }
            }
        }

        private void MoveChildrenToRegion(Region region)
        {
            List<Transform> childrenList = new List<Transform>();
            for (int i = m_childrenPanel.childCount - 1; i >= 0; i--)
            {
                Transform child = m_childrenPanel.GetChild(i);
                childrenList.Add(child);
            }

            for (int i = childrenList.Count - 1; i >= 0; i--)
            {
                childrenList[i].SetParent(region.m_childrenPanel, false);
            }
        }

        private Tab[] MoveContentsToRegion(Region region)
        {
            Transform[] contents = m_contentPanel.OfType<Transform>().ToArray();
            Tab[] tabs = m_tabPanel.transform.OfType<Transform>().Select(t => t.GetComponent<Tab>()).ToArray();
            for (int i = 0; i < tabs.Length; ++i)
            {
                Tab tab = tabs[i];
                Unsubscribe(tab, this);

                tab.transform.SetParent(region.m_tabPanel.transform, false);
                contents[i].transform.SetParent(region.m_contentPanel, false);

                Subscribe(tab, region);
                
                tab.ToggleGroup = region.m_tabPanel;
            }

            return tabs;
        }

        private void Stretch(Transform transform)
        {
            RectTransform rt = (RectTransform)transform;
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
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

        private void OnTabInitializePotentialDrag(Tab tab, PointerEventData args)
        {
            m_root.CursorHelper.SetCursor(this, null);
        }

        private void OnTabBeginDrag(Tab tab, PointerEventData args)
        {
            m_pointerOverTab = null;
            m_isFree = false;
            m_splitType = RegionSplitType.None;
            m_isDraggingOutside = false;
            m_beginDragRegion = m_pointerOverRegion = tab.GetComponentInParent<Region>();
            m_dragContent = m_contentPanel.GetChild(tab.Index);

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
                //tabPanelRect.yMax *= 2.0f;
                //tabPanelRect.yMin *= 2.0f;
                if (tabPanelRect.Contains(localPoint) || region == m_root.RootRegion && region.m_contentPanel.childCount == 0 && region.m_childrenPanel.childCount == 0)
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
            RectTransform contentRT = region.m_content;
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(contentRT, args.position, args.pressEventCamera, out localPoint))
            {
                RegionSplitType splitType = RegionSplitType.None;
                bool isFree = false;

                
                float w = contentRT.rect.width;
                float h = contentRT.rect.height;

                localPoint.y = -localPoint.y;


                if (w / 3 <= localPoint.x && localPoint.x <= 2 * w / 3 && 
                    h / 3 <= localPoint.y && localPoint.y <= 2 * h / 3 ||
                    m_beginDragRegion == region && m_beginDragRegion.m_contentPanel.transform.childCount == 1)
                {
                    Vector3 worldPoint;
                    if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_root.Preview, args.position, args.pressEventCamera, out worldPoint))
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
                                tab.PreviewPosition = contentRT.TransformPoint(pivot);
                                tab.PreviewContentSize = new Vector2(w, h / 3);
                                break;
                            case RegionSplitType.Bottom:
                                tab.PreviewPosition = contentRT.TransformPoint(pivot - new Vector2(0, 2 * h / 3));
                                tab.PreviewContentSize = new Vector2(w, h / 3);
                                break;
                            case RegionSplitType.Left:
                                tab.PreviewPosition = contentRT.TransformPoint(pivot + new Vector2(0, 0));
                                tab.PreviewContentSize = new Vector2(w / 3, h);
                                break;
                            case RegionSplitType.Right:
                                tab.PreviewPosition = contentRT.TransformPoint(pivot + new Vector2(2 * w / 3, 0));
                                tab.PreviewContentSize = new Vector2(w / 3, h);
                                break;
                        }
                    }
                }
            }
        }

        private void SetTabSiblingIndex(Region region, Tab tab, PointerEventData args, Vector2 localPoint)
        {
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
                        }
                    }
                }
            }
        }

        private void SetMaxTabSiblingIndex(Tab tab)
        {
            Region region = tab.GetComponentInParent<Region>();
            tab.transform.SetSiblingIndex(region.m_tabPanel.transform.childCount - 1);
            m_pointerOverTab = null;
        }

        private void OnTabEndDrag(Tab tab, PointerEventData args)
        {
            m_root.CursorHelper.ResetCursor(this);

            if (m_isFree)
            {
                Region freeRegion = Instantiate(m_root.RegionPrefab, m_root.Free);
                freeRegion.name = "Region " + m_regionDebugId++;

                Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(args.pressEventCamera, tab.PreviewPosition);
                Vector3 worldPos;
                
                RectTransform rt = (RectTransform)freeRegion.transform;
                RectTransform beginRt = (RectTransform)m_beginDragRegion.transform;
                Vector2 size = beginRt.rect.size;
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2((tab.PreviewHeaderSize.x * 0.5f) / size.x, 1 - (tab.PreviewHeaderSize.y * 0.5f) / size.y);
                rt.sizeDelta = size;

                Debug.Assert(RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, args.position, args.pressEventCamera, out worldPos));
                freeRegion.transform.position = worldPos;
                
                Unsubscribe(tab, this);
                freeRegion.Insert(0, tab, m_dragContent);

                if(m_contentPanel.childCount == 0 && this != m_root.RootRegion)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                Unsubscribe(tab, m_beginDragRegion);
                Move(tab, m_dragContent, tab.Index, tab.GetComponentInParent<Region>(), m_splitType);
            }

            IEnumerable<Tab> children = m_beginDragRegion.m_tabPanel.transform.OfType<Transform>().Select(t => t.GetComponent<Tab>());
            if (!children.Where(t => t.IsOn).Any())
            {
                Tab firstTab = children.FirstOrDefault();
                if (firstTab != null)
                {
                    firstTab.IsOn = true;
                }
            }

            UpdateResizers();

            m_dragContent = null;
            m_beginDragRegion = null;
            m_pointerOverTab = null;
            m_isFree = false;
            m_splitType = RegionSplitType.None;
            m_isDraggingOutside = false;
        }

        private void UpdateResizers()
        {
            if(m_root == null || m_root.RootRegion == null)
            {
                return;
            }
            Resizer[] resizers = m_root.RootRegion.GetComponentsInChildren<Resizer>();
            for(int i = 0; i < resizers.Length; ++i)
            {
                resizers[i].UpdateState();
            }
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

        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }

        private bool m_isDragging;
        private Vector3 m_prevPoint;
        private Transform m_dragRegion;
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            m_root.CursorHelper.SetCursor(this, null);
            m_dragRegion = GetDragRegion();

            if (m_dragRegion)
            {
                m_isDragging = RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)m_dragRegion, eventData.position, eventData.pressEventCamera, out m_prevPoint);
                if (m_isDragging)
                {
                    if (IsFree())
                    {
                        transform.SetSiblingIndex(m_root.Preview.childCount - 1);
                    }
                }
            }
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint((RectTransform)Root.transform, eventData.position, eventData.pressEventCamera))
            {
                return;
            }

            if (m_isDragging)
            {
                Vector3 point;
                if(RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)m_dragRegion, eventData.position, eventData.pressEventCamera, out point))
                {
                    Vector3 delta = point - m_prevPoint;
                    m_prevPoint = point;
                    m_dragRegion.position += delta;
                }
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            m_root.CursorHelper.ResetCursor(this);
            m_dragRegion = null;
            m_isDragging = false;   
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if(IsFree())
            {
                transform.SetSiblingIndex(m_root.Preview.childCount - 1);
            }
            IsSelected = true;
        }
    }
}

