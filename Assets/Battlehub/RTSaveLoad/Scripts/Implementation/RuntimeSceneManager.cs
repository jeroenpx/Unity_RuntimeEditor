using Battlehub.RTCommon;
using System;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTSaveLoad
{
    public class RuntimeSceneManager : MonoBehaviour, ISceneManager
    {
        public event EventHandler<ProjectManagerEventArgs> SceneCreated;
        public event EventHandler<ProjectManagerEventArgs> SceneSaving;
        public event EventHandler<ProjectManagerEventArgs> SceneSaved;
        public event EventHandler<ProjectManagerEventArgs> SceneLoading;
        public event EventHandler<ProjectManagerEventArgs> SceneLoaded;

        protected IProject m_project;
        protected ISerializer m_serializer;
        protected IRTE m_editor;

        [NonSerialized]
        private ProjectItem m_activeScene;
        public ProjectItem ActiveScene
        {
            get { return m_activeScene; }
        }        

        private void Awake()
        {
            //Prewarm protobuf
            Dependencies.Serializer.DeepClone(1);

            m_project = Dependencies.Project;
            m_serializer = Dependencies.Serializer;
            m_editor = IOC.Resolve<IRTE>();
            if(m_editor == null)
            {
                Debug.LogError("RTE is null");
                return;
            }

            AwakeOverride();
        }

        private void Start()
        {
            StartOverride();
        }

        private void OnDestroy()
        {
            OnDestroyOverride();
            IdentifiersMap.Instance = null;
        }

        protected virtual void AwakeOverride()
        {
            m_activeScene = ProjectItem.CreateScene("New Scene");
        }

        protected virtual void StartOverride()
        {

        }

        protected virtual void OnDestroyOverride()
        {

        }

        public void Exists(ProjectItem item, ProjectManagerCallback<bool> callback)
        {
            m_project.Exists(item, result =>
            {
                if(callback != null)
                {
                    callback(result.Data);
                }
            });
        }

        public virtual void SaveScene(ProjectItem scene, ProjectManagerCallback callback)
        {
            if (SceneSaving != null)
            {
                SceneSaving(this, new ProjectManagerEventArgs(scene));
            }

            GameObject extraData = new GameObject();
            ExtraSceneData saveLoad = extraData.AddComponent<ExtraSceneData>();
            saveLoad.Selection = m_editor.Selection.objects;

            PersistentScene persistentScene = PersistentScene.CreatePersistentScene();
            if (scene.Internal_Data == null)
            {
                scene.Internal_Data = new ProjectItemData();
            }
            scene.Internal_Data.RawData = m_serializer.Serialize(persistentScene);
            Destroy(extraData);

            m_project.Save(scene, false, saveCompleted =>
            {
                m_project.UnloadData(scene);

                m_activeScene = scene;

                if (callback != null)
                {
                    callback();
                }

                if (SceneSaved != null)
                {
                    SceneSaved(this, new ProjectManagerEventArgs(scene));
                }
            });
        }

        public virtual void LoadScene(ProjectItem scene, ProjectManagerCallback callback)
        {
            RaiseSceneLoading(scene);

            bool isEnabled = m_editor.Undo.Enabled;
            m_editor.Undo.Enabled = false;
            m_editor.Selection.objects = null;
            m_editor.Undo.Enabled = isEnabled;

            m_project.LoadData(new[] { scene }, loadDataCompleted =>
            {
                scene = loadDataCompleted.Data[0];

                PersistentScene persistentScene = m_serializer.Deserialize<PersistentScene>(scene.Internal_Data.RawData);
                CompleteSceneLoading(scene, callback, isEnabled, persistentScene);
            });

        }

        protected void RaiseSceneLoading(ProjectItem scene)
        {
            if (SceneLoading != null)
            {
                SceneLoading(this, new ProjectManagerEventArgs(scene));
            }
        }

        protected void RaiseSceneLoaded(ProjectItem scene)
        {
            if (SceneLoaded != null)
            {
                SceneLoaded(this, new ProjectManagerEventArgs(scene));
            }
        }

        protected void CompleteSceneLoading(ProjectItem scene, ProjectManagerCallback callback, bool isEnabled, PersistentScene persistentScene)
        {
            PersistentScene.InstantiateGameObjects(persistentScene);

            m_project.UnloadData(scene);

            ExtraSceneData extraData = FindObjectOfType<ExtraSceneData>();
            m_editor.Undo.Enabled = false;
            m_editor.Selection.objects = extraData.Selection;
            m_editor.Undo.Enabled = isEnabled;
            Destroy(extraData.gameObject);

            m_activeScene = scene;

            if (callback != null)
            {
                callback();
            }

            RaiseSceneLoaded(scene);
        }

        public virtual void CreateScene()
        {
            m_editor.Selection.objects = null;
            m_editor.Undo.Purge();
            ExposeToEditor[] editorObjects = ExposeToEditor.FindAll(m_editor, ExposeToEditorObjectType.EditorMode, false).Select(go => go.GetComponent<ExposeToEditor>()).ToArray();
            for (int i = 0; i < editorObjects.Length; ++i)
            {
                ExposeToEditor exposeToEditor = editorObjects[i];
                if (exposeToEditor != null)
                {
                    DestroyImmediate(exposeToEditor.gameObject);
                }
            }

            GameObject dirLight = new GameObject();
            dirLight.transform.rotation = Quaternion.Euler(50, -30, 0);
            dirLight.transform.position = new Vector3(0, 10, 0);
            Light lightComponent = dirLight.AddComponent<Light>();
            lightComponent.type = LightType.Directional;

            dirLight.name = "Directional Light";
            dirLight.AddComponent<ExposeToEditor>();

            GameObject camera = new GameObject();
            camera.name = "Main Camera";
            camera.transform.position = new Vector3(0, 0, -10);
            camera.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.AddComponent<ExposeToEditor>();

            m_activeScene = ProjectItem.CreateScene("New Scene");

            if (SceneCreated != null)
            {
                SceneCreated(this, new ProjectManagerEventArgs(ActiveScene));
            }
        }
    }
}
