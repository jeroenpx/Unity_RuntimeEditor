using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.UIControls.DockPanels
{
    public class TestCommands : MonoBehaviour
    {
        [SerializeField]
        private Button m_addButton;

        [SerializeField]
        private Button m_deleteButton;

        [SerializeField]
        private DockPanelsRoot m_dockPanels;

        [SerializeField]
        private Sprite m_sprite;

        [SerializeField]
        private string m_headerText;

        [SerializeField]
        private Transform m_contentPrefab;

        [SerializeField]
        private RegionSplitType m_splitType;

        private int m_counter;

        private void Awake()
        {
            m_addButton.onClick.AddListener(OnAddClick);
            m_deleteButton.onClick.AddListener(OnDeleteClick);
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
        }

        private void OnAddClick()
        {
            if(m_dockPanels.SelectedRegion != null)
            {
                m_counter++;

                Transform content = Instantiate(m_contentPrefab);

                Text text = content.GetComponentInChildren<Text>();
                text.text = "Content " + m_counter;

                m_dockPanels.SelectedRegion.Add(m_sprite, m_headerText + " " + m_counter, content, m_splitType);
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
    }
}
