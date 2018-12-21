using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.UIControls.DockPanels
{
    public class TestCommands : MonoBehaviour
    {
        [SerializeField]
        private Button m_addButton = null;

        [SerializeField]
        private Button m_deleteButton = null;

        [SerializeField]
        private Button m_showPopup = null;

        [SerializeField]
        private DockPanelsRoot m_dockPanels = null;

        [SerializeField]
        private Sprite m_sprite = null;

        [SerializeField]
        private string m_headerText = null;

        [SerializeField]
        private Transform m_contentPrefab = null;

        [SerializeField]
        private RegionSplitType m_splitType = RegionSplitType.None;

        private int m_counter;

        private void Awake()
        {
            m_addButton.onClick.AddListener(OnAddClick);
            m_deleteButton.onClick.AddListener(OnDeleteClick);
            m_showPopup.onClick.AddListener(OnShowDialog);
        }

        private void OnDestroy()
        {
            if(m_addButton != null)
            {
                m_addButton.onClick.RemoveListener(OnAddClick);
            }
            
            if(m_deleteButton != null)
            {
                m_deleteButton.onClick.RemoveListener(OnDeleteClick);
            }

            if(m_showPopup != null)
            {
                m_showPopup.onClick.RemoveListener(OnShowDialog);
            }
        }

        private void OnAddClick()
        {
            if(m_dockPanels.SelectedRegion != null)
            {
                m_counter++;

                Transform content = Instantiate(m_contentPrefab);

                Text text = content.GetComponentInChildren<Text>();
                text.text = "Content " + m_counter;

                m_dockPanels.SelectedRegion.Add(m_sprite, m_headerText + " " + m_counter, content, false, m_splitType);
            }
        }

        private void OnDeleteClick()
        {
            if(m_dockPanels.SelectedRegion != null)
            {
                Region region = m_dockPanels.SelectedRegion;
                region.RemoveAt(region.ActiveTabIndex);
            }
        }

        private void OnShowDialog()
        {
            m_counter++;

            Transform content = Instantiate(m_contentPrefab);

            Text text = content.GetComponentInChildren<Text>();
            text.text = "Content " + m_counter;

            Transform headerContent = Instantiate(m_contentPrefab);

            Text headerText = headerContent.GetComponentInChildren<Text>();
            headerText.text = "Header " + m_counter;

            m_dockPanels.AddModalRegion(headerContent, content, 400, 200, true); 
        }
    }
}
