using System;
using UnityEngine;

using Battlehub.UIControls;
using Battlehub.RTCommon;
using Battlehub.UIControls.Dialogs;
using Battlehub.RTSaveLoad2.Interface;

using UnityObject = UnityEngine.Object;
using UnityEngine.UI;
using System.Linq;
using System.Collections;

namespace Battlehub.RTEditor
{
    public class SelectObjectDialog : RuntimeWindow
    {
        [SerializeField]
        private InputField m_filter;
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
        private IWindowManager m_windowManager;
        private IProject m_project;
        private Guid m_noneGuid = Guid.NewGuid();
        private bool m_previewsCreated;
        private AssetItem[] m_cache;
        
        private void Start()
        {
            m_parentDialog = GetComponentInParent<Dialog>();
            m_parentDialog.IsOkVisible = true;
            m_parentDialog.IsCancelVisible = true;
            m_parentDialog.OkText = "Select";
            m_parentDialog.CancelText = "Cancel";
            m_parentDialog.Ok += OnOk;

            m_project = IOC.Resolve<IProject>();
            m_windowManager = IOC.Resolve<IWindowManager>();

            IResourcePreviewUtility resourcePreview = IOC.Resolve<IResourcePreviewUtility>();
            AssetItem[] assetItems = m_project.Root.Flatten(true, false).Where(item => m_project.ToType((AssetItem)item) == ObjectType).OfType<AssetItem>().ToArray();

            m_treeView.SelectionChanged += OnSelectionChanged;
            m_treeView.ItemDataBinding += OnItemDataBinding;
            Editor.IsBusy = true;
            m_parentDialog.IsOkInteractable = false;
            m_project.GetAssetItems(assetItems, (error, assetItemsWithPreviews) =>
            {
                if (error.HasError)
                {
                    Editor.IsBusy = false;
                    m_windowManager.MessageBox("Can't GetAssets", error.ToString());
                    return;
                }

                AssetItem none = new AssetItem();
                none.Name = "None";
                none.TypeGuid = m_noneGuid;

                assetItemsWithPreviews = new[] { none }.Union(assetItemsWithPreviews).ToArray();

                m_previewsCreated = false;
                StartCoroutine(ProjectItemView.CoCreatePreviews(assetItemsWithPreviews, m_project, resourcePreview, () =>
                {
                    m_previewsCreated = true;
                    HandleSelectionChanged((AssetItem)m_treeView.SelectedItem);
                    m_treeView.ItemDoubleClick += OnItemDoubleClick;
                    m_parentDialog.IsOkInteractable = m_previewsCreated && m_treeView.SelectedItem != null;
                    Editor.IsBusy = false;

                    if(m_filter != null)
                    {
                        if (!string.IsNullOrEmpty(m_filter.text))
                        {
                            ApplyFilter(m_filter.text);
                        }
                        m_filter.onValueChanged.AddListener(OnFilterValueChanged);
                    }
                }));

                m_cache = assetItemsWithPreviews;
                m_treeView.Items = m_cache;
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

            if (m_filter != null)
            {
                m_filter.onValueChanged.RemoveListener(OnFilterValueChanged);
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
            if (!m_previewsCreated)
            {
                return;
            }

            AssetItem assetItem = (AssetItem)e.NewItem;
            HandleSelectionChanged(assetItem);
        }

        private void HandleSelectionChanged(AssetItem assetItem)
        {
            if (assetItem != null && assetItem.TypeGuid == m_noneGuid)
            {
                IsNoneSelected = true;
                SelectedObject = null;
            }
            else
            {
                IsNoneSelected = false;
                if (assetItem != null)
                {
                    SelectedObject = null;
                    m_project.Load(assetItem, (error, obj) =>
                    {
                        SelectedObject = obj;
                    });
                }
                else
                {
                    SelectedObject = null;
                }
            }

            m_parentDialog.IsOkInteractable = m_treeView.SelectedItem != null;
        }

        private void OnItemDoubleClick(object sender, ItemArgs e)
        {
            AssetItem assetItem = (AssetItem)e.Items[0];
            if (assetItem != null && assetItem.TypeGuid == m_noneGuid)
            {
                IsNoneSelected = true;
                SelectedObject = null;
                m_parentDialog.Close(true);
            }
            else
            {
                IsNoneSelected = false;
                if (assetItem != null)
                {
                    SelectedObject = null;
                    Editor.IsBusy = true;
                    m_project.Load(assetItem, (error, obj) =>
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

        private void OnFilterValueChanged(string text)
        {
            ApplyFilter(text);
        }

        private void ApplyFilter(string text)
        {
            if (m_coApplyFilter != null)
            {
                StopCoroutine(m_coApplyFilter);
            }
            StartCoroutine(m_coApplyFilter = CoApplyFilter(text));
        }

        private IEnumerator m_coApplyFilter;
        private IEnumerator CoApplyFilter(string filter)
        {
            yield return new WaitForSeconds(0.3f);

            if (string.IsNullOrEmpty(filter))
            {
                m_treeView.Items = m_cache;
            }
            else
            {
                m_treeView.Items = m_cache.Where(item => item.Name.ToLower().Contains(filter.ToLower()));
            }
        }
    }
}

