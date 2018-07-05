using Battlehub.RTCommon.EditorTreeView;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Battlehub.RTSaveLoad2
{
    public class AssetLibraryAssetsGUI 
    {
        [NonSerialized]
        private bool m_Initialized;
        [SerializeField]
        private TreeViewState m_TreeViewState; // Serialized in the window layout file so it survives assembly reloading
        [SerializeField]
        private MultiColumnHeaderState m_MultiColumnHeaderState;
        private SearchField m_SearchField;
        private AssetFolderInfo[] m_folders;
        private AssetLibraryAsset m_asset;
        private const string kSessionStateKeyPrefix = "AssetLibraryAssetsTVS";

        public AssetLibraryAssetsGUI()
        {
        }

        internal AssetTreeView TreeView { get; private set; }

        public void SetTreeAsset(AssetLibraryAsset asset)
        {
            m_asset = asset;
            m_Initialized = false;
        }

        public void SetFolders(AssetFolderInfo[] folders)
        {
            m_folders = folders;
            m_Initialized = false;
        }

        private void InitIfNeeded()
        {
            if (!m_Initialized)
            {
                // Check if it already exists (deserialized from window layout file or scriptable object)
                if (m_TreeViewState == null)
                    m_TreeViewState = new TreeViewState();

                //var jsonState = SessionState.GetString(kSessionStateKeyPrefix + m_asset.GetInstanceID().ToString() + m_folder.id, "");
                //if (!string.IsNullOrEmpty(jsonState))
                //    JsonUtility.FromJsonOverwrite(jsonState, m_TreeViewState);

                bool firstInit = m_MultiColumnHeaderState == null;
                var headerState = AssetTreeView.CreateDefaultMultiColumnHeaderState(0);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_MultiColumnHeaderState, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(m_MultiColumnHeaderState, headerState);
                m_MultiColumnHeaderState = headerState;

                var multiColumnHeader = new MultiColumnHeader(headerState);
                if (firstInit)
                    multiColumnHeader.ResizeToFit();

                var treeModel = new TreeModel<AssetInfo>(GetData());

                TreeView = new AssetTreeView(
                    m_TreeViewState,
                    multiColumnHeader,
                    treeModel,
                    ExternalDropInside,
                    ExternalDropOutside);
                // m_TreeView.Reload();

                m_SearchField = new SearchField();
                m_SearchField.downOrUpArrowKeyPressed += TreeView.SetFocusAndEnsureSelectedItem;

                m_Initialized = true;
            }
        }

        public void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        public void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;

            //SessionState.SetString(kSessionStateKeyPrefix + m_asset.GetInstanceID().ToString() + m_folder.id, JsonUtility.ToJson(m_TreeView.state));
        }

        private void OnUndoRedoPerformed()
        {
            if (TreeView != null)
            {
                TreeView.treeModel.SetData(GetData());
                TreeView.Reload();
            }
        }

        private DragAndDropVisualMode CanDrop(TreeViewItem parent, int insertIndex)
        {
            AssetInfo parentAssetInfo = GetAssetInfo(parent);
            if (parentAssetInfo != TreeView.treeModel.root)
            {
                return DragAndDropVisualMode.None;
            }

            if(m_folders == null || m_folders.Length != 1)
            {
                return DragAndDropVisualMode.None;
            }

            bool allFolders = true;
            foreach (UnityObject dragged_object in DragAndDrop.objectReferences)
            {
                string path = AssetDatabase.GetAssetPath(dragged_object);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    allFolders = false;
                    break;
                }
            }

            if(allFolders)
            {
                return DragAndDropVisualMode.None;
            }

            AssetInfo parentAsset;
            if (parent == null)
            {
                parentAsset = TreeView.treeModel.root;
            }
            else
            {
                parentAsset = ((TreeViewItem<AssetInfo>)parent).data;
            }

            if (parentAsset.hasChildren)
            {
                var names = parentAsset.children.Select(c => c.name);
                if (DragAndDrop.objectReferences.Any(item => names.Contains(item.name)))
                {
                    return DragAndDropVisualMode.None;
                }
            }
            return DragAndDropVisualMode.Copy;
        }

        private DragAndDropVisualMode PerformDrop(TreeViewItem parent, int insertIndex)
        {
            DragAndDrop.AcceptDrag();

            foreach (UnityObject dragged_object in DragAndDrop.objectReferences)
            {
                string path = AssetDatabase.GetAssetPath(dragged_object);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    AssetFolderInfo folder = m_folders[0];
                    AddAssetToFolder(parent, insertIndex, dragged_object, folder);
                }
            }
            return DragAndDropVisualMode.Copy;
        }

        public void AddAssetToFolder(UnityObject obj, AssetFolderInfo folder)
        {
            AddAssetToFolder(null, -1, obj, folder);
        }

        private void AddAssetToFolder(TreeViewItem parent, int insertIndex, UnityObject obj, AssetFolderInfo folder)
        {
            AssetInfo parentAssetInfo = GetAssetInfo(parent);
            AssetInfo assetInfo = CreateAsset(obj.name, parentAssetInfo, insertIndex);

            assetInfo.Object = obj;

            AddAssetToFolder(assetInfo, folder);
            //assetInfo.PersistentId = ?
        }

        public void AddAssetToFolder(AssetInfo assetInfo, AssetFolderInfo folder)
        {
            if (folder.Assets == null)
            {
                folder.Assets = new List<AssetInfo>();
            }

            if(assetInfo.Folder != null)
            {
                TreeView.treeModel.RemoveElements(new[] { assetInfo.id });
                assetInfo.Folder.Assets.Remove(assetInfo);
            }

            assetInfo.Folder = folder;
            folder.Assets.Add(assetInfo);
        }

        private  AssetInfo CreateAsset(string name, TreeElement parent, int insertIndex)
        {
            int depth = parent != null ? parent.depth + 1 : 0;
            int id = TreeView.treeModel.GenerateUniqueID();
            var element = new AssetInfo(name, depth, id);
            TreeView.treeModel.AddElement(element, parent, insertIndex == -1 ?
                    parent.hasChildren ?
                        parent.children.Count
                        : 0
                    : insertIndex);

            // Select newly created element
            TreeView.SetSelection(new[] { id }, TreeViewSelectionOptions.RevealAndFrame);
            return element;
        }

        private AssetInfo GetAssetInfo(TreeViewItem treeViewItem)
        {
            return treeViewItem != null ? ((TreeViewItem<AssetInfo>)treeViewItem).data : TreeView.treeModel.root;
        }

        private DragAndDropVisualMode ExternalDropInside(TreeViewItem parent, int insertIndex, bool performDrop)
        {
            if (performDrop)
            {
                return PerformDrop(parent, insertIndex);
            }
            return CanDrop(parent, insertIndex);
        }

        private DragAndDropVisualMode ExternalDropOutside(TreeViewItem parent, int insertIndex, bool performDrop)
        {
            if (performDrop)
            {
                return PerformDrop(parent, insertIndex);
            }
            return CanDrop(parent, insertIndex);
        }

        private IList<AssetInfo> GetData()
        {
            if (m_folders != null)
            {
                List<AssetInfo> result = new List<AssetInfo>();

                AssetInfo root = new AssetInfo
                {
                    id = 0,
                    name = "Root",
                    IsEnabled = false,
                    depth = -1,
                };

                result.Add(root);
                foreach (AssetInfo assetInfo in m_folders.Where(folder => folder.Assets != null).SelectMany(folder => folder.Assets))
                {
                    assetInfo.parent = root;
                    result.Add(assetInfo);
                }
                return result;
            }

            return new List<AssetInfo>
            {
                new AssetInfo
                {
                    id = 0,
                    name = "Root",
                    IsEnabled = false,
                    depth = -1
                }
            };
        }

        private void SearchBar()
        {
            Rect rect = EditorGUILayout.GetControlRect();
            TreeView.searchString = m_SearchField.OnGUI(rect, TreeView.searchString);
        }

        private void DoTreeView()
        {
            Rect rect = GUILayoutUtility.GetRect(0, 10000, 0, Mathf.Max(10000, TreeView.totalHeight));
            TreeView.OnGUI(rect);
        }

        private void DoCommands()
        {
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.KeyDown:
                    {
                        if (Event.current.keyCode == KeyCode.Delete)
                        {
                            if (TreeView.HasFocus())
                            {
                                RemoveAsset();
                            }
                        }
                        break;
                    }
            }
            EditorGUILayout.BeginHorizontal();

            if(GUILayout.Button("Pick Asset"))
            {
                PickObject();
            }
 
            if (GUILayout.Button("Rename Asset"))
            {
                RenameAsset();
            }
            if (GUILayout.Button("Remove Asset"))
            {
                RemoveAsset();
            }
            EditorGUILayout.EndHorizontal();

            Debug.Log(Event.current.commandName);
            if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == m_currentPickerWindow)
            {
                m_pickedObject = EditorGUIUtility.GetObjectPickerObject();
            }
            else
            {
                if (Event.current.commandName == "ObjectSelectorClosed" && EditorGUIUtility.GetObjectPickerControlID() == m_currentPickerWindow)
                {
                    m_currentPickerWindow = -1;
                    if (m_pickedObject != null)
                    {
                        if (m_folders[0].Assets == null || !m_folders[0].Assets.Any(a => a.Object == m_pickedObject))
                        {
                            AddAssetToFolder(m_pickedObject, m_folders[0]);
                        }
                        m_pickedObject = null;
                    }
                }
            }
        }

        private UnityObject m_pickedObject;
        private int m_currentPickerWindow;
        private void PickObject()
        {
            m_currentPickerWindow = GUIUtility.GetControlID(FocusType.Passive) + 100;
            EditorGUIUtility.ShowObjectPicker<UnityObject>(null, false, string.Empty, m_currentPickerWindow);
        }

        private void RenameAsset()
        {
            var selection = TreeView.GetSelection();
            if (selection != null && selection.Count > 0)
            {
                TreeView.BeginRename(selection[0]);
            }
        }

        private void RemoveAsset()
        {
            Undo.RecordObject(m_asset, "Remove Asset");
            IList<int> selection = TreeView.GetSelection();
            foreach(int selectedId in selection)
            {
                AssetInfo assetInfo = TreeView.treeModel.Find(selectedId);
                if(assetInfo != null)
                {
                    assetInfo.Folder.Assets.Remove(assetInfo);
                    assetInfo.Folder = null;
                }
            }
           
            TreeView.treeModel.RemoveElements(selection);
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            InitIfNeeded();
            EditorGUILayout.Space();
            SearchBar();
            EditorGUILayout.Space();
            DoTreeView();
            EditorGUILayout.Space();
            DoCommands();
            EditorGUILayout.EndVertical();
        }
    }
}
