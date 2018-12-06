using Battlehub.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.UIControls.DockPanels
{
    public class DockPanelsRoot : MonoBehaviour
    {
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

        private void Awake()
        {
            if(m_raycaster == null)
            {
                m_raycaster = GetComponent<GraphicRaycaster>();
                if(m_raycaster == null)
                {
                    m_raycaster = gameObject.AddComponent<GraphicRaycaster>();
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
        }

        private void OnDestroy()
        {
            Region.Selected += OnRegionSelected;
            Region.Unselected += OnRegionUnselected;
        }

        private void OnRegionSelected(Region sender)
        {
            if (sender.Root != this)
            {
                return;
            }

            if(m_selectedRegion != null)
            {
                m_selectedRegion.IsSelected = false;
            }

            m_selectedRegion = sender;
        }

        private void OnRegionUnselected(Region sender)
        {
            if(sender.Root != this)
            {
                return;
            }

            if(m_selectedRegion == sender)
            {
                m_selectedRegion = null;
            }
        }
    }

}
