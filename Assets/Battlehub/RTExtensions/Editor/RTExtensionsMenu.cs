#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

using UnityObject = UnityEngine.Object;
using Battlehub.RTCommon;

namespace Battlehub.RTEditor
{
    public static class RTExtensionsMenu
    {

        [MenuItem("Tools/Runtime Editor/Create Extensions")]
        public static void CreateRuntimeEditor()
        {
            GameObject editorExtensions = InstantiateEditorExtensions();
            Undo.RegisterCreatedObjectUndo(editorExtensions, "Battlehub.RTExtensions.Create");

            if (RenderPipelineInfo.Type == RPType.HDRP)
            {
                GameObject hdrpSupport = InstantiateHDRPSupport();
                Undo.RegisterCreatedObjectUndo(hdrpSupport, "Battlehub.RTExtensions.CreateHDRP");
            }
        }

        public static GameObject InstantiateEditorExtensions()
        {
            UnityObject prefab = AssetDatabase.LoadAssetAtPath("Assets/" + BHRoot.Path + @"/RTExtensions/EditorExtensions.prefab", typeof(GameObject));
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        }

        public static GameObject InstantiateHDRPSupport()
        {
            UnityObject prefab = AssetDatabase.LoadAssetAtPath("Assets/" + BHRoot.Path + "/HDRP/EditorExtensionsHDRP.prefab", typeof(GameObject));
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        }
    }
}
#endif