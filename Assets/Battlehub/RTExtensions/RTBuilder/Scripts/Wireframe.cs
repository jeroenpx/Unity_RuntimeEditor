using Battlehub.ProBuilderIntegration;
using Battlehub.RTCommon;
using Battlehub.RTHandles;
using Battlehub.RTSL;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTBuilder
{
    public class Wireframe : MonoBehaviour
    {
        private IRTE m_editor;
        private RuntimeWindow m_window;
        private IRuntimeSceneComponent m_sceneComponent;

        private void Awake()
        {
            m_window = GetComponent<RuntimeWindow>();
            
            m_editor = IOC.Resolve<IRTE>();
            m_editor.Object.Started += OnObjectStarted;
            m_editor.Selection.SelectionChanged += OnSelectionChanged;

            m_sceneComponent = m_window.IOCContainer.Resolve<IRuntimeSceneComponent>();

            foreach (ExposeToEditor obj in m_editor.Object.Get(false))
            {
                PBMesh pbMesh = obj.GetComponent<PBMesh>();
                if (pbMesh != null)
                {
                    CreateWireframeMesh(pbMesh);
                }
            }
        }

        private void Start()
        {
            SetCullingMask(m_window);
        }

        private void OnDestroy()
        {
            if(m_editor != null && m_editor.Object != null)
            {
                m_editor.Object.Started -= OnObjectStarted;
                m_editor.Selection.SelectionChanged -= OnSelectionChanged;

                foreach (ExposeToEditor obj in m_editor.Object.Get(false))
                {
                    PBMesh pbMesh = obj.GetComponent<PBMesh>();
                    if (pbMesh != null)
                    {
                        WireframeMesh[] wireframeMesh = pbMesh.GetComponentsInChildren<WireframeMesh>(true);
                        for(int i = 0; i < wireframeMesh.Length; ++i)
                        {
                            WireframeMesh wireframe = wireframeMesh[i];
                            if (!wireframe.IsIndividual)
                            {
                                Destroy(wireframe.gameObject);
                                break;
                            }
                        }
                    }
                }
            }

            if(m_window != null)
            {
                ResetCullingMask(m_window);
            }
        }

        private void OnObjectStarted(ExposeToEditor obj)
        {
            PBMesh pbMesh = obj.GetComponent<PBMesh>();
            if(pbMesh != null)
            {
                CreateWireframeMesh(pbMesh);
            }
        }

        private void CreateWireframeMesh(PBMesh pbMesh)
        {
            GameObject wireframe = new GameObject("Wireframe");
            wireframe.transform.SetParent(pbMesh.transform, false);

            wireframe.gameObject.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
            wireframe.layer = m_editor.CameraLayerSettings.ExtraLayer;
            WireframeMesh wireframeMesh = wireframe.AddComponent<WireframeMesh>();

            if (m_editor.Selection.IsSelected(pbMesh.gameObject))
            {
                wireframeMesh.IsSelected = true;
            }
        }

        private void SetCullingMask(RuntimeWindow window)
        {
            window.Camera.cullingMask = (1 << LayerMask.NameToLayer("UI")) | (1 << m_editor.CameraLayerSettings.AllScenesLayer) | (1 << m_editor.CameraLayerSettings.ExtraLayer);
            window.Camera.backgroundColor = Color.white;
            window.Camera.clearFlags = CameraClearFlags.SolidColor;

            if(m_sceneComponent != null && m_sceneComponent.SceneGizmo != null)
            {
                m_sceneComponent.SceneGizmo.TextColor = Color.black;
            }
        }

        private void ResetCullingMask(RuntimeWindow window)
        {
            CameraLayerSettings settings = m_editor.CameraLayerSettings;
            window.Camera.cullingMask = ~((1 << m_editor.CameraLayerSettings.ExtraLayer) | ((1 << settings.MaxGraphicsLayers) - 1) << settings.RuntimeGraphicsLayer);
            window.Camera.clearFlags = CameraClearFlags.Skybox;

            if (m_sceneComponent != null && m_sceneComponent.SceneGizmo != null)
            {
                m_sceneComponent.SceneGizmo.TextColor = Color.white;
            }
        }

        private void OnSelectionChanged(Object[] unselectedObjects)
        {
            if (unselectedObjects != null)
            {
                WireframeMesh[] wireframes = unselectedObjects.Select(go => go as GameObject).Where(go => go != null).SelectMany(go => go.GetComponentsInChildren<WireframeMesh>(true)).ToArray();
                for(int i = 0; i < wireframes.Length; ++i)
                {
                    wireframes[i].IsSelected = false;
                }
            }

            TryToSelect();
        }

        private void TryToSelect()
        {
            if (m_editor.Selection.gameObjects != null)
            {
                WireframeMesh[] wireframes = m_editor.Selection.gameObjects.Where(go => go != null).SelectMany(go => go.GetComponentsInChildren<WireframeMesh>(true)).ToArray();
                for (int i = 0; i < wireframes.Length; ++i)
                {
                    wireframes[i].IsSelected = true;
                }
            }
        }
    }

}

