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

            m_selectedRegion = sender;
        }

        private void OnRegionUnselected(Region sender)
        {
            if(sender.Root != this)
            {
                return;
            }

            m_selectedRegion = sender;
        }
    }

}
