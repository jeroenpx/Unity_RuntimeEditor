using Battlehub.RTCommon;
using Battlehub.RTSaveLoad2.Interface;
using Battlehub.UIControls;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class AssetLibraryImportDialog : MonoBehaviour
    {
        [SerializeField]
        private VirtualizingTreeView TreeViewPrefab;
        
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

            m_project = IOC.Resolve<IProject>();

            m_editor.IsBusy = true;
            m_project.LoadAssetLibrary(Array.IndexOf(m_project.AssetLibraries, m_selectedAssetLibrary), (error, root) =>
            {
                m_editor.IsBusy = false;
                if (error.HasError)
                {
                    PopupWindow.Show("Unable to load AssetLibrary", error.ErrorText, "OK", arg =>
                    {
                        m_parentPopup.Close(false);
                    });
                }
                else
                {
                    m_treeView.Items = new[] { root };
                    m_treeView.SelectedItems = root.Flatten(false);
                    ExpandAll(root);
                    

                    IResourcePreviewUtility resourcePreview = IOC.Resolve<IResourcePreviewUtility>();
                    StartCoroutine(ProjectItemView.CoCreatePreviews(root.Flatten(false), m_project, resourcePreview));
                }
            });
        }

        private void ExpandAll(ProjectItem root)
        {
            Queue<ProjectItem> q = new Queue<ProjectItem>();
            q.Enqueue(root);
            while(q.Count > 0)
            {
                ProjectItem item = q.Dequeue();
                m_treeView.Expand(item);

                if (item.Children != null)
                {
                    for (int i = 0; i < item.Children.Count; ++i)
                    {
                        q.Enqueue(item.Children[i]);
                    }
                }
            }            
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

                ProjectItemView itemView = e.ItemPresenter.GetComponentInChildren<ProjectItemView>(true);
                itemView.ProjectItem = item;

                Toggle toogle = e.ItemPresenter.GetComponentInChildren<Toggle>();
                toogle.isOn = m_treeView.IsItemSelected(item);

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
