using Battlehub.RTCommon;
using Battlehub.RTSaveLoad2.Interface;
using Battlehub.UIControls;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private bool m_isBuiltIn;
        public bool IsBuiltIn
        {
            set { m_isBuiltIn = value; }
        }

        private string m_selectedLibrary;
        public string SelectedLibrary
        {
            set { m_selectedLibrary = value; }
        }

        public ImportItem[] SelectedAssets
        {
            get
            {
                return m_treeView.SelectedItems.OfType<ImportItem>().ToArray();
            }
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

            m_project.LoadAssetLibrary(m_selectedLibrary, m_isBuiltIn, (error, root) =>
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

                Toggle toogle = e.ItemPresenter.GetComponentInChildren<Toggle>(true);
                toogle.isOn = m_treeView.IsItemSelected(item);

                AssetLibraryImportStatus status = e.ItemPresenter.GetComponentInChildren<AssetLibraryImportStatus>(true);
                if (item is ImportItem)
                {
                    ImportItem importItem = (ImportItem)item;
                    status.Current = importItem.Status;
                }
                else
                {
                    status.Current = ImportStatus.None;
                }
                
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
            if (m_treeView.SelectedItemsCount == 0)
            {
                args.Cancel = true;
                return;
            }
        }


    }
}
