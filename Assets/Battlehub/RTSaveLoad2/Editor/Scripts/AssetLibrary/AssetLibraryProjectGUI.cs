using Battlehub.RTCommon.EditorTreeView;
using System;
using System.Collections.Generic;
using System.IO;
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
        private AssetFolderTreeView m_TreeView;
        private AssetLibraryAsset m_asset;
        private const string kSessionStateKeyPrefix = "AssetLibraryTVS";

        public AssetLibraryProjectGUI()
        {
        }
            
        internal AssetFolderTreeView TreeView
        {
            get { return m_TreeView; }
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

                m_TreeView = new AssetFolderTreeView(
                    m_TreeViewState, 
                    multiColumnHeader, 
                    treeModel,
                    ExternalDropInside,
                    ExternalDropOutside);
               // m_TreeView.Reload();

                m_SearchField = new SearchField();
                m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;

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

            SessionState.SetString(kSessionStateKeyPrefix + m_asset.GetInstanceID(), JsonUtility.ToJson(m_TreeView.state));
        }

        private void OnUndoRedoPerformed()
        {
            if (m_TreeView != null)
            {
                m_TreeView.treeModel.SetData(GetData());
                m_TreeView.Reload();
            }
        }

        private DragAndDropVisualMode CanDrop(TreeViewItem parent, int insertIndex)
        {
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
                    path = Path.GetDirectoryName(path);
                }
                else
                {
                    CopyFolder(path, parent, insertIndex);
                }

                Debug.Log(path);
                
                // Do On Drag Stuff here
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

            TreeViewItem folderTreeViewItem = m_TreeView.FindItem(folder.id);
            string[] subfolders = AssetDatabase.GetSubFolders(path);
            for (int i = 0; i < subfolders.Length; ++i)
            {
                CopyFolder(subfolders[i], folderTreeViewItem, i);
            }
        }

        private AssetFolderInfo GetAssetFolderInfo(TreeViewItem treeViewItem)
        {
            return treeViewItem != null ? ((TreeViewItem<AssetFolderInfo>)treeViewItem).data : m_TreeView.treeModel.root;
        }

        private DragAndDropVisualMode ExternalDropInside(TreeViewItem parent, int insertIndex, bool performDrop)
        {
            if(performDrop)
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
            m_TreeView.searchString = m_SearchField.OnGUI(rect, TreeView.searchString);
        }

        private void DoTreeView()
        {
            Rect rect = GUILayoutUtility.GetRect(0, 10000, 0, Mathf.Max(10000, TreeView.totalHeight));
            m_TreeView.OnGUI(rect);
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
                            if(m_TreeView.HasFocus())
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

            m_TreeView.EndRename();
            var selection = m_TreeView.GetSelection();
            TreeElement parent = (selection.Count == 1 ? m_TreeView.treeModel.Find(selection[0]) : null) ?? m_TreeView.treeModel.root;
            AssetFolderInfo folder = CreateFolder("Folder", parent, 0);
            m_TreeView.BeginRename(folder.id);
        }

        private AssetFolderInfo CreateFolder(string name, TreeElement parent, int insertPosition)
        {
            int depth = parent != null ? parent.depth + 1 : 0;
            int id = m_TreeView.treeModel.GenerateUniqueID();
            var element = new AssetFolderInfo(name, depth, id);
            m_TreeView.treeModel.AddElement(element, parent, insertPosition);

            // Select newly created element
            m_TreeView.SetSelection(new[] { id }, TreeViewSelectionOptions.RevealAndFrame);
            return element;
        }

        private void RenameFolder()
        {
            var selection = m_TreeView.GetSelection();
            if (selection != null && selection.Count > 0)
            {
                m_TreeView.BeginRename(selection[0]);
            }
        }

        private void RemoveFolder()
        {
            Undo.RecordObject(m_asset, "Remove Asset Folder");
            var selection = m_TreeView.GetSelection();
            m_TreeView.treeModel.RemoveElements(selection);
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            InitIfNeeded();

            SearchBar();
            EditorGUILayout.Space();
            DoCommands();
            EditorGUILayout.Space();
            DoTreeView();
            EditorGUILayout.EndVertical();
        }
    }
}
