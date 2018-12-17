using Battlehub.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.UIControls.DockPanels
{
    public class DockPanelsRoot : MonoBehaviour
    {
        public event RegionEventHandler<Transform> TabActivated;
        public event RegionEventHandler<Transform> TabDeactivated;
        public event RegionEventHandler<Transform> TabClosed;

        public event RegionEventHandler RegionCreated;
        public event RegionEventHandler<int> RegionDepthChanged;
        public event RegionEventHandler RegionDestroyed;

        public event RegionEventHandler RegionBeginDrag;
        public event RegionEventHandler RegionDrag;
        public event RegionEventHandler RegionEndDrag;
        public event RegionEventHandler RegionTranformChanged;

        public event ResizerEventHandler RegionBeginResize;
        public event ResizerEventHandler RegionResize;
        public event ResizerEventHandler RegionEndResize;

        [SerializeField]
        private GraphicRaycaster m_raycaster = null;
        public GraphicRaycaster Raycaster
        {
            get { return m_raycaster; }
        }

        [SerializeField]
        private Region m_regionPrefab = null;
        public Region RegionPrefab
        {
            get { return m_regionPrefab; }
        }

        private Region m_selectedRegion;
        public Region SelectedRegion
        {
            get { return m_selectedRegion; }
        }

        [SerializeField]
        private Region m_rootRegion = null;
        public Region RootRegion
        {
            get { return m_rootRegion; }
        }

        [SerializeField]
        private Transform m_docked = null;
        public Transform Docked
        {
            get { return m_docked; }
        }

        [SerializeField]
        private Transform m_free = null;
        public Transform Free
        {
            get { return m_free; }
        }

        [SerializeField]
        private RectTransform m_preview = null;
        public RectTransform Preview
        {
            get { return m_preview; }
        }

        private CursorHelper m_cursorHelper = new CursorHelper();
        public CursorHelper CursorHelper
        {
            get { return m_cursorHelper; }
        }

        private int m_regionId;
        public int RegionId
        {
            get { return m_regionId; }
            set { m_regionId = value; }
        }

        [SerializeField]
        private bool m_allowDragOutside = false;
        public bool AllowDragOutside
        {
            get { return m_allowDragOutside; }
        }

        private void Awake()
        {
            if(m_raycaster == null)
            {
                m_raycaster = GetComponentInParent<GraphicRaycaster>();
                if(m_raycaster == null)
                {
                    Canvas canvas = GetComponentInParent<Canvas>();
                    if(canvas)
                    {
                        m_raycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
                    }
                }
            }
            Region.Selected += OnRegionSelected;
            Region.Unselected += OnRegionUnselected;
            Region.Created += OnRegionCreated;
            Region.DepthChanged += OnRegionDepthChanged;
            Region.Destroyed += OnRegionDestroyed;

            Region.BeginDrag += OnRegionBeginDrag;
            Region.Drag += OnRegionDrag;
            Region.EndDrag += OnRegionEndDrag;
            Region.TransformChanged += OnRegionTranformChanged;

            Resizer.BeginResize += OnRegionBeginResize;
            Resizer.Resize += OnRegionResize;
            Resizer.EndResize += OnRegionEndResize;

            Region.TabActivated += OnTabActivated;
            Region.TabDeactivated += OnTabDeactivated;
            Region.TabClosed += OnTabClosed;

            if (m_rootRegion == null)
            {
                m_rootRegion = GetComponentInChildren<Region>();
            }

            if(m_rootRegion == null)
            {
                m_rootRegion = Instantiate(m_regionPrefab, m_docked);
                m_rootRegion.name = "Root Region";
            }
        }

        private void OnDestroy()
        {
            Region.Selected -= OnRegionSelected;
            Region.Unselected -= OnRegionUnselected;
            Region.Created -= OnRegionCreated;
            Region.DepthChanged -= OnRegionDepthChanged;
            Region.Destroyed -= OnRegionDestroyed;

            Region.BeginDrag -= OnRegionBeginDrag;
            Region.Drag -= OnRegionDrag;
            Region.EndDrag -= OnRegionEndDrag;
            Region.TransformChanged -= OnRegionTranformChanged;

            Resizer.BeginResize -= OnRegionBeginResize;
            Resizer.Resize -= OnRegionResize;
            Resizer.EndResize -= OnRegionEndResize;

            Region.TabActivated -= OnTabActivated;
            Region.TabDeactivated -= OnTabDeactivated;
            Region.TabClosed -= OnTabClosed;
        }

        private void OnRectTransformDimensionsChange()
        {
            foreach(Transform child in Free)
            {
                Region region =  child.GetComponent<Region>();
                if(region != null)
                {
                    region.Fit();
                }
            }
        }

        private void OnTabActivated(Region region, Transform arg)
        {
            if (region.Root != this)
            {
                return;
            }

            if(TabActivated != null)
            {
                TabActivated(region, arg);
            }
        }

        private void OnTabDeactivated(Region region, Transform arg)
        {
            if (region.Root != this)
            {
                return;
            }

            if (TabDeactivated != null)
            {
                TabDeactivated(region, arg);
            }
        }

        private void OnTabClosed(Region region, Transform arg)
        {
            if(region.Root != this)
            {
                return;
            }

            if (m_selectedRegion == region)
            {
                m_selectedRegion = null;
            }

            if (TabClosed != null)
            {
                TabClosed(region, arg);
            }
        }

        private void OnRegionSelected(Region region)
        {
            if (region.Root != this)
            {
                return;
            }

            if(m_selectedRegion != null)
            {
                m_selectedRegion.IsSelected = false;
            }

            m_selectedRegion = region;
        }

        private void OnRegionUnselected(Region region)
        {
            if(region.Root != this)
            {
                return;
            }

            if(m_selectedRegion == region)
            {
                m_selectedRegion = null;
            }
        }

        private void OnRegionDepthChanged(Region region, int depth)
        {
            if(region.Root != this)
            {
                return;
            }

            if(RegionDepthChanged != null)
            {
                RegionDepthChanged(region, depth);
            }
        }

        private void OnRegionCreated(Region region)
        {
            if (region.Root != this)
            {
                return;
            }

            if(RegionCreated != null)
            {
                RegionCreated(region);
            }
        }

        private void OnRegionDestroyed(Region region)
        {
            if (region.Root != this)
            {
                return;
            }

            if(RegionDestroyed != null)
            {
                RegionDestroyed(region);
            }
        }

        private void OnRegionBeginResize(Resizer resizer, Region region)
        {
            if (region.Root != this)
            {
                return;
            }

            if(RegionBeginResize != null)
            {
                RegionBeginResize(resizer, region);
            }
        }

        private void OnRegionResize(Resizer resizer, Region region)
        {
            if (region.Root != this)
            {
                return;
            }

            if(RegionResize != null)
            {
                RegionResize(resizer, region);
            }
        }

        private void OnRegionEndResize(Resizer resizer, Region region)
        {
            if (region.Root != this)
            {
                return;
            }

            if(RegionEndResize != null)
            {
                RegionEndResize(resizer, region);
            }
        }


        private void OnRegionBeginDrag(Region region)
        {
            if (region.Root != this)
            {
                return;
            }

            if(RegionBeginDrag != null)
            {
                RegionBeginDrag(region);
            }
        }

        private void OnRegionDrag(Region region)
        {
            if (region.Root != this)
            {
                return;
            }

            if(RegionDrag != null)
            {
                RegionDrag(region);
            }
        }

        private void OnRegionEndDrag(Region region)
        {
            if (region.Root != this)
            {
                return;
            }

            if(RegionEndDrag != null)
            {
                RegionEndDrag(region);
            }
        }


        private void OnRegionTranformChanged(Region region)
        {
            if (region.Root != this)
            {
                return;
            }

            if (RegionTranformChanged != null)
            {
                RegionTranformChanged(region);
            }
        }


    }

}
