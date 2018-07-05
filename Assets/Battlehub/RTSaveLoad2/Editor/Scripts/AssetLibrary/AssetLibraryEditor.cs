using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Battlehub.RTSaveLoad2
{
    [CustomEditor(typeof(AssetLibraryAsset))]
    public class AssetLibraryEditor : Editor
    {
        private AssetLibraryProjectGUI m_projectGUI;
        private AssetLibraryAssetsGUI m_assetsGUI;

        private AssetLibraryAsset Asset
        {
            get { return (AssetLibraryAsset)target; }
        }

        private bool m_canRenderAssetsGUI;

        private void OnEnable()
        {
            if (m_assetsGUI == null)
            {
                m_assetsGUI = new AssetLibraryAssetsGUI();
                m_assetsGUI.SetTreeAsset(Asset);
            }

            if (m_projectGUI == null)
            {
                m_projectGUI = new AssetLibraryProjectGUI(m_assetsGUI);
                m_projectGUI.SetTreeAsset(Asset);
                m_projectGUI.SelectedFoldersChanged += OnSelectedFoldersChanged;
            }

            m_assetsGUI.SetFolders(m_projectGUI.SelectedFolders);
            m_projectGUI.OnEnable();
            m_assetsGUI.OnEnable();
        }

        private void OnDisable()
        {
            EditorUtility.SetDirty(Asset);
            AssetDatabase.SaveAssets();

            m_projectGUI.SelectedFoldersChanged -= OnSelectedFoldersChanged;
            m_projectGUI.OnDisable();
            m_assetsGUI.OnDisable();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical();
            m_projectGUI.OnGUI();
            if(m_canRenderAssetsGUI)
            {
                m_assetsGUI.OnGUI();
            }
            EditorGUILayout.EndVertical();
        }

        private void OnSelectedFoldersChanged(object sender, EventArgs e)
        {
            m_canRenderAssetsGUI = m_projectGUI.SelectedFolders != null && m_projectGUI.SelectedFolders.Length > 0;
            m_assetsGUI.SetFolders(m_projectGUI.SelectedFolders);
        }


        //public void DropAreaGUI()
        //{
        //    Event evt = Event.current;
        //    Rect drop_area = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        //    GUI.Box(drop_area, "Add Trigger");

        //    switch (evt.type)
        //    {
        //        case EventType.DragUpdated:
        //        case EventType.DragPerform:
        //            if (!drop_area.Contains(evt.mousePosition))
        //                return;

        //            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

        //            if (evt.type == EventType.DragPerform)
        //            {
        //                DragAndDrop.AcceptDrag();

        //                foreach (Object dragged_object in DragAndDrop.objectReferences)
        //                {

        //                    string path = "Assets";


        //                        path = AssetDatabase.GetAssetPath(dragged_object);
        //                        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        //                        {
        //                            path = Path.GetDirectoryName(path);
        //                            //break;
        //                        }


        //                    Debug.Log(path);
        //                    // Do On Drag Stuff here
        //                }
        //            }
        //            break;
        //    }
        //}


    }
}
