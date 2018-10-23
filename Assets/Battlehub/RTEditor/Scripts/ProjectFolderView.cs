using Battlehub.RTCommon;
using Battlehub.RTHandles;
using Battlehub.RTSaveLoad2.Interface;
using Battlehub.UIControls;
using Battlehub.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using UnityObject = UnityEngine.Object;
namespace Battlehub.RTEditor
{
    /*
    public class ProjectResourcesEventArgs : EventArgs
    {
        public ProjectItemObjectPair[] ItemObjectPairs
        {
            get;
            private set;
        }

        public ProjectItemObjectPair ItemObjectPair
        {
            get
            {
                if(ItemObjectPairs == null || ItemObjectPairs.Length == 0)
                {
                    return null;
                }
                return ItemObjectPairs[0];
            }
        }

        public ProjectResourcesEventArgs(ProjectItemObjectPair[] itemObjectPairs)
        {
            ItemObjectPairs = itemObjectPairs;
        }
    }

    public class ProjectResourcesRenamedEventArgs : ProjectResourcesEventArgs
    {
        public string[] OldNames
        {
            get;
            private set;
        }

        public string OldName
        {
            get
            {
                if (OldNames == null || OldNames.Length == 0)
                {
                    return null;
                }
                return OldNames[0];
            }
        }

        public ProjectResourcesRenamedEventArgs(ProjectItemObjectPair[] itemObjectPairs, string[] oldNames)
            :base(itemObjectPairs)
        {
            OldNames = oldNames;
        }
    }
    */

    public class ProjectFolderView : RuntimeWindow
    {
        private IProject m_project;
   
        public Type TypeFilter;
        

        /*
        public event EventHandler<SelectionChangedArgs<ProjectItemObjectPair>> SelectionChanged;
        public event EventHandler<ProjectResourcesEventArgs> DoubleClick;
        public event EventHandler<ProjectResourcesRenamedEventArgs> Renamed;
        public event EventHandler<ProjectResourcesEventArgs> Deleted;
        public event EventHandler<ProjectResourcesEventArgs> BeginDrag;
        public event EventHandler<ProjectResourcesEventArgs> Drop;

        private IProjectManager m_projectManager;
        private Dictionary<ID, UnityObject> m_sceneObjectDictionary;
        */

        /*
        [NonSerialized]
        private UnityObject[] m_selectedItems;
        public UnityObject[] SelectedItems
        {
            get { return m_selectedItems; }
        }

        public void SetSelectedItems(ProjectItemObjectPair[] objects, ProjectItem[] projectItems)
        {
            ProjectItemObjectPair[] oldSelection = SelectionToProjectItemObjectPair(m_selectedItems);

            if (projectItems == null)
            {
                m_selectedItems = null;
                m_listBox.SelectedItems = null;
            }
            else
            {
                HashSet<string> paths = new HashSet<string>(projectItems.Select(p => p.ToString()));
                var selection = objects.Where(iop => paths.Contains(iop.ProjectItem.ToString()));
                m_selectedItems = selection.Select(iop => iop.Object).ToArray();
                if(SelectionChanged != null)
                {
                    SelectionChanged(this, new SelectionChangedArgs<ProjectItemObjectPair>(oldSelection, selection.ToArray(), false));
                }
            }
        }

        [NonSerialized]
        private ProjectItemObjectPair[] m_objects;
        public void SetObjects(ProjectItemObjectPair[] value, bool reload)
        {
            m_objects = value;
            m_lockSelection = true;
            DataBind(reload);
          
            m_lockSelection = false;
        }
        */
        private ProjectItem[] m_items;
        private ProjectItem[] m_folders;
        public void SetItems(ProjectItem[] folders, ProjectItem[] items, bool reload)
        {
            m_folders = folders;
            m_items = items;
            if(m_items != null)
            {
                m_items = m_items.Where(item => item.IsFolder).OrderBy(item => item.Name).Union(m_items.Where(item => !item.IsFolder).OrderBy(item => item.NameExt)).ToArray();
            }
            DataBind(true);
        }

        [SerializeField]
        private Texture2D DragIcon;
        [SerializeField]
        private GameObject ListBoxPrefab;
        private VirtualizingTreeView m_listBox;
        private bool m_lockSelection;

        public KeyCode RemoveKey = KeyCode.Delete;
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

        
        private void DataBind(bool clearItems)
        {
            if (m_items == null)
            {
                m_listBox.SelectedItems = null;
                m_listBox.Items = null;
            }
            else
            {
                if (clearItems)
                {
                    if (m_listBox == null)
                    {
                        Debug.LogError("ListBox is null");
                    }
                    m_listBox.Items = null;
                }

                m_listBox.SelectedItems = null;

                List<ProjectItem> itemsList = m_items.ToList();
                if (TypeFilter != null)
                {
                    for (int i = itemsList.Count - 1; i >= 0; i--)
                    {
                        ProjectItem item = itemsList[i];
                        if(item.IsFolder)
                        {
                            itemsList.Remove(item);
                        }
                        else
                        {
                            AssetItem assetItem = (AssetItem)item;
                            Type type = m_project.ToType(assetItem);
                            if(type == null)
                            {
                                itemsList.RemoveAt(i);
                            }
                            else if (!TypeFilter.IsAssignableFrom(type))
                            {
                                itemsList.RemoveAt(i);
                            }
                        }
                    }

                    if (typeof(GameObject) == TypeFilter)
                    {
                        IEnumerable<GameObject> sceneObjects = Editor.IsPlaying ?
                            ExposeToEditor.FindAll(Editor, ExposeToEditorObjectType.PlayMode) :
                            ExposeToEditor.FindAll(Editor, ExposeToEditorObjectType.EditorMode);

                        foreach (GameObject go in sceneObjects)
                        {
                            AssetItem sceneItem = new AssetItem();
                            sceneItem.ItemID = m_project.ToID(go);
                            sceneItem.Name = go.name;
                            sceneItem.Ext = m_project.GetExt(go);
                            sceneItem.TypeGuid = m_project.ToGuid(typeof(GameObject));
                            itemsList.Add(sceneItem);
                        }
                    }
                    else if (typeof(Component).IsAssignableFrom(TypeFilter))
                    {
                        IEnumerable<GameObject> sceneObjects = Editor.IsPlaying ?
                            ExposeToEditor.FindAll(Editor, ExposeToEditorObjectType.PlayMode) :
                            ExposeToEditor.FindAll(Editor, ExposeToEditorObjectType.EditorMode);

                        foreach (GameObject go in sceneObjects)
                        {
                            Component component = go.GetComponent(TypeFilter);
                            Guid typeGuid = m_project.ToGuid(component.GetType());
                            if (component != null && typeGuid != Guid.Empty)
                            {
                                AssetItem sceneItem = new AssetItem();
                                sceneItem.ItemID = m_project.ToID(go);
                                sceneItem.Name = go.name;
                                sceneItem.Ext = m_project.GetExt(go);
                                sceneItem.TypeGuid = typeGuid;

                                itemsList.Add(sceneItem);
                            }
                        }
                    }

                    //itemsList.Insert(0, none);
                    m_listBox.Items = itemsList;

                }
                else
                {
                    m_listBox.Items = itemsList;
                }


                //if (m_selectedItems != null)
                //{
                //    m_listBox.SelectedItems = SelectionToProjectItemObjectPair(m_selectedItems);
                //}
            }
        }

        protected override void AwakeOverride()
        {
            base.AwakeOverride();

            if (!ListBoxPrefab)
            {
                Debug.LogError("Set ListBoxPrefab field");
                return;
            }

            m_project = RTSL2Deps.Get.Project;

            m_listBox = GetComponentInChildren<VirtualizingTreeView>();
            if (m_listBox == null)
            {
                m_listBox = Instantiate(ListBoxPrefab).GetComponent<VirtualizingTreeView>();
                m_listBox.CanDrag = true;
                m_listBox.CanReorder = false;
                //m_listBox.MultiselectKey = KeyCode.None;
               // m_listBox.RangeselectKey = KeyCode.None;
                //m_listBox.RemoveKey = KeyCode.None;
                m_listBox.transform.SetParent(transform, false);
            }

            m_listBox.ItemDataBinding += OnItemDataBinding;
            m_listBox.ItemBeginDrag += OnItemBeginDrag;
            m_listBox.ItemDragEnter += OnItemDragEnter;
            m_listBox.ItemDrag += OnItemDrag;
            m_listBox.ItemDragExit += OnItemDragExit;
            m_listBox.ItemDrop += OnItemDrop;
            m_listBox.ItemEndDrag += OnItemEndDrag;
        }
        
        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();

            if(m_listBox != null)
            {
                m_listBox.ItemDataBinding -= OnItemDataBinding;
                m_listBox.ItemBeginDrag -= OnItemBeginDrag;
                m_listBox.ItemDragEnter -= OnItemDragEnter;
                m_listBox.ItemDrag -= OnItemDrag;
                m_listBox.ItemDragExit -= OnItemDragExit;
                m_listBox.ItemDrop -= OnItemDrop;
                m_listBox.ItemEndDrag -= OnItemEndDrag;
            }
        }

        private void OnItemBeginDrag(object sender, ItemArgs e)
        {
            Editor.DragDrop.RaiseBeginDrag(e.Items, e.PointerEventData);
        }

        private void OnItemDragEnter(object sender, ItemDropCancelArgs e)
        {
            if (e.DropTarget == null || e.DropTarget is AssetItem || e.DragItems != null && e.DragItems.Contains(e.DropTarget))
            {
                Editor.DragDrop.SetCursor(KnownCursor.DropNowAllowed);
                e.Cancel = true;
            }
            else
            {
                Editor.DragDrop.SetCursor(KnownCursor.DropAllowed);
            }
        }

        private void OnItemDrag(object sender, ItemArgs e)
        {
            Editor.DragDrop.RaiseDrag(e.PointerEventData);
        }

        private void OnItemDragExit(object sender, EventArgs e)
        {
            Editor.DragDrop.SetCursor(KnownCursor.DropNowAllowed);
        }

        private void OnItemDrop(object sender, ItemDropArgs e)
        {
            Editor.DragDrop.RaiseDrop(e.PointerEventData);

            if (!(e.DropTarget is AssetItem) && (e.DragItems == null || !e.DragItems.Contains(e.DropTarget)))
            {
                ProjectItem parent = (ProjectItem)e.DropTarget;
                foreach (ProjectItem item in e.DragItems)
                {
                    if (item.Parent != parent)
                    {
                        m_listBox.RemoveChild(item.Parent, item, item.Parent.Children.Count == 1);
                        parent.AddChild(item);
                    }
                }
            }
        }

        private void OnItemEndDrag(object sender, ItemArgs e)
        {
            Editor.DragDrop.RaiseDrop(e.PointerEventData);

            foreach (ProjectItem item in e.Items)
            {
                if (m_folders.All(f => f.Children == null || !f.Children.Contains(item)))
                {
                    m_listBox.RemoveChild(item.Parent, item, item.Parent.Children.Count == 1);
                }
            }
        }

        private void OnItemDataBinding(object sender, ItemDataBindingArgs e)
        {
            ProjectItem projectItem = e.Item as ProjectItem;
            if(projectItem == null)
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
                

                
                

                //projectItemView.Set(ProjectItemType.Scene, null);
                //if (pair.IsFolder || pair.IsScene)
                //{
                //    UnityObject obj = pair.Object;
                //    ProjectItemType itemType = ProjectItemType.None;
                //    if (pair.IsFolder)
                //    {
                //        if (pair.ProjectItem.IsExposedFromEditor)
                //        {
                //            itemType = ProjectItemType.ExposedFolder;
                //        }
                //        else
                //        {
                //            itemType = ProjectItemType.Folder;
                //        }

                //    }
                //    else if (pair.IsScene)
                //    {
                //        itemType = ProjectItemType.Scene;
                //    }

                //    rtResource.Set(itemType, null);
                //    text.text = pair.ProjectItem.Name;
                //}
                //else
                //{
                //    if (pair.ProjectItem != null && pair.ProjectItem.IsExposedFromEditor)
                //    {
                //        rtResource.Set(ProjectItemType.ExposedResource, pair.Object);
                //    }
                //    else
                //    {
                //        if (pair.Object is NoneItem)
                //        {
                //            rtResource.Set(ProjectItemType.None, pair.Object);
                //        }
                //        else
                //        {
                //            rtResource.Set(ProjectItemType.Resource, pair.Object);
                //        }

                //    }

                //    if (pair.ProjectItem != null)
                //    {
                //        text.text = pair.ProjectItem.Name;
                //    }
                //    else
                //    {
                //        text.text = pair.Object.name;
                //    }
                //}

                //if (pair.ProjectItem != null)
                //{
                //    ProjectItem item = pair.ProjectItem;
                //    e.CanEdit = !item.IsExposedFromEditor;
                //    if (item.IsFolder && item.IsScene)
                //    {
                //        e.CanDrag = !item.IsExposedFromEditor;
                //    }
                //    else
                //    {
                //        e.CanDrag = true;
                //    }
                //}
                //else
                //{
                //    e.CanDrag = false;
                //    e.CanEdit = false;
                //}
            }
        }


        /*
              


            m_listBox.SelectionChanged += OnSelectionChanged;
            m_listBox.ItemDoubleClick += OnItemDoubleClick;
            m_listBox.ItemBeginDrag += OnItemBeginDrag;
            m_listBox.ItemEndDrag += OnItemEndDrag;
            m_listBox.ItemBeginDrop += OnItemBeginDrop;
            m_listBox.ItemDrop += OnItemDrop;
            m_listBox.ItemsRemoving += OnItemsRemoving;
            m_listBox.ItemsRemoved += OnItemsRemoved;
            m_listBox.ItemBeginEdit += OnItemBeginEdit;
            m_listBox.ItemEndEdit += OnItemEndEdit;

            RuntimeSelection.SelectionChanged += OnRuntimeSelectionChanged;
            ExposeToEditor.Destroyed += OnObjectDestroyed;
            ExposeToEditor.MarkAsDestroyedChanged += OnObjectMarkAsDestroyedChanged;


            m_sceneObjectDictionary = new Dictionary<ID, UnityObject>();
            */
    }
    /*
            private void Start()
            {

            }

            private void OnEnable()
            {

            }

            private void OnDisable()
            {
                m_selectedItems = null;
                m_listBox.SelectedItem = null;
            }

            protected override void OnDestroyOverride()
            {
                base.OnDestroyOverride();
                Unsubscribe();
            }

            private void OnApplicationQuit()
            {
                Unsubscribe();
            }


            protected override void UpdateOverride()
            {
                base.UpdateOverride();
                if (!RuntimeEditorApplication.IsActiveWindow(this) && !RuntimeEditorApplication.IsActiveWindow(RuntimeWindowType.SceneView))
                {
                    return;
                }

                if (InputController._GetKeyDown(RemoveKey))
                {
                    if (m_listBox.SelectedItem != null)
                    {
                        PopupWindow.Show("Remove Selected assets", "You can not undo this action", "Delete", args =>
                        {
                            m_listBox.RemoveSelectedItems();
                        }, "Cancel");
                    }
                }
            }

            private void Unsubscribe()
            {
                if (m_listBox)
                {
                    m_listBox.ItemDataBinding -= OnItemDataBinding;
                    m_listBox.SelectionChanged -= OnSelectionChanged;
                    m_listBox.ItemDoubleClick -= OnItemDoubleClick;
                    m_listBox.ItemBeginDrag -= OnItemBeginDrag;
                    m_listBox.ItemEndDrag -= OnItemEndDrag;
                    m_listBox.ItemBeginDrop -= OnItemBeginDrop;
                    m_listBox.ItemDrop -= OnItemDrop;
                    m_listBox.ItemsRemoving -= OnItemsRemoving;
                    m_listBox.ItemsRemoved -= OnItemsRemoved;
                    m_listBox.ItemBeginEdit -= OnItemBeginEdit;
                    m_listBox.ItemEndEdit -= OnItemEndEdit;
                }

                RuntimeSelection.SelectionChanged -= OnRuntimeSelectionChanged;
                ExposeToEditor.Destroyed -= OnObjectDestroyed;
                ExposeToEditor.MarkAsDestroyedChanged -= OnObjectMarkAsDestroyedChanged;
            }

            public void UpdateProjectItem(ProjectItem projectItem)
            {
                ItemContainer itemContainer = m_listBox.GetItemContainer(projectItem);
                if (itemContainer != null)
                {
                    m_listBox.DataBindItem(projectItem, itemContainer);
                }
            }

            public void RemoveProjectItem(ProjectItem item)
            {
                ProjectItemObjectPair foundItemObjectPair = m_listBox.Items.OfType<ProjectItemObjectPair>().Where(itemObjectPair => itemObjectPair.ProjectItem.ToString() == item.ToString()).FirstOrDefault();
                if(foundItemObjectPair != null)
                {
                    m_listBox.Remove(foundItemObjectPair);
                }
            }

            public void RemoveProjectItem(ProjectItemObjectPair item)
            {
                m_listBox.Remove(item);
            }

            private void OnItemDoubleClick(object sender, ItemArgs e)
            {
                if(DoubleClick != null)
                {
                    DoubleClick(this, new ProjectResourcesEventArgs(e.Items.OfType<ProjectItemObjectPair>().ToArray()));
                }
            }

            private void OnItemBeginDrag(object sender, ItemArgs e)
            {
                CursorHelper.SetCursor(this, DragIcon, new Vector2(DragIcon.width / 2, DragIcon.height / 2), CursorMode.Auto);

                ItemContainer itemContainer = m_listBox.GetItemContainer(e.Items[0]);
                if (itemContainer != null)
                {
                    ResourcePreview runtimeResource = itemContainer.GetComponentInChildren<ResourcePreview>();
                    runtimeResource.BeginSpawn();
                }

                ProjectItemObjectPair objectItemPair = (ProjectItemObjectPair)e.Items[0];
                if (BeginDrag != null)
                {
                    BeginDrag(this, new ProjectResourcesEventArgs(new[] { objectItemPair }));
                }

                if (objectItemPair.IsResource)
                {
                    DragDrop.RaiseBeginDrag(new[] { objectItemPair.Object });
                }
            }


            private void OnItemBeginDrop(object sender, ItemDropCancelArgs e)
            {

            }

            private void OnItemDrop(object sender, ItemDropArgs e)
            {
                if(e.IsExternal)
                {
                    return;
                }
                CursorHelper.ResetCursor(this);

                ProjectItemObjectPair objectItemPair = (ProjectItemObjectPair)e.DragItems[0];
                CompleteSpawn(objectItemPair);

                if (objectItemPair.IsResource)
                {
                    DragDrop.RaiseDrop();
                }
            }

            private void OnItemEndDrag(object sender, ItemArgs e)
            {

                CursorHelper.ResetCursor(this);

                ProjectItemObjectPair objectItemPair = (ProjectItemObjectPair)e.Items[0];
                CompleteSpawn(objectItemPair);

                if (objectItemPair.IsResource)
                {
                    DragDrop.RaiseDrop();
                }
            }

            private void CompleteSpawn(object item)
            {
                ItemContainer itemContainer = m_listBox.GetItemContainer(item);
                if (itemContainer != null)
                {
                    ResourcePreview resource = itemContainer.GetComponentInChildren<ResourcePreview>();
                    resource.CompleteSpawn();
                }

                if (RuntimeEditorApplication.IsPointerOverWindow(RuntimeWindowType.Resources))
                {
                    RuntimeTools.SpawnPrefab = null;
                }

                if (Drop != null)
                {
                    Drop(this, new ProjectResourcesEventArgs(new [] { (ProjectItemObjectPair)item }));
                }
            }

            private void OnRuntimeSelectionChanged(UnityObject[] unselected)
            {
                if (m_lockSelection)
                {
                    return;
                }

                m_lockSelection = true;

                if (RuntimeSelection.objects != null && m_listBox.Items != null)
                {
                    ProjectItemObjectPair[] selection = SelectionToProjectItemObjectPair(RuntimeSelection.objects);

                    m_selectedItems = RuntimeSelection.objects.ToArray();
                    if (SelectionChanged != null)
                    {
                        ProjectItemObjectPair[] oldSelection = m_listBox.SelectedItems != null ? m_listBox.SelectedItems.OfType<ProjectItemObjectPair>().ToArray() : null;
                        m_listBox.SelectedItems = selection;
                        SelectionChanged(this, new SelectionChangedArgs<ProjectItemObjectPair>(oldSelection, selection.ToArray(), false));
                    }
                }
                else
                {
                    m_selectedItems = null;
                    if (SelectionChanged != null)
                    {
                        ProjectItemObjectPair[] oldSelection = m_listBox.SelectedItems != null ? m_listBox.SelectedItems.OfType<ProjectItemObjectPair>().ToArray() : null;
                        m_listBox.SelectedItems = null;
                        SelectionChanged(this, new SelectionChangedArgs<ProjectItemObjectPair>(oldSelection, null, false));
                    }
                }

                m_lockSelection = false;
            }

            public ProjectItemObjectPair[] SelectionToProjectItemObjectPair(UnityObject[] selectedObjects)
            {
                if(selectedObjects == null)
                {
                    return null;
                }


                ProjectItemObjectPair[] itemObjectPairs = m_listBox.Items.OfType<ProjectItemObjectPair>().ToArray();
                HashSet<ID> selectedIdentifiers = new HashSet<ID>(selectedObjects.Where(o => !(o is ProjectItemWrapper)).Select(o => m_projectManager.GetID(o)));
                HashSet<string> selectedPaths = new HashSet<string>(selectedObjects.OfType<ProjectItemWrapper>().Select(p => p.ProjectItem.ToString()));
                List<ProjectItemObjectPair> selection = new List<ProjectItemObjectPair>();
                for (int i = 0; i < itemObjectPairs.Length; ++i)
                {
                    ProjectItemObjectPair itemObjectPair = itemObjectPairs[i];
                    if (selectedIdentifiers.Contains(m_projectManager.GetID(itemObjectPair.Object)))
                    {
                        selection.Add(itemObjectPair);
                    }
                    else if(itemObjectPair.IsFolder || itemObjectPair.IsScene)
                    {
                        if (selectedPaths.Contains(itemObjectPair.ProjectItem.ToString()))
                        {
                            selection.Add(itemObjectPair);
                        }
                    }    
                }

                return selection.ToArray();
            }

            private void OnSelectionChanged(object sender, SelectionChangedArgs e)
            {
                if (m_lockSelection)
                {
                    return;
                }

                m_lockSelection = true;

                object[] newItems = e.NewItems;
                object[] oldItems = e.OldItems;

                if (newItems == null)
                {
                    newItems = new ProjectItemObjectPair[0];
                }

                if (oldItems == null)
                {
                    oldItems = new ProjectItemObjectPair[0];
                }

                ProjectItemObjectPair[] oldSelection = oldItems.OfType<ProjectItemObjectPair>().ToArray();
                ProjectItemObjectPair[] selection = newItems.OfType<ProjectItemObjectPair>().ToArray();
                if (SelectionChanged != null)
                {
                    SelectionChanged(this, new SelectionChangedArgs<ProjectItemObjectPair>(oldSelection, selection, true));
                }

                m_selectedItems = selection.Select(s => s.Object).ToArray();
                m_lockSelection = false;
            }

            private void OnItemDataBinding(object sender, ItemDataBindingArgs e)
            {
                ProjectItemObjectPair pair = e.Item as ProjectItemObjectPair;
                if (pair != null)
                {
                    Text text = e.ItemPresenter.GetComponentInChildren<Text>(true);
                    ResourcePreview rtResource = e.ItemPresenter.GetComponentInChildren<ResourcePreview>(true);

                    if (pair.IsFolder || pair.IsScene)
                    {
                        UnityObject obj = pair.Object;
                        ProjectItemType itemType = ProjectItemType.None;
                        if(pair.IsFolder)
                        {
                            if(pair.ProjectItem.IsExposedFromEditor)
                            {
                                itemType = ProjectItemType.ExposedFolder;
                            }
                            else
                            {
                                itemType = ProjectItemType.Folder;
                            }

                        }
                        else if(pair.IsScene)
                        {
                            itemType = ProjectItemType.Scene;
                        }

                        rtResource.Set(itemType, null);
                        text.text = pair.ProjectItem.Name;
                    }
                    else
                    {
                        if (pair.ProjectItem != null &&  pair.ProjectItem.IsExposedFromEditor)
                        {
                            rtResource.Set(ProjectItemType.ExposedResource, pair.Object);
                        }
                        else
                        {
                            if(pair.Object is NoneItem)
                            {
                                rtResource.Set(ProjectItemType.None, pair.Object);
                            }
                            else
                            {
                                rtResource.Set(ProjectItemType.Resource, pair.Object);
                            }

                        }

                        if(pair.ProjectItem != null)
                        {
                            text.text = pair.ProjectItem.Name;
                        }
                        else
                        {
                            text.text = pair.Object.name;
                        } 
                    }

                    if(pair.ProjectItem != null)
                    {
                        ProjectItem item = pair.ProjectItem;
                        e.CanEdit = !item.IsExposedFromEditor;
                        if (item.IsFolder && item.IsScene)
                        {
                            e.CanDrag = !item.IsExposedFromEditor;
                        }
                        else
                        {
                            e.CanDrag = true;
                        }
                    }
                    else
                    {
                        e.CanDrag = false;
                        e.CanEdit = false;
                    }
                }
            }

            private void OnItemsRemoving(object sender, ItemsCancelArgs e)
            {
                if (e.Items == null)
                {
                    return;
                }

                //if (!RuntimeEditorApplication.IsActiveWindow(this) && !Ru)
                //{
                //    e.Items.Clear();
                //    return;
                //}

                for (int i = e.Items.Count - 1; i >= 0; i--)
                {
                    ProjectItemObjectPair item = (ProjectItemObjectPair)e.Items[i];
                    if (item.ProjectItem.IsExposedFromEditor)
                    {
                        e.Items.Remove(item);
                    }
                }

                if (e.Items.Count == 0)
                {
                    PopupWindow.Show("Can't remove item", "Unable to remove folders & resources exposed from editor", "OK");
                }
            }

            private void OnItemsRemoved(object sender, ItemsRemovedArgs e)
            {
                ProjectItemObjectPair[] itemObjectPairs = e.Items.OfType<ProjectItemObjectPair>().ToArray();
                if (Deleted != null)
                {
                    Deleted(this, new ProjectResourcesEventArgs(itemObjectPairs));
                }
                for(int i = 0; i < itemObjectPairs.Length; ++i)
                {
                    ProjectItemObjectPair itemObjectPair = itemObjectPairs[i];
                    if(itemObjectPair.ProjectItem.Parent != null)
                    {
                        itemObjectPair.ProjectItem.Parent.RemoveChild(itemObjectPair.ProjectItem);
                    }
                }
            }

            private void OnItemBeginEdit(object sender, ItemDataBindingArgs e)
            {
                Text text = e.ItemPresenter.GetComponentInChildren<Text>(true);
                Image[] images = e.ItemPresenter.GetComponentsInChildren<Image>(true);

                InputField inputField = e.EditorPresenter.GetComponentInChildren<InputField>(true);
                inputField.ActivateInputField();
                inputField.text = text.text;

                Image[] editorImages = e.EditorPresenter.GetComponentsInChildren<Image>(true);
                for (int i = 0; i < images.Length; ++i)
                {
                    editorImages[i].sprite = images[i].sprite;
                    editorImages[i].gameObject.SetActive(true);
                }
            }

            private void OnItemEndEdit(object sender, ItemDataBindingArgs e)
            {
                InputField inputField = e.EditorPresenter.GetComponentInChildren<InputField>(true);
                Text text = e.ItemPresenter.GetComponentInChildren<Text>(true);

                ProjectItemObjectPair itemObjectPair = (ProjectItemObjectPair)e.Item;
                string oldName = itemObjectPair.ProjectItem.Name;
                if(itemObjectPair.ProjectItem.Parent != null)
                {
                    ProjectItem parentItem = itemObjectPair.ProjectItem.Parent;
                    string newNameExt = inputField.text.Trim() + "." + itemObjectPair.ProjectItem.Ext;
                    if (!string.IsNullOrEmpty(inputField.text.Trim()) && ProjectItem.IsValidName(inputField.text.Trim()) && !parentItem.Children.Any(p => p.NameExt == newNameExt))
                    {
                        string newName = inputField.text.Trim();
                        itemObjectPair.ProjectItem.Name = newName;
                        itemObjectPair.Object.name = newName;
                        text.text = newName;
                    }
                }

                if (Renamed != null)
                {
                    Renamed(this, new ProjectResourcesRenamedEventArgs(new[] { itemObjectPair }, new[] { oldName }));
                }

                //Following code is required to unfocus inputfield if focused and release InputManager
                if (EventSystem.current != null && !EventSystem.current.alreadySelecting)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }

            private void OnObjectDestroyed(ExposeToEditor obj)
            {
                m_listBox.Remove(obj.gameObject);
            }

            private void OnObjectMarkAsDestroyedChanged(ExposeToEditor obj)
            {

            }


            */

}
