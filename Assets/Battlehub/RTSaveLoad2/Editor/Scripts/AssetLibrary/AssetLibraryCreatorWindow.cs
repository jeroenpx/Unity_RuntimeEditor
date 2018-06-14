using System.IO;
using UnityEditor;
using UnityEngine;

namespace Battlehub.RTSaveLoad2
{
    public class AssetLibraryCreatorWindow : EditorWindow
    {
        private AssetLibraryLibrariesGUI m_librariesGUI;
        private AssetLibraryProjectGUI m_projectGUI;
        private AssetLibraryAssetsGUI m_assetsGUI;

        private void Awake()
        {
            m_librariesGUI = new AssetLibraryLibrariesGUI();
            m_projectGUI = new AssetLibraryProjectGUI();
            m_assetsGUI = new AssetLibraryAssetsGUI();
        }

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


        private void OnGUI()
        {
            DropAreaGUI();
            EditorGUILayout.BeginHorizontal();
            m_librariesGUI.OnGUI();
            m_projectGUI.OnGUI();
            m_assetsGUI.OnGUI();
            EditorGUILayout.EndHorizontal();
        }

        public void DropAreaGUI()
        {
            Event evt = Event.current;
            Rect drop_area = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            GUI.Box(drop_area, "Add Trigger");

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!drop_area.Contains(evt.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (Object dragged_object in DragAndDrop.objectReferences)
                        {

                            string path = "Assets";

                           
                                path = AssetDatabase.GetAssetPath(dragged_object);
                                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                                {
                                    path = Path.GetDirectoryName(path);
                                    break;
                                }
                            
                        
                            Debug.Log(path);
                            // Do On Drag Stuff here
                        }
                    }
                    break;
            }
        }


    }
}
