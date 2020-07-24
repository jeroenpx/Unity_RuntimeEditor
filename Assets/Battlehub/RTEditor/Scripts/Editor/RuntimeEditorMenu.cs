#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine.EventSystems;
using System.IO;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;
using Battlehub.RTSL;
using Battlehub.RTCommon;

namespace Battlehub.RTEditor
{
    public static class RTEditorMenu
    {
        public static string Root
        {
            get { return BHRoot.Path + @"/RTEditor/"; }
        }
            
        [MenuItem("Tools/Runtime Editor/Create Editor")]
        public static void CreateRuntimeEditor()
        {
            Undo.RegisterCreatedObjectUndo(InstantiateRuntimeEditor(), "Battlehub.RTEditor.Create");

            if(RenderPipelineInfo.Type == RPType.URP)
            {
                GameObject urpSupport = InstantiateURPSupport();
                if (urpSupport != null)
                {
                    Undo.RegisterCreatedObjectUndo(urpSupport, "Battlehub.RTEditor.URPSupport");
                }
            }
            else if(RenderPipelineInfo.Type == RPType.HDRP)
            {
                GameObject hdrpSupport = InstantiateHDRPSupport();
                if(hdrpSupport != null)
                {
                    Undo.RegisterCreatedObjectUndo(hdrpSupport, "Battlehub.RTEditor.HDRPSupport");
                }
            }
            
            EventSystem eventSystem = UnityObject.FindObjectOfType<EventSystem>();
            if (!eventSystem)
            {
                GameObject es = new GameObject();
                eventSystem = es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
                es.name = "EventSystem";
            }

            eventSystem.gameObject.AddComponent<RTSLIgnore>();

            GameObject camera = GameObject.Find("Main Camera");
            if(camera != null)
            {
                if(camera.GetComponent<GameViewCamera>() == null)
                {
                    if(EditorUtility.DisplayDialog("Main Camera setup.", "Do you want to add Game View Camera script to Main Camera and render it to Runtime Editors's Game view?", "Yes", "No"))
                    {
                        Undo.AddComponent<GameViewCamera>(camera.gameObject);
                    }
                }
            }
        }

        public static GameObject InstantiateRuntimeEditor()
        {
            UnityObject prefab = AssetDatabase.LoadAssetAtPath("Assets/" + BHRoot.Path + "/RTEditor/Prefabs/RuntimeEditor.prefab" , typeof(GameObject));
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        }

        public static GameObject InstantiateURPSupport()
        {
            UnityObject prefab = AssetDatabase.LoadAssetAtPath("Assets/" + BHRoot.Path + "/UniversalRP/RTEditorInitURP.prefab", typeof(GameObject));
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        }

        public static GameObject InstantiateHDRPSupport()
        {
            UnityObject prefab = AssetDatabase.LoadAssetAtPath("Assets/" + BHRoot.Path + "/HDRP/RTEditorInitHDRP.prefab", typeof(GameObject));
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        }

        [MenuItem("Tools/Runtime Editor/Custom Window/Create Prefab")]
        public static void CreateCustomWindowPrefab()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create View", "CustomWindow", "prefab", "Create Window");
            if(string.IsNullOrEmpty(path))
            {
                return;
            }
            AssetDatabase.CopyAsset("Assets/" + BHRoot.Path + "/RTEditor/Prefabs/Views/Resources/TemplateWindow.prefab", path);
        }

        [MenuItem("Tools/Runtime Editor/Custom Window/Create Script")]
        public static void CreateCustomWindowScript()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create View", "CustomWindow", "cs", "Create Window");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            string fileName = Path.GetFileNameWithoutExtension(path);

            CreateScriptFromTemplate action = ScriptableObject.CreateInstance<CreateScriptFromTemplate>();
            action.AssetTemplate = "Assets/" + BHRoot.Path + "/RTEditor/Prefabs/Views/Resources/TemplateWindow.cs.txt";

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0, action, Path.GetDirectoryName(path) + "/" + fileName + ".cs",
                EditorGUIUtility.FindTexture("cs Script Icon"), null);

        }

        [MenuItem("Tools/Runtime Editor/Custom Window/Create Registration Script")]
        public static void CreateCustomRegistrationScript()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create View", "RegisterCustomWindow", "cs", "Create Window Registration Script");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            string fileName = Path.GetFileNameWithoutExtension(path);

            CreateScriptFromTemplate action = ScriptableObject.CreateInstance<CreateScriptFromTemplate>();
            action.AssetTemplate = "Assets/" + BHRoot.Path + "/RTEditor/Prefabs/Views/Resources/RegisterTemplateWindow.cs.txt";
            action.Replacements.Add("#WINDOWNAME#", fileName);

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0, action, Path.GetDirectoryName(path) + "/" + fileName + ".cs",
                EditorGUIUtility.FindTexture("cs Script Icon"), null);
        }


        private class CreateScriptFromTemplate : EndNameEditAction
        {
            public string AssetTemplate;

            public readonly Dictionary<string, string> Replacements = new Dictionary<string, string>();

            public override void Action(int instanceId, string pathName,
                string resourceFile)
            {
                if (AssetDatabase.CopyAsset(AssetTemplate, pathName))
                {
                    string scriptName = Path.GetFileNameWithoutExtension(pathName);
                    string contents = File.ReadAllText(pathName);
                    contents = contents.Replace("#SCRIPTNAME#", scriptName);
                    foreach(var kvp in Replacements)
                    {
                        contents = contents.Replace(kvp.Key, kvp.Value);
                    }

                    File.WriteAllText(pathName, contents);
                }
            }
        }
    }
}
#endif