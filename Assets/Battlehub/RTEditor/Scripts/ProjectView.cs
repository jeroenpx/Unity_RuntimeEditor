using UnityEngine;
using UnityEngine.UI;
using Battlehub.RTCommon;
using System.Linq;
using Battlehub.UIControls;
using Battlehub.RTSaveLoad2.Interface;

namespace Battlehub.RTEditor
{
    public class ProjectView : RuntimeWindow
    {
        private IProject m_project;
        private IResourcePreviewUtility m_resourcePreview;

        [SerializeField]
        private Text m_loadingProgressText = null;

        [SerializeField]
        private ProjectTreeView m_projectTree = null;
        [SerializeField]
        private ProjectFolderView m_projectResources = null;
        //[SerializeField]
        //private Button m_btnDuplicate = null;
        [SerializeField]
        private string ProjectName = "DefaultProject";

        public KeyCode DuplicateKey = KeyCode.D;
        public KeyCode RuntimeModifierKey = KeyCode.LeftControl;
        public KeyCode EditorModifierKey = KeyCode.LeftShift;
        public KeyCode ModifierKey
        {
            get
            {
                #if UNITY_EDITOR
                return EditorModifierKey;
                #else
                return RuntimeModifierKey;
                #endif
            }
        }

        private bool m_showProgress;
        private bool ShowProgress
        {
            get { return m_showProgress; }
            //Show progress bar if ui here needed
            set
            {
                if (m_showProgress != value)
                {
                    m_showProgress = value;
                    if (m_loadingProgressText != null)
                    {
                        m_loadingProgressText.text = "Loading...";
                        m_loadingProgressText.gameObject.SetActive(m_showProgress);
                    }

                }
            }
        }

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.Project;
            base.AwakeOverride();
        }

        private void Start()
        {
            m_project = IOC.Resolve<IProject>();
            if(m_project == null)
            {
                Debug.LogWarning("RTSL2Deps.Get.Project is null");
                Destroy(gameObject);
                return;
            }

            m_resourcePreview = IOC.Resolve<IResourcePreviewUtility>();
            if(m_resourcePreview == null)
            {
                Debug.LogWarning("RTEDeps.Get.ResourcePreview is null");
            }

            m_projectResources.ItemDoubleClick += OnProjectResourcesDoubleClick;
            m_projectResources.ItemRenamed += OnProjectResourcesRenamed; 
            m_projectResources.ItemDeleted += OnProjectResourcesDeleted;
                
            m_projectTree.SelectionChanged += OnProjectTreeSelectionChanged;
            m_projectTree.ItemRenamed += OnProjectTreeItemRenamed;
            m_projectTree.ItemDeleted += OnProjectTreeItemDeleted;

            m_project.OpenProjectCompleted += OnProjectOpenCompleted;
            m_project.CloseProjectCompleted += OnCloseProjectCompleted;
            m_project.ImportCompleted += OnImportCompleted;
            m_project.DeleteCompleted += OnDeleteCompleted;
            m_project.RenameCompleted += OnRenameCompleted;
            m_project.MoveCompleted += OnMoveCompleted;
            m_project.CreateCompleted += OnPrefabCreateCompleted;
            m_project.SaveCompleted += OnSaveCompleted;


            if (!m_project.IsOpened && !m_project.IsBusy)
            {
                ShowProgress = true;
                Editor.IsBusy = true;
                m_project.OpenProject(ProjectName);
            }
            else
            {
                m_projectTree.LoadProject(m_project.Root);
                m_projectTree.SelectedFolder = m_project.Root;
            }   
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            if (m_projectResources != null)
            {
                m_projectResources.ItemDoubleClick -= OnProjectResourcesDoubleClick;
                m_projectResources.ItemRenamed -= OnProjectResourcesRenamed;
                m_projectResources.ItemDeleted -= OnProjectResourcesDeleted;
            }

            if(m_projectTree != null)
            {
                m_projectTree.SelectionChanged -= OnProjectTreeSelectionChanged;
                m_projectTree.ItemDeleted -= OnProjectTreeItemDeleted;
                m_projectTree.ItemRenamed -= OnProjectTreeItemRenamed;
            }

            if (m_project != null)
            {
                m_project.OpenProjectCompleted -= OnProjectOpenCompleted;
                m_project.ImportCompleted -= OnImportCompleted;
                m_project.DeleteCompleted -= OnDeleteCompleted;
                m_project.RenameCompleted -= OnRenameCompleted;
                m_project.MoveCompleted -= OnMoveCompleted;
                m_project.CreateCompleted -= OnPrefabCreateCompleted;
                m_project.SaveCompleted -= OnSaveCompleted;
            }
        }


        private void OnProjectOpenCompleted(Error error, ProjectInfo projectInfo)
        {
            ShowProgress = false;
            Editor.IsBusy = false;
            if (error.HasError)
            {
                PopupWindow.Show("Can't open project", error.ToString(), "OK");
                return;
            }
            
            m_projectTree.LoadProject(m_project.Root);
            m_projectTree.SelectedFolder = m_project.Root;
        }

        private void OnCloseProjectCompleted(Error error)
        {
            if (error.HasError)
            {
                PopupWindow.Show("Can't close project", error.ToString(), "OK");
                return;
            }

            m_projectTree.LoadProject(null);
            m_projectTree.SelectedFolder = null;
            m_projectResources.SetItems(null, null, true);
        }

        private void OnImportCompleted(Error error, AssetItem[] result)
        {
            Editor.IsBusy = false;
            if (error.HasError)
            {
                PopupWindow.Show("Unable to Import assets", error.ErrorText, "OK");
            }

            string path = string.Empty;
            if (m_projectTree.SelectedFolder != null)
            {
                path = m_projectTree.SelectedFolder.ToString();
            }

            m_projectTree.LoadProject(m_project.Root);

            if (!string.IsNullOrEmpty(path))
            {
                m_projectTree.SelectedFolder = m_project.Root.Get(path);
            }
            else
            {
                m_projectTree.SelectedFolder = m_project.Root;
            }
        }

        private void OnDeleteCompleted(Error error, ProjectItem[] result)
        {
            Editor.IsBusy = false;
            if (error.HasError)
            {
                PopupWindow.Show("Unable to remove", error.ErrorText, "OK");
            }
            m_projectTree.RemoveProjectItemsFromTree(result);
        }

        private void OnRenameCompleted(Error error, ProjectItem result)
        {
            Editor.IsBusy = false;
            if (error.HasError)
            {
                PopupWindow.Show("Unable to rename asset", error.ToString(), "OK");
            }
        }

        private void OnMoveCompleted(Error error, ProjectItem[] projectItems, ProjectItem parent)
        {
            if (error.HasError)
            {
                PopupWindow.Show("Unable to move assets", error.ErrorText, "OK");
                return;
            }

            for (int i = 0; i < projectItems.Length; ++i)
            {
                ProjectItem projectItem = projectItems[i];
            
                if (!(projectItem is AssetItem))
                {
                    m_projectTree.ChangeParent(projectItems);
                }
            }
        }

        private void OnSaveCompleted(Error saveError, AssetItem[] result)
        {
        }

        private void OnPrefabCreateCompleted(Error createPrefabError, AssetItem result)
        {
            m_projectResources.InsertItems(new[] { result });
        }
  
        private void OnProjectTreeSelectionChanged(object sender, SelectionChangedArgs<ProjectItem> e)
        {
            ShowProgress = true;
            m_project.GetAssetItems(e.NewItems, (error, assets) =>
            {
                ShowProgress = false;
                if (error.HasError)
                {
                    PopupWindow.Show("Can't GetAssets", error.ToString(), "OK");
                    return;
                }

                StartCoroutine(ProjectItemView.CoCreatePreviews(assets, m_project, m_resourcePreview));
                m_projectResources.SetItems(e.NewItems.ToArray(), assets, true);
            });

        }

        private void OnProjectResourcesDeleted(object sender, ProjectTreeEventArgs e)
        {
            Editor.IsBusy = true;
            m_project.Delete(e.ProjectItems);
        }

        private void OnProjectResourcesDoubleClick(object sender, ProjectTreeEventArgs e)
        {
            m_projectTree.SelectedFolder = e.ProjectItem;
        }

        private void OnProjectResourcesRenamed(object sender, ProjectTreeRenamedEventArgs e)
        {
            m_projectTree.UpdateProjectItem(e.ProjectItem);

            Editor.IsBusy = true;
            m_project.Rename(e.ProjectItem, e.OldName);
        }

        private void OnProjectTreeItemRenamed(object sender, ProjectTreeRenamedEventArgs e)
        {
            Editor.IsBusy = true;
            m_project.Rename(e.ProjectItem, e.OldName);
        }

        private void OnProjectTreeItemDeleted(object sender, ProjectTreeEventArgs e)
        {
            Editor.IsBusy = true;
            m_project.Delete(e.ProjectItems);
        }

        private void CreateFolder()
        {
            Debug.LogWarning("CreateFolder");
        }
    }
}
