using Battlehub.RTCommon.EditorTreeView;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

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



        public AssetLibraryProjectGUI()
        {
        }
            

        //private Rect multiColumnTreeViewRect
        //{
        //    get { return new Rect(20, 30, position.width - 40, position.height - 60); }
        //}

        //private Rect toolbarRect
        //{
        //    get { return new Rect(20f, 10f, position.width - 40f, 20f); }
        //}

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

         
                bool firstInit = m_MultiColumnHeaderState == null;
                var headerState = AssetFolderTreeView.CreateDefaultMultiColumnHeaderState(0);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_MultiColumnHeaderState, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(m_MultiColumnHeaderState, headerState);
                m_MultiColumnHeaderState = headerState;

                var multiColumnHeader = new MultiColumnHeader(headerState);
                if (firstInit)
                    multiColumnHeader.ResizeToFit();

                var treeModel = new TreeModel<AssetFolderInfo>(GetData());

                m_TreeView = new AssetFolderTreeView(m_TreeViewState, multiColumnHeader, treeModel);

                m_SearchField = new SearchField();
                m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;

                m_Initialized = true;
            }
        }

        private IList<AssetFolderInfo> GetData()
        {
            if (m_asset != null && m_asset.AssetLibrary != null && m_asset.AssetLibrary.Folders != null)
            {
                return m_asset.AssetLibrary.Folders;
            }

            return AssetFolderInfoGenerator.GenerateRandomTree(100);
        }

        public void OnSelectionChange()
        {
            if (!m_Initialized)
                return;

            var asset = Selection.activeObject as AssetLibraryAsset;
            if (asset != null && asset != m_asset)
            {
                m_asset = asset;
                m_TreeView.treeModel.SetData(GetData());
                m_TreeView.Reload();
            }
        }

        private void SearchBar()
        {
            Rect rect = EditorGUILayout.GetControlRect();
            m_TreeView.searchString = m_SearchField.OnGUI(rect, TreeView.searchString);
        }

        private void DoTreeView()
        {
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true));
            m_TreeView.OnGUI(rect);
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("AssetLibraryProjectGUI");

            InitIfNeeded();

            SearchBar();
            DoTreeView();
            EditorGUILayout.EndVertical();
        }

        private static class AssetFolderInfoGenerator
        {
            static int IDCounter;
            static int minNumChildren = 5;
            static int maxNumChildren = 10;
            static float probabilityOfBeingLeaf = 0.5f;

            public static List<AssetFolderInfo> GenerateRandomTree(int numTotalElements)
            {
                int numRootChildren = numTotalElements / 4;
                IDCounter = 0;
                var treeElements = new List<AssetFolderInfo>(numTotalElements);

                var root = new AssetFolderInfo("Root", -1, IDCounter);
                treeElements.Add(root);
                for (int i = 0; i < numRootChildren; ++i)
                {
                    int allowedDepth = 6;
                    AddChildrenRecursive(root, UnityEngine.Random.Range(minNumChildren, maxNumChildren), true, numTotalElements, ref allowedDepth, treeElements);
                }

                return treeElements;
            }
            static void AddChildrenRecursive(TreeElement element, int numChildren, bool force, int numTotalElements, ref int allowedDepth, List<AssetFolderInfo> treeElements)
            {
                if (element.depth >= allowedDepth)
                {
                    allowedDepth = 0;
                    return;
                }

                for (int i = 0; i < numChildren; ++i)
                {
                    if (IDCounter > numTotalElements)
                        return;

                    var child = new AssetFolderInfo("Element " + IDCounter, element.depth + 1, ++IDCounter);
                    treeElements.Add(child);

                    if (!force && UnityEngine.Random.value < probabilityOfBeingLeaf)
                        continue;

                    AddChildrenRecursive(child, UnityEngine.Random.Range(minNumChildren, maxNumChildren), false, numTotalElements, ref allowedDepth, treeElements);
                }
            }
        }
    }
}
