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
    public class AssetLibraryProjectGUI 
    {
        [NonSerialized]
        private bool m_Initialized;
        [SerializeField]
        private TreeViewState m_TreeViewState; // Serialized in the window layout file so it survives assembly reloading
        [SerializeField]
        private MultiColumnHeaderState m_MultiColumnHeaderState;
        private SearchField m_SearchField;
        private AssetLibraryAsset m_asset;
        private const string kSessionStateKeyPrefix = "AssetLibraryTVS";

        public event EventHandler SelectedFoldersChanged;

        public AssetFolderInfo[] SelectedFolders
        {
            get;
            private set;
        }
        internal AssetFolderTreeView TreeView
        {
            get;
            private set;
        }

        private AssetLibraryAssetsGUI m_assetsGUI;
        public AssetLibraryProjectGUI(AssetLibraryAssetsGUI assetsGUI)
        {
            m_assetsGUI = assetsGUI;
        }

        public void SetTreeAsset(AssetLibraryAsset asset)
        {
            m_asset = asset;
            m_Initialized = false;
        }

        private void InitIfNeeded()
        {
            if (!m_Initialized)
            {
                // Check if it already exists (deserialized from window layout file or scriptable object)
                if (m_TreeViewState == null)
                    m_TreeViewState = new TreeViewState();
                
                var jsonState = SessionState.GetString(kSessionStateKeyPrefix + m_asset.GetInstanceID(), "");
                if (!string.IsNullOrEmpty(jsonState))
                    JsonUtility.FromJsonOverwrite(jsonState, m_TreeViewState);
      
                bool firstInit = m_MultiColumnHeaderState == null;
                var headerState = AssetFolderTreeView.CreateDefaultMultiColumnHeaderState(0);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_MultiColumnHeaderState, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(m_MultiColumnHeaderState, headerState);
                m_MultiColumnHeaderState = headerState;

                var multiColumnHeader = new MultiColumnHeader(headerState);
                if (firstInit)
                    multiColumnHeader.ResizeToFit();

                var treeModel = new TreeModel<AssetFolderInfo>(GetData());

                TreeView = new AssetFolderTreeView(
                    m_TreeViewState, 
                    multiColumnHeader, 
                    treeModel,
                    OnExternalDropInside,
                    OnExternalDropOutside,
                    OnSelectionChanged);
                // m_TreeView.Reload();

                SelectedFolders = TreeView.Selection;
                if(SelectedFoldersChanged != null)
                {
                    OnSelectionChanged(SelectedFolders);
                }

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
            SessionState.SetString(kSessionStateKeyPrefix + m_asset.GetInstanceID(), JsonUtility.ToJson(TreeView.state));
        }

        private void OnUndoRedoPerformed()
        {
            if (TreeView != null)
            {
                TreeView.treeModel.SetData(GetData());
                TreeView.Reload();
            }
        }

        private DragAndDropVisualMode CanDrop(TreeViewItem parent, int insertIndex, bool outside)
        {
            AssetFolderInfo parentFolder;
            if(parent == null)
            {
                parentFolder = TreeView.treeModel.root;
            }
            else
            {
                parentFolder = ((TreeViewItem<AssetFolderInfo>)parent).data;
            }

            if (parentFolder.hasChildren)
            {
                var names = parentFolder.children.Select(c => c.name);

                var draggedRows = DragAndDrop.GetGenericData(AssetTreeView.k_GenericDragID) as List<TreeViewItem>;
                if (draggedRows != null)
                {
                    if(draggedRows.Any(item => names.Contains(((TreeViewItem<AssetInfo>)item).data.name)))
                    {
                        return DragAndDropVisualMode.None;
                    }
                }
                else
                {
                    if (DragAndDrop.objectReferences.Any(item => names.Contains(item.name)))
                    {
                        return DragAndDropVisualMode.None;
                    }
                }
            }

            if(parentFolder.Assets != null)
            {
                var names = parentFolder.Assets.Select(c => c.name);
                var draggedRows = DragAndDrop.GetGenericData(AssetTreeView.k_GenericDragID) as List<TreeViewItem>;
                if (draggedRows != null)
                {
                    if (draggedRows.Any(item => names.Contains(((TreeViewItem<AssetInfo>)item).data.name)))
                    {
                        return DragAndDropVisualMode.None;
                    }
                }
                else
                {
                    if (DragAndDrop.objectReferences.Any(item => names.Contains(item.name)))
                    {
                        return DragAndDropVisualMode.None;
                    }
                }
            }

            if (outside)
            {
                var draggedRows = DragAndDrop.GetGenericData(AssetTreeView.k_GenericDragID) as List<TreeViewItem>;
                if (draggedRows != null)
                {
                    return DragAndDropVisualMode.None;
                } 
                else
                {
                    var allPath = DragAndDrop.objectReferences.Select(o => AssetDatabase.GetAssetPath(o));
                    if (allPath.All(path => !string.IsNullOrEmpty(path) && File.Exists(path)))
                    {
                        return DragAndDropVisualMode.None;
                    }
                }
            }

            return DragAndDropVisualMode.Copy;
        }

        private DragAndDropVisualMode PerformDrop(TreeViewItem parent, int insertIndex, bool outside)
        {
            DragAndDrop.AcceptDrag();

            var draggedRows = DragAndDrop.GetGenericData(AssetTreeView.k_GenericDragID) as List<TreeViewItem>;
            if (draggedRows != null)
            {
                foreach (TreeViewItem<AssetInfo> dragged_object in draggedRows)
                {
                    if (!outside)
                    {
                        AssetFolderInfo folder = GetAssetFolderInfo(parent);
                        m_assetsGUI.AddAssetToFolder(dragged_object.data, folder);
                    }
                }
            }
            else
            {
                foreach (UnityObject dragged_object in DragAndDrop.objectReferences)
                {
                    string path = AssetDatabase.GetAssetPath(dragged_object);
                    if (!string.IsNullOrEmpty(path) && File.Exists(path))
                    {
                        if (!outside)
                        {
                            AssetFolderInfo folder = GetAssetFolderInfo(parent);
                            m_assetsGUI.AddAssetToFolder(dragged_object, folder);
                        }
                    }
                    else
                    {
                        CopyFolder(path, parent, insertIndex);
                    }
                    // Do On Drag Stuff here
                }
            }
           
            return DragAndDropVisualMode.Copy;
        }

        private void CopyFolder(string path, TreeViewItem parent, int insertIndex)
        {
            string lastFolderName = Path.GetFileName(path);
            AssetFolderInfo parentData = GetAssetFolderInfo(parent);
            AssetFolderInfo folder = CreateFolder(lastFolderName, parentData,
                insertIndex == -1 ?
                    parentData.hasChildren ?
                        parentData.children.Count
                        : 0
                    : insertIndex);

            TreeViewItem folderTreeViewItem = TreeView.FindItem(folder.id);
            string[] subfolders = AssetDatabase.GetSubFolders(path);
            for (int i = 0; i < subfolders.Length; ++i)
            {
                CopyFolder(subfolders[i], folderTreeViewItem, i);
            }
        }

        private AssetFolderInfo GetAssetFolderInfo(TreeViewItem treeViewItem)
        {
            return treeViewItem != null ? ((TreeViewItem<AssetFolderInfo>)treeViewItem).data : TreeView.treeModel.root;
        }

        private DragAndDropVisualMode OnExternalDropInside(TreeViewItem parent, int insertIndex, bool performDrop)
        {
            if(performDrop)
            {
                return PerformDrop(parent, insertIndex, false);
            }
            return CanDrop(parent, insertIndex, false);
        }

        private DragAndDropVisualMode OnExternalDropOutside(TreeViewItem parent, int insertIndex, bool performDrop)
        {
            if (performDrop)
            {
                return PerformDrop(parent, insertIndex, true);
            }
            return CanDrop(parent, insertIndex, true);
        }

        private void OnSelectionChanged(AssetFolderInfo[] selection)
        {
            SelectedFolders = selection;
            if(SelectedFoldersChanged != null)
            {
                SelectedFoldersChanged(this, EventArgs.Empty);
            }
        }

        private IList<AssetFolderInfo> GetData()
        {
            if (m_asset != null && m_asset.AssetLibrary != null && m_asset.AssetLibrary.Folders != null)
            {
                return m_asset.AssetLibrary.Folders;
            }

            return new List<AssetFolderInfo>
            {
                new AssetFolderInfo
                {
                    id = 0,
                    name = "Assets",
                    IsEnabled = true,
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
                            if(TreeView.HasFocus())
                            {
                                RemoveFolder();
                            }
                        }
                        break;
                    }
            }
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Create Folder"))
            {
                CreateFolder();
            }
            if (GUILayout.Button("Rename Folder"))
            {
                RenameFolder();
            }
            if (GUILayout.Button("Remove Folder"))
            {
                RemoveFolder();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void CreateFolder()
        {
            Undo.RecordObject(m_asset, "Create Asset Folder");

            TreeView.EndRename();
            var selection = TreeView.GetSelection();
            TreeElement parent = (selection.Count == 1 ? TreeView.treeModel.Find(selection[0]) : null) ?? TreeView.treeModel.root;
            AssetFolderInfo folder = CreateFolder("Folder", parent, 0);
            TreeView.BeginRename(folder.id);
        }

        private AssetFolderInfo CreateFolder(string name, TreeElement parent, int insertPosition)
        {
            int depth = parent != null ? parent.depth + 1 : 0;
            int id = TreeView.treeModel.GenerateUniqueID();
            var element = new AssetFolderInfo(name, depth, id);
            TreeView.treeModel.AddElement(element, parent, insertPosition);

            // Select newly created element
            TreeView.SetSelection(new[] { id }, TreeViewSelectionOptions.RevealAndFrame);
            return element;
        }

        private void RenameFolder()
        {
            var selection = TreeView.GetSelection();
            if (selection != null && selection.Count > 0)
            {
                TreeView.BeginRename(selection[0]);
            }
        }

        private void RemoveFolder()
        {
            Undo.RecordObject(m_asset, "Remove Asset Folder");
            var selection = TreeView.GetSelection();
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
