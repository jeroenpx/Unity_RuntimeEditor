using Battlehub.RTCommon;
using Battlehub.RTSaveLoad2;
using Battlehub.RTSaveLoad2.Interface;
using Battlehub.UIControls;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class AssetLibrarySelectDialog : MonoBehaviour
    {
        [SerializeField]
        private VirtualizingTreeView m_builtInTreeView = null;
        [SerializeField]
        private VirtualizingTreeView m_externalTreeView = null;
        [SerializeField]
        private Toggle m_builtInToggle = null;

        [SerializeField]
        private Sprite AssetLibraryIcon = null;

        private PopupWindow m_parentPopup;

        private IProject m_project;

        public bool IsBuiltInLibrary
        {
            get { return m_builtInToggle.isOn; }
        }

        public string SelectedLibrary
        {
            get
            {
                if(IsBuiltInLibrary)
                {
                    return m_builtInTreeView.SelectedItem as string;
                }
                return m_externalTreeView.SelectedItem as string;
            }
        }


        private void Start()
        {
            m_parentPopup = GetComponentInParent<PopupWindow>();
            if (m_parentPopup != null)
            {
                m_parentPopup.OK.AddListener(OnOK);
            }

            
            if (m_builtInTreeView == null)
            {
                Debug.LogError("m_builtInTreeView == null");
                return;
            }

            if(m_externalTreeView == null)
            {
                Debug.LogError("m_externalTreeView == null");
                return;
            }

            m_builtInTreeView.ItemDataBinding += OnItemDataBinding;
            m_builtInTreeView.ItemDoubleClick += OnItemDoubleClick;
            m_builtInTreeView.CanDrag = false;
            m_builtInTreeView.CanEdit = false;
            m_builtInTreeView.CanUnselectAll = false;

            m_externalTreeView.ItemDataBinding += OnItemDataBinding;
            m_externalTreeView.ItemDoubleClick += OnItemDoubleClick;
            m_externalTreeView.CanDrag = false;
            m_externalTreeView.CanEdit = false;
            m_externalTreeView.CanUnselectAll = false;

            m_externalTreeView.transform.parent.gameObject.SetActive(false);
            m_builtInTreeView.transform.parent.gameObject.SetActive(true);

            m_project = IOC.Resolve<IProject>();
            m_builtInTreeView.Items = m_project.AssetLibraries;
            m_builtInTreeView.SelectedIndex = 0;

            IRTE editor = IOC.Resolve<IRTE>();
            editor.IsBusy = true;
            m_project.GetAssetBundles((error, assetBundles) =>
            {
                editor.IsBusy = false;
                if (error.HasError)
                {
                    PopupWindow.Show("Unable to list asset bundles", error.ToString(), "OK");
                    return;
                }
                m_externalTreeView.Items = assetBundles;
                m_externalTreeView.SelectedIndex = 0;
            });
        }

        private void OnDestroy()
        {
            if (m_parentPopup != null)
            {
                m_parentPopup.OK.RemoveListener(OnOK);
            }

            if (m_builtInTreeView != null)
            {
                m_builtInTreeView.ItemDataBinding -= OnItemDataBinding;
                m_builtInTreeView.ItemDoubleClick -= OnItemDoubleClick;
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
            m_parentPopup.Close(true);
        }

        private void OnOK(PopupWindowArgs args)
        {
            if (m_builtInTreeView.SelectedItem == null && IsBuiltInLibrary || m_externalTreeView.SelectedItem == null && !IsBuiltInLibrary)
            {
                args.Cancel = true;
                return;
            }
        }
    }
}

