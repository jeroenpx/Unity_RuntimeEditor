using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using Battlehub.UIControls;
using Battlehub.RTCommon;
using Battlehub.RTSaveLoad2.Interface;
using Battlehub.Utils;

namespace Battlehub.RTEditor
{
    public class HierarchyView : RuntimeWindow
    {
        public GameObject TreeViewPrefab;
        private VirtualizingTreeView m_treeView;
        public Color DisabledItemColor = new Color(0.5f, 0.5f, 0.5f);
        public Color EnabledItemColor = new Color(0.2f, 0.2f, 0.2f);
        public UnityEvent ItemDoubleClick;
        private bool m_lockSelection;
        private bool m_isSpawningPrefab;
        private bool m_isStarted;

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
        public KeyCode SelectAllKey = KeyCode.A;

        private IProject m_project;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            if (!TreeViewPrefab)
            {
                Debug.LogError("Set TreeViewPrefab field");
                return;
            }

            m_project = RTSL2Deps.Get.Project;

            m_treeView = Instantiate(TreeViewPrefab).GetComponent<VirtualizingTreeView>();
            m_treeView.transform.SetParent(transform, false);
                        
            m_treeView.ItemDataBinding += OnItemDataBinding;
            m_treeView.SelectionChanged += OnSelectionChanged;
            m_treeView.ItemsRemoved += OnItemsRemoved;
            m_treeView.ItemExpanding += OnItemExpanding;
            m_treeView.ItemBeginDrag += OnItemBeginDrag;
            m_treeView.ItemBeginDrop += OnItemBeginDrop;
            m_treeView.ItemDrop += OnItemDrop;
            m_treeView.ItemEndDrag += OnItemEndDrag;
            m_treeView.ItemDoubleClick += OnItemDoubleClicked;
            m_treeView.ItemBeginEdit += OnItemBeginEdit;
            m_treeView.ItemEndEdit += OnItemEndEdit;   
        }

        private void Start()
        {
            m_isStarted = true;
        }

        private void OnEnable()
        {
            if(m_project != null)
            {
                //m_project.SceneLoading += OnSceneLoading;
                //m_project.SceneLoaded += OnSceneLoaded;
                //m_project.SceneCreated += OnSceneCreated;
            }

            EnableHierarchy();    
        }

        private void OnDisable()
        {
            if (m_project != null)
            {
               // m_project.SceneLoading -= OnSceneLoading;
                //m_project.SceneLoaded -= OnSceneLoaded;
                //m_project.SceneCreated -= OnSceneCreated;
            }

            DisableHierarchy();
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            if (!m_treeView)
            {
                return;
            }
            m_treeView.ItemDataBinding -= OnItemDataBinding;
            m_treeView.SelectionChanged -= OnSelectionChanged;
            m_treeView.ItemsRemoved -= OnItemsRemoved;
            m_treeView.ItemExpanding -= OnItemExpanding;
            m_treeView.ItemBeginDrag -= OnItemBeginDrag;
            m_treeView.ItemBeginDrop -= OnItemBeginDrop;
            m_treeView.ItemDrop -= OnItemDrop;
            m_treeView.ItemEndDrag -= OnItemEndDrag;
            m_treeView.ItemDoubleClick -= OnItemDoubleClicked;
            m_treeView.ItemBeginEdit -= OnItemBeginEdit;
            m_treeView.ItemEndEdit -= OnItemEndEdit;          
        }

        private void OnApplicationQuit()
        {
            if(Editor != null)
            {
                Editor.Object.Awaked -= OnObjectAwaked;
                Editor.Object.Started -= OnObjectStarted;
                Editor.Object.Enabled -= OnObjectEnabled;
                Editor.Object.Disabled -= OnObjectDisabled;
                Editor.Object.Destroying -= OnObjectDestroying;
                Editor.Object.Destroyed -= OnObjectDestroyed;
                Editor.Object.MarkAsDestroyedChanged -= OnObjectMarkAsDestroyedChanged;
                Editor.Object.ParentChanged -= OnParentChanged;
                Editor.Object.NameChanged -= OnNameChanged;
                Editor.PlaymodeStateChanged -= OnPlaymodeStateChanged;
            }

            if(m_project != null)
            {
              //  m_project.SceneLoading -= OnSceneLoading;
               // m_project.SceneLoaded -= OnSceneLoaded;
               // m_project.SceneCreated -= OnSceneCreated;
            }            
        }

        protected override void UpdateOverride()
        {
            base.UpdateOverride();
            if (Editor.ActiveWindow != this)
            {
                return;
            }

            if(Editor.Input.GetKeyDown(SelectAllKey))
            {
                if (Editor.Input.GetKey(ModifierKey))
                {
                    m_treeView.SelectedItems = m_treeView.Items;
                }
            }
        }

        private void EnableHierarchy()
        {
            BindGameObjects();
            m_lockSelection = true;
            m_treeView.SelectedItems = Editor.Selection.gameObjects;
            m_lockSelection = false;

            Editor.Selection.SelectionChanged += OnRuntimeSelectionChanged;

            Editor.Object.Awaked += OnObjectAwaked;
            Editor.Object.Started += OnObjectStarted;
            Editor.Object.Enabled += OnObjectEnabled;
            Editor.Object.Disabled += OnObjectDisabled;
            Editor.Object.Destroying += OnObjectDestroying;
            Editor.Object.Destroyed += OnObjectDestroyed;
            Editor.Object.MarkAsDestroyedChanged += OnObjectMarkAsDestroyedChanged;
            Editor.Object.ParentChanged += OnParentChanged;
            Editor.Object.NameChanged += OnNameChanged;

            Editor.PlaymodeStateChanged += OnPlaymodeStateChanged;
        }

        private void DisableHierarchy()
        {
            if(Editor != null)
            {
                Editor.Selection.SelectionChanged -= OnRuntimeSelectionChanged;

                Editor.Object.Awaked -= OnObjectAwaked;
                Editor.Object.Started -= OnObjectStarted;
                Editor.Object.Enabled -= OnObjectEnabled;
                Editor.Object.Disabled -= OnObjectDisabled;
                Editor.Object.Destroying -= OnObjectDestroying;
                Editor.Object.Destroyed -= OnObjectDestroyed;
                Editor.Object.MarkAsDestroyedChanged -= OnObjectMarkAsDestroyedChanged;
                Editor.Object.ParentChanged -= OnParentChanged;
                Editor.Object.NameChanged -= OnNameChanged;

                Editor.PlaymodeStateChanged -= OnPlaymodeStateChanged;
            }
        }

#warning SceneCreated/SceneLoading/SceneLoaded commented out

        //private void OnSceneCreated(object sender, ProjectManagerEventArgs args)
        //{
        //    DisableHierarchy();
        //    EnableHierarchy();
        //}

        //private void OnSceneLoading(object sender, ProjectManagerEventArgs args)
        //{
        //    DisableHierarchy();
        //}

        //private void OnSceneLoaded(object sender, ProjectManagerEventArgs args)
        //{
        //    EnableHierarchy();
        //}

        private void BindGameObjects()
        {
            IEnumerable<GameObject> gameObjects = Editor.IsPlaying ?
                ExposeToEditor.FindAll(Editor, ExposeToEditorObjectType.PlayMode) :
                ExposeToEditor.FindAll(Editor, ExposeToEditorObjectType.EditorMode);

            if(gameObjects.Any())
            {
                Transform commonParent = gameObjects.First().transform.parent;
                foreach(GameObject go in gameObjects)
                {
                    if(go.transform.parent != commonParent)
                    {
                        Debug.LogWarning("ExposeToEditor objects have different parents, hierarchy may not work correctly.");
                        break;
                    }
                }
            }
            m_treeView.Items = gameObjects.OrderBy(g => g.transform.GetSiblingIndex());
        }

        private void OnPlaymodeStateChanged()
        {
            //if(RuntimeEditor.Instance.GamePrefab != null)
            //{
            //    BindGameObjects();
            //}  
        }

        private void OnItemExpanding(object sender, VirtualizingItemExpandingArgs e)
        {
            GameObject gameObject = (GameObject)e.Item;
            ExposeToEditor exposeToEditor = gameObject.GetComponent<ExposeToEditor>();
            
            if (exposeToEditor.ChildCount > 0)
            {
                e.Children = exposeToEditor.GetChildren().Where(obj => !obj.MarkAsDestroyed).Select(obj => obj.gameObject);

                //This line is required to syncronize selection, runtime selection and treeview selection
                OnTreeViewSelectionChanged(m_treeView.SelectedItems, m_treeView.SelectedItems);
            }
            else
            {
                e.Children = new GameObject[0];
            }
        }

        private void OnRuntimeSelectionChanged(Object[] unselected)
        {
            if (m_lockSelection)
            {
                return;
            }
            m_lockSelection = true;

            if(Editor.Selection.gameObjects == null)
            {
                m_treeView.SelectedItems = new GameObject[0];
            }
            else
            {
                m_treeView.SelectedItems = Editor.Selection.gameObjects;
            }
            
            m_lockSelection = false;
        }

        private void OnSelectionChanged(object sender, SelectionChangedArgs e)
        {
            OnTreeViewSelectionChanged(e.OldItems, e.NewItems);
        }

        private void OnTreeViewSelectionChanged(IEnumerable oldItems, IEnumerable newItems)
        {
            if (m_lockSelection)
            {
                return;
            }

            m_lockSelection = true;

            if (newItems == null)
            {
                newItems = new GameObject[0];
            }
            GameObject[] selectableGameObjects = newItems.OfType<GameObject>().Where(g => g.GetComponent<ExposeToEditor>() && g.GetComponent<ExposeToEditor>().CanSelect).ToArray();
            Editor.Selection.objects = selectableGameObjects;

            //sync with RunitimeSelectiom.objects because of OnBeforeSelectionChanged event
            m_treeView.SelectedItems = selectableGameObjects;

            m_lockSelection = false;
        }

        private void OnItemsRemoved(object sender, ItemsRemovedArgs e)
        {
            //Removal handled in RuntimeEditor class
        }

        private void OnItemDataBinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            GameObject dataItem = e.Item as GameObject;
            if (dataItem != null)
            {
                Text text = e.ItemPresenter.GetComponentInChildren<Text>(true);
                text.text = dataItem.name;
                if (dataItem.activeInHierarchy)
                {
                    text.color = EnabledItemColor;
                }
                else
                {
                    text.color = DisabledItemColor;
                }

                e.HasChildren = dataItem.GetComponent<ExposeToEditor>().ChildCount > 0;
            }
        }

        private void OnItemDoubleClicked(object sender, ItemArgs e)
        {
            GameObject go = (GameObject)e.Items[0];
            ExposeToEditor exposeToEditor = go.GetComponent<ExposeToEditor>();
            if (exposeToEditor.CanSelect)
            {
                Editor.Selection.activeObject = go;
                ItemDoubleClick.Invoke();
            }
        }

        private void OnItemBeginEdit(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            GameObject dataItem = e.Item as GameObject;
            if (dataItem != null)
            {
                InputField inputField = e.EditorPresenter.GetComponentInChildren<InputField>(true);
                inputField.text = dataItem.name;
                inputField.ActivateInputField();
                inputField.Select();
                LayoutElement layout = inputField.GetComponent<LayoutElement>();

                Text text = e.ItemPresenter.GetComponentInChildren<Text>(true);
                text.text = dataItem.name;

                RectTransform rt = text.GetComponent<RectTransform>();
                layout.preferredWidth = rt.rect.width;
            }
        }

        private void OnItemEndEdit(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            GameObject dataItem = e.Item as GameObject;
            if (dataItem != null)
            {
                
                InputField inputField = e.EditorPresenter.GetComponentInChildren<InputField>(true);
                if (!string.IsNullOrEmpty(inputField.text))
                {
                    dataItem.name = inputField.text;
                    Text text = e.ItemPresenter.GetComponentInChildren<Text>(true);
                    text.text = dataItem.name;
                }
                else
                {
                    inputField.text = dataItem.name;
                }
            }

            //Following code is required to unfocus inputfield if focused and release InputManager
            if (EventSystem.current != null && !EventSystem.current.alreadySelecting)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        private void OnItemBeginDrag(object sender, ItemArgs e)
        {
        }

        private void OnItemBeginDrop(object sender, ItemDropCancelArgs e)
        {
            if(e.IsExternal)
            {

            }
            else
            {
                Editor.Undo.BeginRecord();
                for (int i = 0; i < e.DragItems.Length; ++i)
                {
                    Transform dragT = ((GameObject)e.DragItems[i]).transform;
                    Editor.Undo.RecordTransform(dragT, dragT.parent, dragT.GetSiblingIndex());
                    Editor.Undo.RecordObject(dragT.gameObject, m_treeView.IndexOf(dragT.gameObject), RestoreIndexFromUndoRecord);
                }
                Editor.Undo.EndRecord();
            }
        }

        private void OnItemDrop(object sender, ItemDropArgs e)
        {
            if (e.IsExternal)
            {
                
            }
            else
            {
                Transform dropT = ((GameObject)e.DropTarget).transform;
                if (e.Action == ItemDropAction.SetLastChild)
                {
                    Editor.Undo.BeginRecord();
                    for (int i = 0; i < e.DragItems.Length; ++i)
                    {
                        Transform dragT = ((GameObject)e.DragItems[i]).transform;
                        dragT.SetParent(dropT, true);
                        dragT.SetAsLastSibling();

                        Editor.Undo.RecordTransform(dragT, dropT, dragT.GetSiblingIndex());
                        Editor.Undo.RecordObject(dragT.gameObject, m_treeView.IndexOf(dragT.gameObject), RestoreIndexFromUndoRecord);
                    }
                    Editor.Undo.EndRecord();
                }
                else if (e.Action == ItemDropAction.SetNextSibling)
                {
                    Editor.Undo.BeginRecord();
                    for (int i = e.DragItems.Length - 1; i >= 0; --i)
                    {
                        Transform dragT = ((GameObject)e.DragItems[i]).transform;
                        int dropTIndex = dropT.GetSiblingIndex();
                        if (dragT.parent != dropT.parent)
                        {
                            dragT.SetParent(dropT.parent, true);
                            dragT.SetSiblingIndex(dropTIndex + 1);
                        }
                        else
                        {
                            int dragTIndex = dragT.GetSiblingIndex();
                            if (dropTIndex < dragTIndex)
                            {
                                dragT.SetSiblingIndex(dropTIndex + 1);
                            }
                            else
                            {
                                dragT.SetSiblingIndex(dropTIndex);
                            }
                        }

                        Editor.Undo.RecordTransform(dragT, dropT.parent, dragT.GetSiblingIndex());
                        Editor.Undo.RecordObject(dragT.gameObject, m_treeView.IndexOf(dragT.gameObject), RestoreIndexFromUndoRecord);
                    }

                    Editor.Undo.EndRecord();
                }
                else if (e.Action == ItemDropAction.SetPrevSibling)
                {
                    Editor.Undo.BeginRecord();
                    for (int i = 0; i < e.DragItems.Length; ++i)
                    {
                        Transform dragT = ((GameObject)e.DragItems[i]).transform;
                        if (dragT.parent != dropT.parent)
                        {
                            dragT.SetParent(dropT.parent, true);
                        }

                        int dropTIndex = dropT.GetSiblingIndex();
                        int dragTIndex = dragT.GetSiblingIndex();
                        if (dropTIndex > dragTIndex)
                        {
                            dragT.SetSiblingIndex(dropTIndex - 1);
                        }
                        else
                        {
                            dragT.SetSiblingIndex(dropTIndex);
                        }

                        Editor.Undo.RecordTransform(dragT, dropT.parent, dragT.GetSiblingIndex());
                        Editor.Undo.RecordObject(dragT.gameObject, m_treeView.IndexOf(dragT.gameObject), RestoreIndexFromUndoRecord);
                    }
                    Editor.Undo.EndRecord();
                }
            }
        }

        private bool RestoreIndexFromUndoRecord(Record record)
        {
            int currentIndex = m_treeView.IndexOf(record.Target);

            int index = (int)record.State;
            bool hasChanged = currentIndex != index;

            if (hasChanged)
            {
                m_treeView.SetIndex(record.Target, index);
                m_treeView.UpdateIndent(record.Target);
            }
            return false;
        }

        private void OnItemEndDrag(object sender, ItemArgs e)
        {
        }

        private void OnObjectAwaked(ExposeToEditor obj)
        {
            if (m_isSpawningPrefab && m_treeView.DropAction != ItemDropAction.None)
            {
                VirtualizingTreeViewItem treeViewItem = m_treeView.GetTreeViewItem(m_treeView.DropTarget);
                GameObject dropTarget = (GameObject)m_treeView.DropTarget;
                if (m_treeView.DropAction == ItemDropAction.SetLastChild)
                {
                    obj.transform.SetParent(dropTarget.transform);
                    if (m_treeView.IndexOf(obj.gameObject) == -1)
                    {
                        m_treeView.AddChild(dropTarget, obj.gameObject);
                    }
                    treeViewItem.CanExpand = true;
                    treeViewItem.IsExpanded = true;
                }
                else
                {
                    int index;
                    if (m_treeView.DropAction == ItemDropAction.SetNextSibling)
                    {
                        index = m_treeView.IndexOf(dropTarget) + 1;
                    }
                    else
                    {
                        index = m_treeView.IndexOf(dropTarget);
                    }

                    obj.transform.SetParent(dropTarget.transform.parent);
                    obj.transform.SetSiblingIndex(index);

                    TreeViewItemContainerData itemContainerData = (TreeViewItemContainerData)m_treeView.Insert(index, obj.gameObject);
                    itemContainerData.Parent = treeViewItem.Parent;
                }
            }
            else
            {
                GameObject parent = null;

                if (obj.Parent != null)
                {
                    parent = obj.Parent.gameObject;
                }
                if (m_treeView.IndexOf(obj.gameObject) == -1)
                {
                    m_treeView.AddChild(parent, obj.gameObject);
                }
            }

            m_isSpawningPrefab = false;
        }

        private void OnObjectStarted(ExposeToEditor obj)
        {

        }

        private void OnObjectEnabled(ExposeToEditor obj)
        {
            VirtualizingTreeViewItem tvItem = m_treeView.GetTreeViewItem(obj.gameObject);
            if (tvItem == null)
            {
                return;
            }
            Text text = tvItem.GetComponentInChildren<Text>();
            text.color = EnabledItemColor;
        }

        private void OnObjectDisabled(ExposeToEditor obj)
        {
            VirtualizingTreeViewItem tvItem = m_treeView.GetTreeViewItem(obj.gameObject);
            if (tvItem == null)
            {
                return;
            }
            Text text = tvItem.GetComponentInChildren<Text>();
            text.color = DisabledItemColor;
        }

        private void OnObjectDestroying(ExposeToEditor o)
        {
            GameObject parent = null;
            bool isLastChild = false;
            if (o.Parent != null)
            {
                parent = o.Parent.gameObject;
                isLastChild = (o.Parent.ChildCount - o.Parent.MarkedAsDestroyedChildCount) <= 1; //ChildCount is not yet decremented
            }

            m_treeView.RemoveChild(parent, o.gameObject, isLastChild);
        }

        private void OnObjectDestroyed(ExposeToEditor o)
        {
           
        }

        private void OnObjectMarkAsDestroyedChanged(ExposeToEditor o)
        {
            if (o.MarkAsDestroyed)
            {
                GameObject parent = null;
                bool isLastChild = false;
                if (o.Parent != null)
                {
                    parent = o.Parent.gameObject;
                    isLastChild = (o.Parent.ChildCount - o.Parent.MarkedAsDestroyedChildCount) <= 0;
                }

                m_treeView.RemoveChild(parent, o.gameObject, isLastChild);
            }
            else
            {
                GameObject parent = null;

                if (o.Parent != null)
                {
                    parent = o.Parent.gameObject;
                }

                ExposeToEditor nextSibling = o.NextSibling();

                m_treeView.AddChild(parent, o.gameObject); //TODO: replace with Insert                    
                 
                if(nextSibling != null)
                {
                    m_treeView.SetPrevSibling(nextSibling.gameObject, o.gameObject);
                }
                
            }
        }

        private void OnParentChanged(ExposeToEditor obj, ExposeToEditor oldParent, ExposeToEditor newParent)
        {
            if(!m_isStarted)
            {
                return;
            }
            if(Editor.IsPlaymodeStateChanging)
            {
                return;
            }
            GameObject newParentGO = null;
            GameObject oldParentGO = null;
            bool isNewParentExpanded = true;
            bool isOldParentExpanded = true;
            bool isLastChild = false;
            if (newParent != null)
            {
                newParentGO = newParent.gameObject;
                isNewParentExpanded = m_treeView.IsExpanded(newParentGO);
            }

            if (oldParent != null)
            {
                oldParentGO = oldParent.gameObject;
                isLastChild = (oldParent.ChildCount - oldParent.MarkedAsDestroyedChildCount) <= 1;
                isOldParentExpanded = m_treeView.IsExpanded(oldParentGO);
            }

            if (isNewParentExpanded)
            {
                m_treeView.ChangeParent(newParentGO, obj.gameObject);
                if (!isOldParentExpanded)
                {
                    if (isLastChild)
                    {
                        VirtualizingTreeViewItem oldParentContainer = m_treeView.GetTreeViewItem(oldParentGO);
                        if (oldParentContainer)
                        {
                            oldParentContainer.CanExpand = false;
                        }
                    }
                }
            }
            else
            {   
                if(newParentGO != null)
                {
                    VirtualizingTreeViewItem newParentTreeViewItem = m_treeView.GetTreeViewItem(newParentGO);
                    if(newParentTreeViewItem != null)
                    {
                        newParentTreeViewItem.CanExpand = true;
                    }
                }

                m_treeView.RemoveChild(oldParentGO, obj.gameObject, isLastChild);
            }
        }

        private void OnNameChanged(ExposeToEditor obj)
        {
            VirtualizingTreeViewItem tvItem = m_treeView.GetTreeViewItem(obj.gameObject);
            if (tvItem == null)
            {
                return;
            }
            Text text = tvItem.GetComponentInChildren<Text>();
            text.text = obj.gameObject.name;
        }

        private bool CanDrop(object[] dragObjects)
        {
            if(m_treeView.DropTarget == null)
            {
                return false;
            }

            IEnumerable<AssetItem> assetItems = dragObjects.OfType<AssetItem>();

            return assetItems.Count() > 0 && assetItems.Any(assetItem => m_project.ToType(assetItem) == typeof(GameObject));
        }

        public override void DragEnter(object[] dragObjects, PointerEventData pointerEventData)
        {
            base.DragEnter(dragObjects, pointerEventData);
            m_treeView.ExternalBeginDrag(pointerEventData.position);
        }

        public override void DragLeave(PointerEventData pointerEventData)
        {
            base.DragLeave(pointerEventData);
            m_treeView.ExternalItemDrop();
            Editor.DragDrop.SetCursor(KnownCursor.DropNowAllowed);
        }

        public override void Drag(object[] dragObjects, PointerEventData pointerEventData)
        {
            base.Drag(dragObjects, pointerEventData);
            m_treeView.ExternalItemDrag(pointerEventData.position);
            if (CanDrop(dragObjects))
            {
                Editor.DragDrop.SetCursor(KnownCursor.DropAllowed);
            }
            else
            {
                Editor.DragDrop.SetCursor(KnownCursor.DropNowAllowed);
                m_treeView.ClearTarget();
            }
        }

        public override void Drop(object[] dragObjects, PointerEventData pointerEventData)
        {
            base.Drop(dragObjects, pointerEventData);
         
            m_isSpawningPrefab = true;
            if(CanDrop(dragObjects))
            {
                for (int i = 0; i < dragObjects.Length; ++i)
                {
                    object dragObject = dragObjects[i];
                    AssetItem assetItem = dragObject as AssetItem;
                    
                    if(assetItem != null && m_project.ToType(assetItem) == typeof(GameObject))
                    {
                        m_project.Load(assetItem, (error, obj) =>
                        {
                            if(obj is GameObject)
                            {
                                GameObject prefab = (GameObject)obj;
                                bool wasPrefabEnabled = prefab.activeSelf;
                                prefab.SetActive(false);

                                GameObject prefabInstance = Instantiate(prefab, Vector3.zero, Quaternion.identity, ((GameObject)m_treeView.DropTarget).transform);

                                prefab.SetActive(wasPrefabEnabled);

                                ExposeToEditor exposeToEditor = prefabInstance.GetComponent<ExposeToEditor>();
                                if (exposeToEditor == null)
                                {
                                    exposeToEditor = prefabInstance.AddComponent<ExposeToEditor>();
                                }

                                exposeToEditor.SetName(obj.name);
                                exposeToEditor.Parent = ((GameObject)m_treeView.DropTarget).GetComponent<ExposeToEditor>();
                                prefabInstance.SetActive(true);

                                Editor.Undo.BeginRecord();
                                Editor.Undo.RecordSelection();
                                Editor.Undo.BeginRegisterCreateObject(prefabInstance);
                                Editor.Undo.EndRecord();

                                bool isEnabled = Editor.Undo.Enabled;
                                Editor.Undo.Enabled = false;
                                Editor.Selection.activeGameObject = prefabInstance;
                                Editor.Undo.Enabled = isEnabled;

                                Editor.Undo.BeginRecord();
                                Editor.Undo.RegisterCreatedObject(prefabInstance);
                                Editor.Undo.RecordSelection();
                                Editor.Undo.EndRecord();

                                m_isSpawningPrefab = false;
                            }
                        });
                    }
                }
            }

            m_treeView.ExternalItemDrop();
        }
    }
}

