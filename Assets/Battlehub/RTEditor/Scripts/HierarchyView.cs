using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using Battlehub.UIControls;
using Battlehub.RTCommon;
using Battlehub.RTSaveLoad2;
using Battlehub.Utils;

namespace Battlehub.RTEditor
{
    public class HierarchyView : RuntimeEditorWindow
    {
        public GameObject TreeViewPrefab;
        private VirtualizingTreeView m_treeView;
        public Color DisabledItemColor = new Color(0.5f, 0.5f, 0.5f);
        public Color EnabledItemColor = new Color(0.2f, 0.2f, 0.2f);
        public UnityEvent ItemDoubleClick;
        private bool m_lockSelection;
        private bool m_isSpawningPrefab;
        private IProject m_project;
        private ITypeMap m_typeMap;
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

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            if (!TreeViewPrefab)
            {
                Debug.LogError("Set TreeViewPrefab field");
                return;
            }

            m_treeView = Instantiate(TreeViewPrefab).GetComponent<VirtualizingTreeView>();
            m_treeView.transform.SetParent(transform, false);
            //m_treeView.RemoveKey = KeyCode.None;
            
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

            m_project = RTSL2Deps.Get.Project;
            m_typeMap = RTSL2Deps.Get.TypeMap;
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
            ExposeToEditor.Awaked -= OnObjectAwaked;
            ExposeToEditor.Started -= OnObjectStarted;
            ExposeToEditor.Enabled -= OnObjectEnabled;
            ExposeToEditor.Disabled -= OnObjectDisabled;
            ExposeToEditor.Destroying -= OnObjectDestroying;
            ExposeToEditor.Destroyed -= OnObjectDestroyed;
            ExposeToEditor.MarkAsDestroyedChanged -= OnObjectMarkAsDestroyedChanged;
            ExposeToEditor.ParentChanged -= OnParentChanged;
            ExposeToEditor.NameChanged -= OnNameChanged;
            RuntimeEditorApplication.PlaymodeStateChanged -= OnPlaymodeStateChanged;
            //RuntimeTools.SpawnPrefabChanged -= OnSpawnPrefabChanged;

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
            if (!RuntimeEditorApplication.IsPointerOverWindow(this))
            {
                return;
            }

            if(InputController._GetKeyDown(SelectAllKey))
            {
                if(InputController._GetKey(ModifierKey))
                {
                    if(RuntimeEditorApplication.IsActiveWindow(this))
                    {
                        m_treeView.SelectedItems = m_treeView.Items;
                    }
                }
            }

            //if (RuntimeTools.SpawnPrefab == null)
            //{
            //    return;
            //}

            //m_treeView.ExternalItemDrag(InputController._MousePosition);

            //if (InputController._GetMouseButtonUp(0))
            //{
            //    m_isSpawningPrefab = true;
            //    GameObject prefabInstance = RuntimeTools.SpawnPrefab.InstantiatePrefab(Vector3.zero, Quaternion.identity);
            //    prefabInstance.SetActive(true);

            //    ExposeToEditor exposeToEditor = prefabInstance.GetComponent<ExposeToEditor>();
            //    if (exposeToEditor == null)
            //    {
            //        exposeToEditor = prefabInstance.AddComponent<ExposeToEditor>();
            //    }

            //    exposeToEditor.SetName(RuntimeTools.SpawnPrefab.name);
            //    RuntimeUndo.BeginRecord();
            //    RuntimeUndo.RecordSelection();
            //    RuntimeUndo.BeginRegisterCreateObject(prefabInstance);
            //    RuntimeUndo.EndRecord();

            //    bool isEnabled = RuntimeUndo.Enabled;
            //    RuntimeUndo.Enabled = false;
            //    RuntimeSelection.activeGameObject = prefabInstance;
            //    RuntimeUndo.Enabled = isEnabled;

            //    RuntimeUndo.BeginRecord();
            //    RuntimeUndo.RegisterCreatedObject(prefabInstance);
            //    RuntimeUndo.RecordSelection();
            //    RuntimeUndo.EndRecord();
            //}
        }

        private void EnableHierarchy()
        {
            BindGameObjects();
            m_lockSelection = true;
            m_treeView.SelectedItems = RuntimeSelection.gameObjects;
            m_lockSelection = false;

            RuntimeSelection.SelectionChanged += OnRuntimeSelectionChanged;
            ExposeToEditor.Awaked += OnObjectAwaked;
            ExposeToEditor.Started += OnObjectStarted;
            ExposeToEditor.Enabled += OnObjectEnabled;
            ExposeToEditor.Disabled += OnObjectDisabled;
            ExposeToEditor.Destroying += OnObjectDestroying;
            ExposeToEditor.Destroyed += OnObjectDestroyed;
            ExposeToEditor.MarkAsDestroyedChanged += OnObjectMarkAsDestroyedChanged;
            ExposeToEditor.ParentChanged += OnParentChanged;
            ExposeToEditor.NameChanged += OnNameChanged;
            RuntimeEditorApplication.PlaymodeStateChanged += OnPlaymodeStateChanged;
        }

        private void DisableHierarchy()
        {
            RuntimeSelection.SelectionChanged -= OnRuntimeSelectionChanged;
            ExposeToEditor.Awaked -= OnObjectAwaked;
            ExposeToEditor.Started -= OnObjectStarted;
            ExposeToEditor.Enabled -= OnObjectEnabled;
            ExposeToEditor.Disabled -= OnObjectDisabled;
            ExposeToEditor.Destroying -= OnObjectDestroying;
            ExposeToEditor.Destroyed -= OnObjectDestroyed;
            ExposeToEditor.MarkAsDestroyedChanged -= OnObjectMarkAsDestroyedChanged;
            ExposeToEditor.ParentChanged -= OnParentChanged;
            ExposeToEditor.NameChanged -= OnNameChanged;
            RuntimeEditorApplication.PlaymodeStateChanged -= OnPlaymodeStateChanged;
        }

#warning SceneCreated/SceneLoadeing/SceneLoaded commented out

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
            IEnumerable<GameObject> gameObjects = RuntimeEditorApplication.IsPlaying ?
                ExposeToEditor.FindAll(ExposeToEditorObjectType.PlayMode) :
                ExposeToEditor.FindAll(ExposeToEditorObjectType.EditorMode);

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
            if(RuntimeEditor.Instance.GamePrefab != null)
            {
                BindGameObjects();
            }  
        }

        private void OnItemExpanding(object sender, VirtualizingItemExpandingArgs e)
        {
            GameObject gameObject = (GameObject)e.Item;
            ExposeToEditor exposeToEditor = gameObject.GetComponent<ExposeToEditor>();
            //gameObject.GetComponentsInChildren<ExposeToEditor>(true).Where(exp =>
            //    exp != exposeToEditor &&
            //    exp.transform.parent.GetComponentsInParent<ExposeToEditor>(true).FirstOrDefault() == exposeToEditor);


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

            if(RuntimeSelection.gameObjects == null)
            {
                m_treeView.SelectedItems = new GameObject[0];
            }
            else
            {
                m_treeView.SelectedItems = RuntimeSelection.gameObjects;
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
            RuntimeSelection.objects = selectableGameObjects;

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
                RuntimeSelection.activeObject = go;
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
                RuntimeUndo.BeginRecord();
                for (int i = 0; i < e.DragItems.Length; ++i)
                {
                    Transform dragT = ((GameObject)e.DragItems[i]).transform;
                    RuntimeUndo.RecordTransform(dragT, dragT.parent, dragT.GetSiblingIndex());
                    RuntimeUndo.RecordObject(dragT.gameObject, m_treeView.IndexOf(dragT.gameObject), RestoreIndexFromUndoRecord);
                }
                RuntimeUndo.EndRecord();
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
                    RuntimeUndo.BeginRecord();
                    for (int i = 0; i < e.DragItems.Length; ++i)
                    {
                        Transform dragT = ((GameObject)e.DragItems[i]).transform;
                        dragT.SetParent(dropT, true);
                        dragT.SetAsLastSibling();

                        RuntimeUndo.RecordTransform(dragT, dropT, dragT.GetSiblingIndex());
                        RuntimeUndo.RecordObject(dragT.gameObject, m_treeView.IndexOf(dragT.gameObject), RestoreIndexFromUndoRecord);
                    }
                    RuntimeUndo.EndRecord();
                }
                else if (e.Action == ItemDropAction.SetNextSibling)
                {
                    RuntimeUndo.BeginRecord();
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

                        RuntimeUndo.RecordTransform(dragT, dropT.parent, dragT.GetSiblingIndex());
                        RuntimeUndo.RecordObject(dragT.gameObject, m_treeView.IndexOf(dragT.gameObject), RestoreIndexFromUndoRecord);
                    }

                    RuntimeUndo.EndRecord();
                }
                else if (e.Action == ItemDropAction.SetPrevSibling)
                {
                    RuntimeUndo.BeginRecord();
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

                        RuntimeUndo.RecordTransform(dragT, dropT.parent, dragT.GetSiblingIndex());
                        RuntimeUndo.RecordObject(dragT.gameObject, m_treeView.IndexOf(dragT.gameObject), RestoreIndexFromUndoRecord);
                    }
                    RuntimeUndo.EndRecord();
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

        private void OnObjectAwaked(ExposeToEditor o)
        {
            if(o is ExposeToEditor)
            {
                if (m_isSpawningPrefab && m_treeView.DropAction != ItemDropAction.None)
                {
                    VirtualizingTreeViewItem treeViewItem = m_treeView.GetTreeViewItem(m_treeView.DropTarget);
                    GameObject dropTarget = (GameObject)m_treeView.DropTarget;
                    if (m_treeView.DropAction == ItemDropAction.SetLastChild)
                    {
                        o.transform.SetParent(dropTarget.transform);
                        if (m_treeView.IndexOf(o.gameObject) == -1)
                        {
                            m_treeView.AddChild(dropTarget, o.gameObject);
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

                        o.transform.SetParent(dropTarget.transform.parent);
                        o.transform.SetSiblingIndex(index);

                        TreeViewItemContainerData itemContainerData = (TreeViewItemContainerData)m_treeView.Insert(index, o.gameObject);
                        itemContainerData.Parent = treeViewItem.Parent;

                        

                        //TreeViewItem newTreeViewItem = (TreeViewItem)m_treeView.Insert(index, o.gameObject);
                        //newTreeViewItem.Parent = treeViewItem.Parent;
                    }
                }
                else
                {
                    ExposeToEditor obj = o;
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
            if(RuntimeEditorApplication.IsPlaymodeStateChanging)
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

            return dragObjects.OfType<AssetItem>().Count() > 0;
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
            DragDrop.SetCursor(KnownCursor.DropNowAllowed);
        }

        public override void Drag(object[] dragObjects, PointerEventData pointerEventData)
        {
            base.Drag(dragObjects, pointerEventData);
            m_treeView.ExternalItemDrag(pointerEventData.position);
            if (CanDrop(dragObjects))
            {
                DragDrop.SetCursor(KnownCursor.DropAllowed);
            }
            else
            {
                DragDrop.SetCursor(KnownCursor.DropNowAllowed);
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
                    
                    if(assetItem != null)
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

                                RuntimeUndo.BeginRecord();
                                RuntimeUndo.RecordSelection();
                                RuntimeUndo.BeginRegisterCreateObject(prefabInstance);
                                RuntimeUndo.EndRecord();

                                bool isEnabled = RuntimeUndo.Enabled;
                                RuntimeUndo.Enabled = false;
                                RuntimeSelection.activeGameObject = prefabInstance;
                                RuntimeUndo.Enabled = isEnabled;

                                RuntimeUndo.BeginRecord();
                                RuntimeUndo.RegisterCreatedObject(prefabInstance);
                                RuntimeUndo.RecordSelection();
                                RuntimeUndo.EndRecord();

                                m_isSpawningPrefab = false;
                            }
                        });
                    }
                }
                m_treeView.ExternalItemDrop();
            }


        }

        //protected override void OnPointerEnterOverride(PointerEventData eventData)
        //{
        //    base.OnPointerEnterOverride(eventData);
        //    if (RuntimeTools.SpawnPrefab != null)
        //    {
        //        m_treeView.ExternalBeginDrag(eventData.position);
        //    }
        //}

        //protected override void OnPointerExitOverride(PointerEventData eventData)
        //{
        //    base.OnPointerExitOverride(eventData);
        //    if(RuntimeTools.SpawnPrefab != null)
        //    {
        //        m_treeView.ExternalItemDrop();
        //    }
        //}


        //private void OnSpawnPrefabChanged(GameObject oldPrefab)
        //{
        //    if (RuntimeTools.SpawnPrefab == null)
        //    {
        //        m_treeView.ExternalItemDrop();
        //    }
        //}

    }
}

