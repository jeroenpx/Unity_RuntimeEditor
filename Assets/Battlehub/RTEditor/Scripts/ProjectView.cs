using UnityEngine;
using UnityEngine.UI;
using Battlehub.RTCommon;
using System.Linq;
using Battlehub.UIControls;
using Battlehub.RTSaveLoad2.Interface;
using System.Collections;

using UnityObject = UnityEngine.Object;
using System;

namespace Battlehub.RTEditor
{
    public class ProjectView : RuntimeWindow
    {
        private IProject m_project;
        private IResourcePreviewUtility m_resourcePreview;
       
        [SerializeField]
        private Text m_loadingProgressText;

        [SerializeField]
        private ProjectTreeView m_projectTree;
        [SerializeField]
        private ProjectFolderView m_projectResources;
        [SerializeField]
        private Button m_btnDuplicate;
        [SerializeField]
        private Dropdown m_ddCreate;
        [SerializeField]
        private Button m_btnImport;
        [SerializeField]
        private AssetLibrarySelectDialog m_assetLibrarySelectorPrefab;
        [SerializeField]
        private AssetLibraryImportDialog m_assetLibraryImportPrefab;
        
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

            if(m_ddCreate != null)
            {
                m_ddCreate.onValueChanged.AddListener(OnCreate);
            }
            


            //RuntimeEditorApplication.SaveSelectedObjectsRequired += OnSaveSelectedObjectsRequest;

            //m_projectResources.SelectionChanged += OnProjectResourcesSelectionChanged;
            m_projectResources.ItemDoubleClick += OnProjectResourcesDoubleClick;
            m_projectResources.ItemRenamed += OnProjectResourcesRenamed; 
            m_projectResources.ItemDeleted += OnProjectResourcesDeleted;
            m_projectResources.ItemsDropped += OnProjectResourcesDrop;
            //m_projectResources.BeginDrag += OnProjectResourcesBeginDrag;
            //m_projectResources.Drop += OnProjectResourcesDrop;

            

            m_projectTree.SelectionChanged += OnProjectTreeSelectionChanged;
            m_projectTree.ItemRenamed += OnProjectTreeItemRenamed;
            m_projectTree.ItemDeleted += OnProjectTreeItemDeleted;
            //m_projectTree.Drop += OnProjectTreeItemDrop;

            ShowProgress = true;

         

            m_project.Open(ProjectName, error =>
            {
                if(error.HasError)
                {
                    PopupWindow.Show("Can't open project", error.ToString(), "OK");
                    return;
                }

                ShowProgress = false;

                m_projectTree.LoadProject(m_project.Root);
                m_projectTree.SelectedFolder = m_project.Root;
            });


            if(m_btnImport != null)
            {
                m_btnImport.onClick.AddListener(SelectLibrary);
            }
        }

        protected override void OnDestroyOverride()
        {
            if (m_projectResources != null)
            {
                m_projectResources.ItemDoubleClick -= OnProjectResourcesDoubleClick;

                m_projectResources.ItemRenamed -= OnProjectResourcesRenamed;
                m_projectResources.ItemDeleted -= OnProjectResourcesDeleted;
                m_projectResources.ItemsDropped -= OnProjectResourcesDrop;

            }

            if(m_projectTree != null)
            {
                m_projectTree.SelectionChanged -= OnProjectTreeSelectionChanged;
                m_projectTree.ItemDeleted -= OnProjectTreeItemDeleted;
                m_projectTree.ItemRenamed -= OnProjectTreeItemRenamed;
            }

            if (m_btnImport != null)
            {
                m_btnImport.onClick.RemoveListener(SelectLibrary);
            }

            if(m_ddCreate != null)
            {
                m_ddCreate.onValueChanged.RemoveListener(OnCreate);
            }
        }

        protected override void UpdateOverride()
        {
            base.UpdateOverride();
            //if (InputController._GetKeyDown(DuplicateKey) && InputController._GetKey(ModifierKey))
            //{
            //    if (RuntimeEditorApplication.IsActiveWindow(m_projectResources))
            //    {
            //        DuplicateProjectResources();
            //    }
            //}
        }

        private void OnCreate(int index)
        {
            if(m_ddCreate.value != 0)
            {
                if(m_ddCreate.value == 1)
                {
                    CreateFolder();
                }
            }

            m_ddCreate.value = 0;
        }

        private void CreateFolder()
        {
            Debug.LogWarning("CreateFolder");
        }

        private void SelectLibrary()
        {
            AssetLibrarySelectDialog assetLibrarySelector = Instantiate(m_assetLibrarySelectorPrefab);
            assetLibrarySelector.transform.position = Vector3.zero;

            PopupWindow.Show("Import Asset Library", assetLibrarySelector.transform, "Select",
                args =>
                {
                    Import(assetLibrarySelector.SelectedAssetLibrary);
                },
                "Cancel");
        }

        private void Import(string assetLibrary)
        {
            AssetLibraryImportDialog assetLibraryImporter = Instantiate(m_assetLibraryImportPrefab);
            assetLibraryImporter.transform.position = Vector3.zero;
            assetLibraryImporter.SelectedAssetLibrary = assetLibrary;

            PopupWindow.Show("Select Assets", assetLibraryImporter.transform, "Import",
                args =>
                {
                    Editor.IsBusy = true;
                    m_project.Import(assetLibraryImporter.SelectedAssets, error =>
                    {
                        Editor.IsBusy = false;
                        if (error.HasError)
                        {
                            PopupWindow.Show("Unable to Import assets", error.ErrorText, "OK");
                        }

                        string path = string.Empty;
                        if(m_projectTree.SelectedFolder != null)
                        {
                            path = m_projectTree.SelectedFolder.ToString();
                        }
                        
                        m_projectTree.LoadProject(m_project.Root);

                        if(!string.IsNullOrEmpty(path))
                        {
                            m_projectTree.SelectedFolder = m_project.Root.Get(path);
                        }
                        else
                        {
                            m_projectTree.SelectedFolder = m_project.Root;
                        }
                        
                    });
                },
                "Cancel");
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
            
            m_project.Delete(e.ProjectItems, error =>
            {
                Editor.IsBusy = false;
                if (error.HasError)
                {
                    PopupWindow.Show("Unable to remove", error.ErrorText, "OK");
                }

                m_projectTree.RemoveProjectItemsFromTree(e.ProjectItems);
            });
        }

        private void OnProjectResourcesDrop(object sender, ProjectTreeEventArgs e)
        {
            m_projectTree.ChangeParent(e.ProjectItems);
        }

        private void OnProjectResourcesDoubleClick(object sender, ProjectTreeEventArgs e)
        {
            m_projectTree.SelectedFolder = e.ProjectItem;
        }

        private void OnProjectResourcesRenamed(object sender, ProjectTreeRenamedEventArgs e)
        {
            m_projectTree.UpdateProjectItem(e.ProjectItem);

            Editor.IsBusy = true;
            m_project.Rename(e.ProjectItem, e.OldName, error =>
            {
                Editor.IsBusy = false;
                if (error.HasError)
                {
                    PopupWindow.Show("Unable to rename asset", error.ToString(), "OK");
                }
            });
        }

        private void OnProjectTreeItemRenamed(object sender, ProjectTreeRenamedEventArgs e)
        {
            Editor.IsBusy = true;
            m_project.Rename(e.ProjectItem, e.OldName, error =>
            {
                Editor.IsBusy = false;
                if(error.HasError)
                {
                    PopupWindow.Show("Unable to rename asset", error.ToString(), "OK");
                }
            });
        }

        private void OnProjectTreeItemDeleted(object sender, ProjectTreeEventArgs e)
        {
            Editor.IsBusy = true;
            m_project.Delete(e.ProjectItems, error =>
            {
                Editor.IsBusy = false;
                if (error.HasError)
                {
                    PopupWindow.Show("Unable to remove", error.ErrorText, "OK");
                }
            });
        }


        //private void OnProjectResourcesBeginDrag(object sender, ProjectResourcesEventArgs e)
        //{
        //    if(!e.ItemObjectPair.ProjectItem.IsExposedFromEditor)
        //    {
        //        m_projectTree.BeginDragProjectItem(e.ItemObjectPair.ProjectItem);
        //    }

        //}

        //private void OnProjectResourcesDrop(object sender, ProjectResourcesEventArgs e)
        //{
        //    if (e.ItemObjectPair.ProjectItem.IsExposedFromEditor)
        //    {
        //        return;
        //    }
        //    ProjectItem dragItem = e.ItemObjectPair.ProjectItem;
        //    ProjectItem dropTarget = m_projectTree.BeginDropProjectItem();
        //    if(dragItem == dropTarget)
        //    {
        //        return;
        //    }

        //    if(dropTarget != null)
        //    {
        //        if(dropTarget.Children != null && dropTarget.Children.Any(c => c.NameExt == dragItem.NameExt))
        //        {
        //            return;
        //        }

        //        m_projectManager.Move(new[] { dragItem }, dropTarget, () =>
        //        {
        //            if(dragItem.IsFolder)
        //            {
        //                m_projectTree.DropProjectItem(dragItem, dropTarget);
        //            }
        //            if (m_projectTree.SelectedFolder.Children == null ||
        //                !m_projectTree.SelectedFolder.Children.Contains(e.ItemObjectPair.ProjectItem))
        //            {
        //                m_projectResources.RemoveProjectItem(e.ItemObjectPair);
        //            }
        //        });
        //    }
        //}

        //private void OnProjectTreeItemDrop(object sender, ItemDropArgs e)
        //{

        //    ProjectItem[] dragItems = e.DragItems.OfType<ProjectItem>().ToArray();
        //    ProjectItem dropTarget = (ProjectItem)e.DropTarget;
        //    m_projectManager.Move(dragItems, dropTarget, () =>
        //    {
        //        for (int i = 0; i < e.DragItems.Length; ++i)
        //        {
        //            ProjectItem dragItem = (ProjectItem)e.DragItems[i];

        //            m_projectTree.DropProjectItem(dragItem, dropTarget);

        //            if (m_projectTree.SelectedFolder != null &&
        //                (m_projectTree.SelectedFolder.Children == null ||
        //                !m_projectTree.SelectedFolder.Children.Contains(dragItem)))
        //            {
        //                m_projectResources.RemoveProjectItem(dragItem);
        //            }
        //        }
        //    });
        //}

    }
}
