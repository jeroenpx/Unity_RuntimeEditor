using UnityEngine;
using UnityEngine.UI;
using Battlehub.RTCommon;
using System.Linq;
using Battlehub.UIControls;
using Battlehub.RTSaveLoad2.Interface;
using Battlehub.UIControls.DockPanels;

namespace Battlehub.RTEditor
{
    public class ProjectView : RuntimeWindow
    {
        private IWindowManager m_windowManager;
        private IProject m_project;
        private IResourcePreviewUtility m_resourcePreview;
        [SerializeField]
        private ProjectTreeView m_projectTree = null;
        [SerializeField]
        private ProjectFolderView m_projectResources = null;
        [SerializeField]
        private string ProjectName = "DefaultProject";

        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.Project;
            base.AwakeOverride();
        }

        private void Start()
        {
            m_windowManager = IOC.Resolve<IWindowManager>();
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

            DockPanelsRoot dockPanelsRoot = GetComponent<DockPanelsRoot>();
            if (dockPanelsRoot != null)
            {
                dockPanelsRoot.CursorHelper = Editor.CursorHelper;
            }

            m_projectResources.ItemDoubleClick += OnProjectResourcesDoubleClick;
            m_projectResources.ItemRenamed += OnProjectResourcesRenamed; 
            m_projectResources.ItemDeleted += OnProjectResourcesDeleted;
            m_projectResources.SelectionChanged += OnProjectResourcesSelectionChanged;
                
            m_projectTree.SelectionChanged += OnProjectTreeSelectionChanged;
            m_projectTree.ItemRenamed += OnProjectTreeItemRenamed;
            m_projectTree.ItemDeleted += OnProjectTreeItemDeleted;

            m_project.OpenProjectCompleted += OnProjectOpenCompleted;
            m_project.CloseProjectCompleted += OnCloseProjectCompleted;
            m_project.ImportCompleted += OnImportCompleted;
            m_project.BeforeDeleteCompleted += OnBeforeDeleteCompleted;
            m_project.DeleteCompleted += OnDeleteCompleted;
            m_project.RenameCompleted += OnRenameCompleted;
            m_project.CreateCompleted += OnCreateCompleted;
            m_project.MoveCompleted += OnMoveCompleted;
            m_project.SaveCompleted += OnSaveCompleted;
            m_project.DuplicateCompleted += OnDuplicateCompleted;
         
            if (!m_project.IsOpened && !m_project.IsBusy)
            {
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
                m_projectResources.SelectionChanged -= OnProjectResourcesSelectionChanged;
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
                m_project.BeforeDeleteCompleted -= OnBeforeDeleteCompleted;
                m_project.DeleteCompleted -= OnDeleteCompleted;
                m_project.RenameCompleted -= OnRenameCompleted;
                m_project.CreateCompleted -= OnCreateCompleted;
                m_project.MoveCompleted -= OnMoveCompleted;
                m_project.SaveCompleted -= OnSaveCompleted;
                m_project.DuplicateCompleted -= OnDuplicateCompleted;
            }
        }

        private void OnProjectOpenCompleted(Error error, ProjectInfo projectInfo)
        {
            Editor.IsBusy = false;
            if (error.HasError)
            {
                m_windowManager.MessageBox("Can't open project", error.ToString());
                return;
            }
            
            m_projectTree.LoadProject(m_project.Root);
            m_projectTree.SelectedFolder = m_project.Root;
        }

        private void OnCloseProjectCompleted(Error error)
        {
            if (error.HasError)
            {
                m_windowManager.MessageBox("Can't close project", error.ToString());
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
                m_windowManager.MessageBox("Unable to Import assets", error.ErrorText);
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

        private void OnBeforeDeleteCompleted(Error error, ProjectItem[] result)
        {
            Editor.IsBusy = false;
            if (error.HasError)
            {
                m_windowManager.MessageBox("Unable to remove", error.ErrorText);
            }
            m_projectTree.RemoveProjectItemsFromTree(result);
        }

        private void OnDeleteCompleted(Error error, ProjectItem[] result)
        {
            m_projectTree.SelectRootIfNothingSelected();

            if(Editor.Selection.activeObject != null)
            {
                long selectedObjectId = m_project.ToID(Editor.Selection.activeObject);
                if(result.Any(r => r.ItemID == selectedObjectId))
                {
                    bool wasEnabled = Editor.Undo.Enabled;
                    Editor.Undo.Enabled = false;
                    Editor.Selection.activeObject = null;
                    Editor.Undo.Enabled = wasEnabled;
                }
            }
            
        }

        private void OnRenameCompleted(Error error, ProjectItem result)
        {
            Editor.IsBusy = false;
            if (error.HasError)
            {
                m_windowManager.MessageBox("Unable to rename asset", error.ToString());
            }
        }

        private void OnCreateCompleted(Error error, ProjectItem result)
        {
            Editor.IsBusy = false;
            if (error.HasError)
            {
                m_windowManager.MessageBox("Unable to create folder", error.ToString());
            }

            m_projectTree.AddItem(result.Parent, result);
            m_projectTree.SelectedFolder = result;
        }

        private void OnMoveCompleted(Error error, ProjectItem[] projectItems, ProjectItem parent)
        {
            if (error.HasError)
            {
                m_windowManager.MessageBox("Unable to move assets", error.ErrorText);
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

     
        private void OnSaveCompleted(Error createPrefabError, AssetItem[] result)
        {
            Editor.IsBusy = false;
            m_projectResources.InsertItems(result);
        }

        private void OnDuplicateCompleted(Error error, AssetItem[] result)
        {
            Editor.IsBusy = false;
            m_projectResources.InsertItems(result);
        }

        private void OnProjectTreeSelectionChanged(object sender, SelectionChangedArgs<ProjectItem> e)
        {
            m_project.GetAssetItems(e.NewItems, (error, assets) =>
            {  
                if (error.HasError)
                {
                    m_windowManager.MessageBox("Can't GetAssets", error.ToString());
                    return;
                }

                StartCoroutine(ProjectItemView.CoCreatePreviews(assets, m_project, m_resourcePreview));
                m_projectResources.SetItems(e.NewItems.ToArray(), assets, true);
            });

        }

        private void OnProjectResourcesDeleted(object sender, ProjectTreeEventArgs e)
        {
            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
            editor.DeleteAssets(e.ProjectItems);
        }

        private void OnProjectResourcesDoubleClick(object sender, ProjectTreeEventArgs e)
        {
            if(e.ProjectItem == null)
            {
                return;
            }

            if(e.ProjectItem.IsFolder)
            {
                m_projectTree.SelectedFolder = e.ProjectItem;
            }
            else
            {
                if(m_project.IsScene(e.ProjectItem))
                {
                    Editor.IsBusy = true;
                    m_project.Load(new[] { (AssetItem)e.ProjectItem }, (error, obj) =>
                    {
                        Editor.IsBusy = false;
                        if(error.HasError)
                        {
                            m_windowManager.MessageBox("Unable to load scene " + e.ProjectItem.ToString(), error.ToString());
                        }
                    });
                }
            }
        }

        private void OnProjectResourcesRenamed(object sender, ProjectTreeRenamedEventArgs e)
        {
            m_projectTree.UpdateProjectItem(e.ProjectItem);            
            Editor.IsBusy = true;
            m_project.Rename(e.ProjectItem, e.OldName);
        }

        private void OnProjectResourcesSelectionChanged(object sender, ProjectTreeEventArgs e)
        {
            if(e.ProjectItem != null)
            {
                
                if (!e.ProjectItem.IsFolder)
                {
                    if(m_project.IsScene(e.ProjectItem))
                    {
                        Editor.Selection.activeObject = null;
                    }
                    else
                    {
                        Editor.IsBusy = true;
                        m_project.Load(new[] { (AssetItem)e.ProjectItem }, (error, obj) =>
                        {
                            Editor.IsBusy = false;
                            Editor.Selection.activeObject = obj[0];
                        });
                    }
                }   
                else
                {                    
                    Editor.Selection.activeObject = null;
                }
            }
        }

        private void OnProjectTreeItemRenamed(object sender, ProjectTreeRenamedEventArgs e)
        {
            Editor.IsBusy = true;
            m_project.Rename(e.ProjectItem, e.OldName);
        }

        private void OnProjectTreeItemDeleted(object sender, ProjectTreeEventArgs e)
        {
            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
            editor.DeleteAssets(e.ProjectItems);
        }

        private void CreateFolder()
        {
            Debug.LogWarning("CreateFolder");
        }
    }
}
