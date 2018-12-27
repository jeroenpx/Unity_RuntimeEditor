using System;
using UnityEngine;

using Battlehub.UIControls;
using Battlehub.RTCommon;
using Battlehub.UIControls.Dialogs;
using Battlehub.RTSaveLoad2.Interface;

using UnityObject = UnityEngine.Object;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class SelectObjectDialog : RuntimeWindow
    {
        [HideInInspector]
        public UnityObject SelectedObject;
        [HideInInspector]
        public Type ObjectType;
        [SerializeField]
        private VirtualizingTreeView m_treeView = null;
        public bool IsNoneSelected
        {
            get;
            private set;
        }

        private Dialog m_parentDialog;
        private IProject m_project;
        
        private void Start()
        {
            m_parentDialog = GetComponentInParent<Dialog>();
            m_parentDialog.IsOkVisible = true;
            m_parentDialog.IsCancelVisible = true;
            m_parentDialog.OkText = "Select";
            m_parentDialog.CancelText = "Cancel";
            m_parentDialog.Ok += OnOk;

            m_project = IOC.Resolve<IProject>();
            IResourcePreviewUtility resourcePreview = IOC.Resolve<IResourcePreviewUtility>();
            
            ProjectItem[] folders = m_project.Root.Flatten(false, true);

            m_treeView.ItemDoubleClick += OnItemDoubleClick;
            m_treeView.SelectionChanged += OnSelectionChanged;
            m_treeView.ItemDataBinding += OnItemDataBinding;
            Editor.IsBusy = true;
            m_project.GetAssetItems(folders, (error, assets) =>
            {
                Editor.IsBusy = false;
                if (error.HasError)
                {
                    PopupWindow.Show("Can't GetAssets", error.ToString(), "OK");
                    return;
                }

                StartCoroutine(ProjectItemView.CoCreatePreviews(assets, m_project, resourcePreview));
                m_treeView.Items = assets;
            });
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
        
            if (m_parentDialog != null)
            {
                m_parentDialog.Ok -= OnOk;
            }

            if (m_treeView != null)
            {
                m_treeView.ItemDoubleClick -= OnItemDoubleClick;
                m_treeView.SelectionChanged -= OnSelectionChanged;
                m_treeView.ItemDataBinding -= OnItemDataBinding;
            }
        }


        private void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            ProjectItem projectItem = e.Item as ProjectItem;
            if (projectItem == null)
            {
                Text text = e.ItemPresenter.GetComponentInChildren<Text>(true);
                text.text = null;
                ProjectItemView itemView = e.ItemPresenter.GetComponentInChildren<ProjectItemView>(true);
                itemView.ProjectItem = null;
            }
            else
            {
                Text text = e.ItemPresenter.GetComponentInChildren<Text>(true);
                text.text = projectItem.Name;
                ProjectItemView itemView = e.ItemPresenter.GetComponentInChildren<ProjectItemView>(true);
                itemView.ProjectItem = projectItem;
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedArgs e)
        {
            //if (e.ProjectItem != null && e.ProjectItem.IsNone)
            //{
            //    IsNoneSelected = true;
            //}
            //else
            {
                IsNoneSelected = false;
                if (e.NewItem != null)
                {
                    SelectedObject = null;
                    m_project.Load((AssetItem)e.NewItem, (error, obj) =>
                    {
                        SelectedObject = obj;
                    });
                }
                else
                {
                    SelectedObject = null;
                }
            }
        }

        private void OnItemDoubleClick(object sender, ItemArgs e)
        {
            //if (e.ProjectItem != null && e.ProjectItem.IsNone)
            //{
            //    IsNoneSelected = true;
            //}
            //else
            {
                IsNoneSelected = false;
                if (e.Items != null)
                {
                    SelectedObject = null;
                    Editor.IsBusy = true;
                    m_project.Load((AssetItem)e.Items[0], (error, obj) =>
                    {
                        Editor.IsBusy = false;
                        SelectedObject = obj;
                        m_parentDialog.Close(true);
                    });
                }
                else
                {
                    SelectedObject = null;
                    m_parentDialog.Close(true);
                }
            }
        }

        private void OnOk(Dialog sender, DialogCancelArgs args)
        {
            if (SelectedObject == null && !IsNoneSelected)
            {
                args.Cancel = true;
            }
        }
    }
}

