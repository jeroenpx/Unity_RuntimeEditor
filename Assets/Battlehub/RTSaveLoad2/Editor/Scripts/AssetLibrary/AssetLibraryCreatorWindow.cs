using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Battlehub.RTSaveLoad2
{
    public class AssetLibraryCreatorWindow : EditorWindow
    {
        [MenuItem("Tools/Runtime SaveLoad2/Asset Libraries")]
        public static void ShowMenuItem()
        {
            ShowWindow();
        }

        public static void ShowWindow()
        {
            AssetLibraryCreatorWindow prevWindow = GetWindow<AssetLibraryCreatorWindow>();
            if (prevWindow != null)
            {
                prevWindow.Close();
            }

            AssetLibraryCreatorWindow window = CreateInstance<AssetLibraryCreatorWindow>();
            window.titleContent = new GUIContent("Asset Libraries");
            window.Show();
            window.position = new Rect(20, 40, 1280, 768);
        }
    }
}
