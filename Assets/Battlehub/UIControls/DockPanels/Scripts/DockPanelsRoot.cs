using UnityEngine;

namespace Battlehub.UIControls.DockPanels
{
    public class DockPanelsRoot : MonoBehaviour
    {
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

        private void Awake()
        {
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
