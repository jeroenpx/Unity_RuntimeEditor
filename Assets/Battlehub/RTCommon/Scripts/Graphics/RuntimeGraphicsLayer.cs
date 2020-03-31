using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTCommon
{
    public interface IRuntimeGraphicsLayer
    {
        void BeginRefresh();
        void Refresh();
        void EndRefresh();
        void AddRenderers(Renderer[] renderers);
        void RemoveRenderers(Renderer[] renderers);
        void AddMesh(Mesh mesh, Matrix4x4 matrix, Material material);
        void RemoveMesh(Mesh mesh);
    }

    [DefaultExecutionOrder(-55)]
    [RequireComponent(typeof(RuntimeWindow))]
    public class RuntimeGraphicsLayer : MonoBehaviour, IRuntimeGraphicsLayer
    {
        [SerializeField]
        private Camera m_graphicsLayerCamera;

        [SerializeField]
        private bool m_useCommandBuffer = true;

        private RenderTextureCamera m_renderTextureCamera;
        
        private RuntimeWindow m_window;

        public RuntimeWindow Window
        {
            get { return m_window; }
        }
        
        private void Awake()
        {
            m_window = GetComponent<RuntimeWindow>();
            m_window.IOCContainer.RegisterFallback<IRuntimeGraphicsLayer>(this);
            m_window.CameraResized += OnCameraResized;
            
            PrepareGraphicsLayerCamera();
        }

        private void OnDestroy()
        {
            if (m_window != null)
            {
                m_window.IOCContainer.UnregisterFallback<IRuntimeGraphicsLayer>(this);
                m_window.CameraResized -= OnCameraResized;
            }

            if (m_graphicsLayerCamera != null)
            {
                Destroy(m_graphicsLayerCamera.gameObject);
            }

            if (m_renderTextureCamera != null && m_renderTextureCamera.OverlayMaterial != null)
            {
                Destroy(m_renderTextureCamera.OverlayMaterial);
            }
        }

        private void Start()
        {
            if (m_window.Index >= m_window.Editor.CameraLayerSettings.MaxGraphicsLayers)
            {
                Debug.LogError("m_editorWindow.Index >= m_editorWindow.Editor.CameraLayerSettings.MaxGraphicsLayers");
            }
        }

        private void OnEnable()
        {
            UpdateGraphicsLayerCamera();
        }

        private void LateUpdate()
        {
            UpdateGraphicsLayerCamera();
        }

        private void OnCameraResized()
        {
            UpdateGraphicsLayerCamera();            
        }

        private void PrepareGraphicsLayerCamera()
        {
            bool wasActive = m_window.Camera.gameObject.activeSelf;
            m_window.Camera.gameObject.SetActive(false);

            //m_trackedPoseDriver = m_editorWindow.Camera.GetComponent<TrackedPoseDriver>();
            if (m_window.Editor.IsVR && m_window.Camera.stereoEnabled && m_window.Camera.stereoTargetEye == StereoTargetEyeMask.Both )
            {
                m_graphicsLayerCamera = Instantiate(m_window.Camera, m_window.Camera.transform.parent);
                m_graphicsLayerCamera.transform.SetSiblingIndex(m_window.Camera.transform.GetSiblingIndex() + 1);
            }
            else
            {
                m_graphicsLayerCamera = Instantiate(m_window.Camera, m_window.Camera.transform);
            }

            for (int i = m_graphicsLayerCamera.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(m_graphicsLayerCamera.transform.GetChild(i).gameObject);
            }

            Component[] components = m_graphicsLayerCamera.GetComponents<Component>();
            for (int i = 0; i < components.Length; ++i)
            {
                Component component = components[i];
                if (component is Transform)
                {
                    continue;
                }
                if (component is Camera)
                {
                    continue;
                }
                if(component is RenderTextureCamera)
                {
                    continue;
                }

                Destroy(component);
            }

            m_graphicsLayerCamera.transform.localPosition = Vector3.zero;
            m_graphicsLayerCamera.transform.localRotation = Quaternion.identity;
            m_graphicsLayerCamera.transform.localScale = Vector3.one;
            m_graphicsLayerCamera.name = "GraphicsLayerCamera";
            m_graphicsLayerCamera.depth = m_window.Camera.depth + 1;

            if (m_useCommandBuffer)
            {
                InitializeCommandBuffer(m_graphicsLayerCamera);
                m_graphicsLayerCamera.cullingMask = 0;
            }
            else
            {
                m_graphicsLayerCamera.cullingMask = 1 << (m_window.Editor.CameraLayerSettings.RuntimeGraphicsLayer + m_window.Index);
            }


            m_renderTextureCamera = m_graphicsLayerCamera.GetComponent<RenderTextureCamera>();
            if (m_renderTextureCamera == null)
            {
                #if UNITY_2019_1_OR_NEWER
                if(RenderPipelineInfo.Type != RPType.Standard)
                {
                    UnityEngine.Rendering.RenderPipelineManager.endFrameRendering += OnEndFrameRendering;
                }
                #endif
                m_graphicsLayerCamera.clearFlags = CameraClearFlags.Depth;
            }
            else
            {
                m_renderTextureCamera.OverlayMaterial = new Material(Shader.Find("Battlehub/RTCommon/RenderTextureOverlay"));
                m_graphicsLayerCamera.clearFlags = CameraClearFlags.SolidColor;
                m_graphicsLayerCamera.backgroundColor = new Color(0, 0, 0, 0);
            }

            m_graphicsLayerCamera.allowHDR = false; //fix strange screen blinking bug...
            m_graphicsLayerCamera.projectionMatrix = m_window.Camera.projectionMatrix; //for ARCore
            
            
            m_window.Camera.gameObject.SetActive(wasActive);
            m_graphicsLayerCamera.gameObject.SetActive(true);
        }

        #if UNITY_2019_1_OR_NEWER
        private void OnEndFrameRendering(UnityEngine.Rendering.ScriptableRenderContext arg1, Camera[] arg2)
        {
            UnityEngine.Rendering.RenderPipelineManager.endFrameRendering -= OnEndFrameRendering;

            //LWRP OR HDRP;

            bool wasActive = m_graphicsLayerCamera.gameObject.activeSelf;
            m_graphicsLayerCamera.gameObject.SetActive(false);

            m_renderTextureCamera = m_graphicsLayerCamera.gameObject.AddComponent<RenderTextureCamera>();

            IRTE rte = IOC.Resolve<IRTE>();
            RuntimeWindow sceneWindow = rte.GetWindow(RuntimeWindowType.Scene);
            m_renderTextureCamera.OutputRoot = (RectTransform)sceneWindow.transform;
            m_renderTextureCamera.OverlayMaterial = new Material(Shader.Find("Battlehub/RTCommon/RenderTextureOverlay"));
            m_graphicsLayerCamera.clearFlags = CameraClearFlags.SolidColor;
            m_graphicsLayerCamera.backgroundColor = new Color(0, 0, 0, 0);

            m_graphicsLayerCamera.gameObject.SetActive(wasActive);
        }
        #endif

        private void UpdateGraphicsLayerCamera()
        {
            if (m_renderTextureCamera != null)
            {
                m_renderTextureCamera.TryResizeRenderTexture();
            }

            if (m_graphicsLayerCamera.depth != m_window.Camera.depth + 1)
            {
                m_graphicsLayerCamera.depth = m_window.Camera.depth + 1;
            }

            if (m_graphicsLayerCamera.fieldOfView != m_window.Camera.fieldOfView)
            {
                m_graphicsLayerCamera.fieldOfView = m_window.Camera.fieldOfView;
            }

            if (m_graphicsLayerCamera.orthographic != m_window.Camera.orthographic)
            {
                m_graphicsLayerCamera.orthographic = m_window.Camera.orthographic;
            }

            if (m_graphicsLayerCamera.orthographicSize != m_window.Camera.orthographicSize)
            {
                m_graphicsLayerCamera.orthographicSize = m_window.Camera.orthographicSize;
            }

            if (m_graphicsLayerCamera.rect != m_window.Camera.rect)
            {
                m_graphicsLayerCamera.rect = m_window.Camera.rect;
            }

            if (m_graphicsLayerCamera.enabled != m_window.Camera.enabled)
            {
                m_graphicsLayerCamera.enabled = m_window.Camera.enabled;
            }

            if (m_window.Camera.pixelWidth > 0 && m_window.Camera.pixelHeight > 0)
            {
                m_graphicsLayerCamera.projectionMatrix = m_window.Camera.projectionMatrix; //ARCore
            }
        }

        #region IRuntimeGraphicsLayer
        private CommandBuffer m_cmdBuffer;
        private List<Renderer> m_renderers = new List<Renderer>();
        private Dictionary<Mesh, Tuple<Matrix4x4, Material>> m_meshes = new Dictionary<Mesh, Tuple<Matrix4x4, Material>>();
        private bool m_updateInProgress = false;

        private void InitializeCommandBuffer(Camera camera)
        {
            m_cmdBuffer = new CommandBuffer();
            m_cmdBuffer.name = "RuntimeGraphicsLayer";
            camera.AddCommandBuffer(CameraEvent.AfterForwardAlpha, m_cmdBuffer);
        }

        public void BeginRefresh()
        {
            m_updateInProgress = true;
        }

        public void EndRefresh()
        {
            if(m_updateInProgress)
            {
                Refresh();
            }

            m_updateInProgress = false;
        }

        public void AddRenderers(Renderer[] renderers)
        {
            if (m_cmdBuffer == null)
            {
                return;
            }

            foreach(Renderer renderer in renderers)
            {
                if(renderer is SkinnedMeshRenderer)
                {
                    SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)renderer;
                    skinnedMeshRenderer.forceMatrixRecalculationPerRender = true;
                }

                if(!renderer.forceRenderingOff)
                {
                    renderer.enabled = false;
                    m_renderers.Add(renderer);
                }
            }

            if (!m_updateInProgress)
            {
                Refresh();
            }
        }

        public void RemoveRenderers(Renderer[] renderers)
        {
            if (m_cmdBuffer == null)
            {
                return;
            }

            foreach (Renderer renderer in renderers)
            {
                if (renderer is SkinnedMeshRenderer)
                {
                    SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)renderer;
                    skinnedMeshRenderer.forceMatrixRecalculationPerRender = false;
                }

                renderer.enabled = true;
                m_renderers.Remove(renderer);
            }
            if (!m_updateInProgress)
            {
                Refresh();
            }
        }

        public void AddMesh(Mesh mesh, Matrix4x4 matrix, Material material)
        {
            m_meshes[mesh] = new Tuple<Matrix4x4, Material>(matrix, material);
            if (!m_updateInProgress)
            {
                Refresh();
            }
        }

        public void RemoveMesh(Mesh mesh)
        {
            m_meshes.Remove(mesh);
            if (!m_updateInProgress)
            {
                Refresh();
            }
        }

        public void Refresh()
        {
            m_cmdBuffer.Clear();
            for(int i = 0; i < m_renderers.Count; ++i)
            {
                Renderer renderer = m_renderers[i];
                Material[] materials = renderer.sharedMaterials;
                for(int j = 0; j < materials.Length; ++j)
                {
                    Material material = materials[j];
                    m_cmdBuffer.DrawRenderer(renderer, material, j, -1);
                }
            }

            foreach(KeyValuePair<Mesh, Tuple<Matrix4x4, Material>> kvp in m_meshes)
            {
                m_cmdBuffer.DrawMesh(kvp.Key, kvp.Value.Item1, kvp.Value.Item2);
            }
        }

        #endregion
    }
}



