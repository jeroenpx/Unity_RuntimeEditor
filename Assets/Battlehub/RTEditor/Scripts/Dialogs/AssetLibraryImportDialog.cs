using Battlehub.RTCommon;
using Battlehub.RTSaveLoad2.Interface;
using Battlehub.UIControls;
using System;
using System.Collections;
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

        private IEnumerator m_coCreatePreviews;

        private void Start()
        {
            m_editor = IOC.Resolve<IRTE>();
            m_parentPopup = GetComponentInParent<PopupWindow>();
            
            if (m_parentPopup != null)
            {
                m_parentPopup.OK.AddListener(OnOK);
                m_parentPopup.Cancel.AddListener(OnCancel);
            }

            m_treeView = GetComponentInChildren<VirtualizingTreeView>();
            if (m_treeView == null)
            {
                m_treeView = Instantiate(TreeViewPrefab);
                m_treeView.transform.SetParent(transform, false);
            }

            m_treeView.ItemDataBinding += OnItemDataBinding;
            m_treeView.ItemExpanding += OnItemExpanding;
            
            m_treeView.CanDrag = false;
            m_treeView.CanEdit = false;

            m_project = IOC.Resolve<IProject>();

            m_editor.IsBusy = true;
            m_parentPopup.IsContentLoaded = false;

            m_project.LoadImportItems(m_selectedLibrary, m_isBuiltIn, (error, root) =>
            {  
                if (error.HasError)
                {
                    m_parentPopup.IsContentLoaded = true;
                    m_editor.IsBusy = false;
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

                    m_editor.IsBusy = true;
                    IResourcePreviewUtility resourcePreview = IOC.Resolve<IResourcePreviewUtility>();

                    m_coCreatePreviews = ProjectItemView.CoCreatePreviews(root.Flatten(false), m_project, resourcePreview, () =>
                    {
                        m_project.UnloadImportItems(root);
                        m_parentPopup.IsContentLoaded = true;
                        m_editor.IsBusy = false;
                        m_coCreatePreviews = null;
                    });

                    StartCoroutine(m_coCreatePreviews);
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
                m_parentPopup.Cancel.RemoveListener(OnCancel);
            }

            if (m_treeView != null)
            {
                m_treeView.ItemDataBinding -= OnItemDataBinding;
                m_treeView.ItemExpanding -= OnItemExpanding;
            }

            if(m_coCreatePreviews != null)
            {
                StopCoroutine(m_coCreatePreviews);
                m_coCreatePreviews = null;
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

        private void OnOK(PopupWindowArgs args)
        {
            if(!m_parentPopup.IsContentLoaded)
            {
                args.Cancel = true;
                return;
            }
            if (m_treeView.SelectedItemsCount == 0)
            {
                args.Cancel = true;
                return;
            }
        }

        private void OnCancel(PopupWindowArgs args)
        {
            if(m_editor.IsBusy)
            {
                args.Cancel = true;
                return;
            }

            if (m_coCreatePreviews != null)
            {
                StopCoroutine(m_coCreatePreviews);
                m_coCreatePreviews = null;
            }

            if(m_treeView.Items != null)
            {
                m_project.UnloadImportItems(m_treeView.Items.OfType<ProjectItem>().FirstOrDefault());
            }
            else
            {
                Debug.LogWarning("m_treeView.Items == null");
            }
        }
    }
}
