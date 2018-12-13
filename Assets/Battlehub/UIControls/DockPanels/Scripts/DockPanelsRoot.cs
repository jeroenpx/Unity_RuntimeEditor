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

        [SerializeField]
        private GraphicRaycaster m_raycaster;
        public GraphicRaycaster Raycaster
        {
            get { return m_raycaster; }
        }

        [SerializeField]
        private Region m_regionPrefab;
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
        private Region m_rootRegion;
        public Region RootRegion
        {
            get { return m_rootRegion; }
        }

        [SerializeField]
        private Transform m_docked;    
        public Transform Docked
        {
            get { return m_docked; }
        }

        [SerializeField]
        private Transform m_free;
        public Transform Free
        {
            get { return m_free; }
        }

        [SerializeField]
        private RectTransform m_preview;
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

            if(m_rootRegion == null)
            {
                m_rootRegion = GetComponentInChildren<Region>();
            }

            if(m_rootRegion == null)
            {
                m_rootRegion = Instantiate(m_regionPrefab, m_docked);
                m_rootRegion.name = "Root Region";
            }

            Region.Selected += OnRegionSelected;
            Region.Unselected += OnRegionUnselected;
            Region.TabActivated += OnTabActivated;
            Region.TabDeactivated += OnTabDeactivated;
            Region.TabClosed += OnTabClosed;
        }

        private void OnDestroy()
        {
            Region.Selected -= OnRegionSelected;
            Region.Unselected -= OnRegionUnselected;
            Region.TabActivated -= OnTabActivated;
            Region.TabDeactivated -= OnTabDeactivated;
            Region.TabClosed -= OnTabClosed;
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


    }

}
