using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;

using UnityObject = UnityEngine.Object;

namespace Battlehub.RTExtensions
{
    public static class RTExtensionsMenu
    {
        const string root = BHPath.Root + @"/RTExtensions/";

        [MenuItem("Tools/Runtime Editor/Create Extensions")]
        public static void CreateRuntimeEditor()
        {
            GameObject editorExtensions = InstantiateEditorExtensions();
            Undo.RegisterCreatedObjectUndo(editorExtensions, "Battlehub.RTExtensions.Create");
        }

        public static GameObject InstantiateEditorExtensions()
        {
            return InstantiatePrefab("EditorExtensions.prefab");
        }

        public static GameObject InstantiatePrefab(string name)
        {
            UnityObject prefab = AssetDatabase.LoadAssetAtPath("Assets/" + root + "/" + name, typeof(GameObject));
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        } 
    }
}
