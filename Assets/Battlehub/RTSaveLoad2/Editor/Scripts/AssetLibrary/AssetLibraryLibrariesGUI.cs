using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Battlehub.RTSaveLoad2
{
    public class AssetLibraryLibrariesGUI
    {
        [NonSerialized] bool m_Initialized;
        [SerializeField] TreeViewState m_TreeViewState; // Serialized in the window layout file so it survives assembly reloading
       
        MultiColumnTreeView m_TreeView;
        MyTreeAsset m_MyTreeAsset;

        public void OnGUI()
        {
            EditorGUILayout.LabelField("AssetLibraryLibrariesGUI");
        }
    }
}

