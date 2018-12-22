using UnityEngine;
using UnityEngine.UI;

using Battlehub.UIControls.Dialogs;

namespace Battlehub.UIControls.DockPanels
{
    public class TestCommands : MonoBehaviour
    {
        [SerializeField]
        private DialogManager m_dialog = null;

        [SerializeField]
        private Button m_addButton = null;

        [SerializeField]
        private Button m_deleteButton = null;

        [SerializeField]
        private Button m_showMsgBox = null;

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
            m_showMsgBox.onClick.AddListener(OnShowMsgBox);
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

            if(m_showMsgBox != null)
            {
                m_showMsgBox.onClick.RemoveListener(OnShowMsgBox);
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

            Dialog dlg = m_dialog.ShowDialog(m_sprite, "Popup Test", content, (sender, okArgs) =>
            {
                Debug.Log("YES");

            }, "Yes", (sender, cancelArgs) =>
            {
                Debug.Log("NO");
            }, "No");

            dlg.IsOkVisible = false;
            dlg.IsCancelVisible = false;
        }

        private void OnShowMsgBox()
        {
            m_dialog.ShowDialog(m_sprite, "Msg Test", "Is everything ok?", (sender, okArgs) =>
            {
                Debug.Log("YES");
                OnShowMsgBox();
                okArgs.Cancel = true;

            }, "Yes", (sender, cancelArgs) =>
            {
                Debug.Log("NO");
            }, "No");
        }
    }
}
