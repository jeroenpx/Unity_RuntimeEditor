using Battlehub.RTCommon;
using Battlehub.RTSaveLoad2.Interface;
using Battlehub.UIControls;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class AssetLibraryImportDialog : MonoBehaviour
    {
        [SerializeField]
        private VirtualizingTreeView TreeViewPrefab;
        [SerializeField]
        private Sprite AssetLibraryIcon;

        private PopupWindow m_parentPopup;
        private VirtualizingTreeView m_treeView;


        private IProject m_project;
        private IRTE m_editor;

        private string m_selectedAssetLibrary;
        public string SelectedAssetLibrary
        {
            set { m_selectedAssetLibrary = value; }
        }

        private void Start()
        {
            m_editor = IOC.Resolve<IRTE>();
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
            m_treeView.ItemExpanding += OnItemExpanding;

            m_treeView.CanDrag = false;
            m_treeView.CanEdit = false;
            m_treeView.CanUnselectAll = false;

            m_project = IOC.Resolve<IProject>();

            m_editor.IsBusy = true;
            m_project.LoadAssetLibrary(Array.IndexOf(m_project.AssetLibraries, m_selectedAssetLibrary), (error, root) =>
            {
                m_editor.IsBusy = false;

                m_treeView.Items = new[] { root };
                m_treeView.SelectedIndex = 0;
            });
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
                m_treeView.ItemExpanding -= OnItemExpanding;
            }
        }

        private void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            ProjectItem item = e.Item as ProjectItem;
            if (item != null)
            {
                Text text = e.ItemPresenter.GetComponentInChildren<Text>(true);
                text.text = item.Name;

                Image image = e.ItemPresenter.GetComponentInChildren<Image>(true);
                image.sprite = AssetLibraryIcon;
                image.gameObject.SetActive(true);

                e.HasChildren = item.Children != null && item.Children.Count > 0;
            }
        }

        private void OnItemExpanding(object sender, VirtualizingItemExpandingArgs e)
        {
            ProjectItem item = (ProjectItem)e.Item;
            e.Children = item.Children;
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
