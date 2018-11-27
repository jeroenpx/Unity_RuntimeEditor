using System;
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
        }

        private void Unsubscribe(Tab tab, Region region)
        {
            tab.Toggle -= region.OnTabToggle;
            tab.PointerDown -= region.OnTabPointerDown;
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
                    if (index < m_tabPanel.transform.childCount - 1)
                    {
                        Tab nextTab = m_tabPanel.transform.GetChild(index + 1).GetComponent<Tab>();
                        nextTab.IsOn = true;
                    }
                    else
                    {
                        Tab nextTab = m_tabPanel.transform.GetChild(index - 1).GetComponent<Tab>();
                        nextTab.IsOn = true;
                    }
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

            Transform content = m_contentPanel.transform.GetChild(index);
            targetRegion.Insert(targetIndex, tab, content, targetSplitType);

            if(m_tabPanel.transform.childCount == 0)
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

            tab.transform.SetParent(m_tabPanel.transform);
            tab.transform.SetSiblingIndex(index);
            tab.ToggleGroup = m_tabPanel;

            Subscribe(tab, this);
            tab.IsOn = true;
        }

        private void SplitTop(Tab tab, Transform content)
        {
            CreateVerticalLayoutGroup();
            CreateVerticalRegion(tab, content);
            MoveContentsToChildRegion();
        }

        private void SplitBottom(Tab tab, Transform content)
        {
            CreateVerticalLayoutGroup();
            MoveContentsToChildRegion();
            CreateVerticalRegion(tab, content);
        }

        private void SplitLeft(Tab tab, Transform content)
        {
            CreateHorizontalLayoutGroup();
            CreateHorizontalRegion(tab, content);
            MoveContentsToChildRegion();
        }

        private void SplitRight(Tab tab, Transform content)
        {
            CreateHorizontalLayoutGroup();
            MoveContentsToChildRegion();
            CreateHorizontalRegion(tab, content);
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

        private void CreateVerticalRegion(Tab tab, Transform content)
        {
            Rect rect = m_childrenPanel.rect;
            Region region = Instantiate(m_root.RegionPrefab, m_childrenPanel, false);
         
            region.m_layoutElement.preferredHeight = rect.height / 3;
            region.m_layoutElement.preferredWidth = -1;
            region.m_layoutElement.flexibleHeight = 0;
            region.m_layoutElement.flexibleWidth = -1;
            region.Insert(0, tab, content);
        }

        private void CreateHorizontalRegion(Tab tab, Transform content)
        {
            Rect rect = m_childrenPanel.rect;
            Region region = Instantiate(m_root.RegionPrefab, m_childrenPanel, false);
          
            region.m_layoutElement.preferredWidth = rect.width / 3;
            region.m_layoutElement.preferredHeight = -1;
            region.m_layoutElement.flexibleWidth = 0;
            region.m_layoutElement.flexibleHeight = -1;
            region.Insert(0, tab, content);
        }

        private void MoveContentsToChildRegion()
        {
            Region childRegion = Instantiate(m_root.RegionPrefab, m_childrenPanel, false);
            childRegion.m_layoutElement.preferredWidth = -1;
            childRegion.m_layoutElement.preferredHeight = -1;
            childRegion.m_layoutElement.flexibleWidth = 1;
            childRegion.m_layoutElement.flexibleHeight = 1;
            
            Transform[] contents = m_contentPanel.OfType<Transform>().ToArray();
            Tab[] tabs = m_tabPanel.transform.OfType<Transform>().Select(t => t.GetComponent<Tab>()).ToArray();
            for (int i = 0; i < tabs.Length; ++i)
            {                
                Tab tab = tabs[i];
                Unsubscribe(tab, this);
                Subscribe(tab, childRegion);
                
                tab.transform.SetParent(childRegion.m_tabPanel.transform, false);
                tab.ToggleGroup = childRegion.m_tabPanel;

                contents[i].transform.SetParent(childRegion.m_contentPanel, false);
            }   
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

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log("Region OnPointerDown Handler");
            IsSelected = true;
        }
    }
}

