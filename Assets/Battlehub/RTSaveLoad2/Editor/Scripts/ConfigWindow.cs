using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Battlehub.RTSaveLoad2
{
    [InitializeOnLoad]
    public class ConfigWindow : EditorWindow
    {
        private static bool AllowAutoShow
        {
            get { return EditorPrefs.GetBool("RTSLConfigAllowAutoOpen", true); }
            set { EditorPrefs.SetBool("RTSLConfigAllowAutoOpen", value); }
        }

        [MenuItem("Tools/Runtime SaveLoad2/Config")]
        public static void ShowWindow()
        {
            ConfigWindow prevWindow = GetWindow<ConfigWindow>();
            if (prevWindow != null)
            {
                prevWindow.Close();
            }

            ConfigWindow window = CreateInstance<ConfigWindow>();
            window.titleContent = new GUIContent("RT Save & Load Config");
            window.Show();
            window.position = new Rect(20, 40, 380, 100);
        }

        static ConfigWindow()
        {
            EditorApplication.update += OnFirstUpdate;
    
        }

        private static void OnFirstUpdate()
        {
            EditorApplication.update -= OnFirstUpdate;
            if(!AllowAutoShow)
            {
                return;
            }

            bool typeModelExists = !string.IsNullOrEmpty(AssetDatabase.FindAssets(RTSL2Path.TypeModelDll.Replace(".dll", string.Empty)).FirstOrDefault());
            bool saveLoadDataFolderExists = AssetDatabase.IsValidFolder("Assets" + RTSL2Path.UserRoot);
            if (!typeModelExists || !saveLoadDataFolderExists)
            {
                ShowWindow();
            }
        }

        private bool m_doNotShowItAgain;
        private string m_path;
        private void OnEnable()
        {
            m_doNotShowItAgain = !AllowAutoShow;
            m_path = RTSL2Path.UserRoot;
        }

        private void OnGUI()
        {
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Runtime Save & Load configuration:");

            EditorGUILayout.Separator();

        
            m_path = EditorGUILayout.TextField("Data Path:", m_path);

            EditorGUILayout.Separator();

            EditorGUI.BeginChangeCheck();

            m_doNotShowItAgain = GUILayout.Toggle(m_doNotShowItAgain, "Do not show this window again");
            
            if (EditorGUI.EndChangeCheck())
            {
                AllowAutoShow = !m_doNotShowItAgain;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Build All"))
            {
                if (RTSL2Path.UserRoot != m_path && Directory.Exists(Application.dataPath + m_path))
                {
                    EditorUtility.DisplayDialog("Directory already exists", "Unable to change data path. Directory " + Application.dataPath + m_path + " already exists", "OK");
                }
                else
                {
                    if (Directory.Exists(Application.dataPath + RTSL2Path.UserRoot))
                    {
                        AssetDatabase.MoveAsset("Assets" + RTSL2Path.UserRoot, "Assets" + m_path);
                    }
                    RTSL2Path.UserRoot = m_path;
                    Menu.BuildAll();
                }
            }

            if(GUILayout.Button("Cancel"))
            {
                Close();
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}

    

