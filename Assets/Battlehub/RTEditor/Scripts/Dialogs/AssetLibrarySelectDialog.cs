using Battlehub.RTCommon;
using Battlehub.RTSaveLoad2.Interface;
using Battlehub.UIControls;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class AssetLibrarySelectDialog : MonoBehaviour
    {
        [SerializeField]
        private VirtualizingTreeView TreeViewPrefab;
        [SerializeField]
        private Sprite AssetLibraryIcon;

        private PopupWindow m_parentPopup;
        private VirtualizingTreeView m_treeView;


        private IProject m_project;

        public string SelectedAssetLibrary
        {
            get { return m_treeView.SelectedItem as string; }
        }

        private void Start()
        {
            m_parentPopup = GetComponentInParent<PopupWindow>();
            if (m_parentPopup != null)
            {
                m_parentPopup.OK.AddListener(OnOK);
            }

            m_treeView = GetComponentInChildren<VirtualizingTreeView>();
            if (m_treeView == null)
            {
                m_treeView = Instantiate(TreeViewPrefab);
                m_treeView.transform.SetParent(transform, false);
            }

            m_treeView.ItemDataBinding += OnItemDataBinding;
            m_treeView.ItemDoubleClick += OnItemDoubleClick;
            m_treeView.CanDrag = false;
            m_treeView.CanEdit = false;
            m_treeView.CanUnselectAll = false;

            m_project = IOC.Resolve<IProject>();
            m_treeView.Items = m_project.AssetLibraries;
            m_treeView.SelectedIndex = 0;
        }

        private void OnDestroy()
        {
            if (m_parentPopup != null)
            {
                m_parentPopup.OK.RemoveListener(OnOK);
            }

            if (m_treeView != null)
            {
                m_treeView.ItemDataBinding -= OnItemDataBinding;
                m_treeView.ItemDoubleClick -= OnItemDoubleClick;
            }
        }

        private void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            string item = e.Item as string;
            if (item != null)
            {
                Text text = e.ItemPresenter.GetComponentInChildren<Text>(true);
                text.text = item;

                Image image = e.ItemPresenter.GetComponentInChildren<Image>(true);
                image.sprite = AssetLibraryIcon;
                image.gameObject.SetActive(true);

                e.HasChildren = false;
            }
        }

        private void OnItemDoubleClick(object sender, ItemArgs e)
        {
            
        }

        private void OnOK(PopupWindowArgs args)
        {
            if (m_treeView.SelectedItem == null)
            {
                args.Cancel = true;
                return;
            }
        }
    }
}

