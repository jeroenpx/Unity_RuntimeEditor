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

        private void OnEnable()
        {
            if (m_projectGUI == null)
            {
                m_projectGUI = new AssetLibraryProjectGUI();
                m_projectGUI.SetTreeAsset(Asset);
            }

            if (m_assetsGUI == null)
            {
                m_assetsGUI = new AssetLibraryAssetsGUI();
            }
            m_projectGUI.OnEnable();
        }

        private void OnDisable()
        {
            m_projectGUI.OnDisable();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginHorizontal();
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
                                    //break;
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
