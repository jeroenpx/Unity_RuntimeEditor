using Battlehub.ProBuilderIntegration;
using Battlehub.RTCommon;
using Battlehub.RTSL;
using UnityEngine;

namespace Battlehub.RTBuilder
{
    public class Wireframe : MonoBehaviour
    {
        private IRTE m_editor;
        private RuntimeWindow m_window;

        private void Awake()
        {
            m_window = GetComponent<RuntimeWindow>();
            
            m_editor = IOC.Resolve<IRTE>();
            m_editor.Object.Started += OnObjectStarted;

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

                foreach (ExposeToEditor obj in m_editor.Object.Get(false))
                {
                    PBMesh pbMesh = obj.GetComponent<PBMesh>();
                    if (pbMesh != null)
                    {
                        WireframeMesh wireframeMesh = pbMesh.GetComponentInChildren<WireframeMesh>(true);
                        if (wireframeMesh != null)
                        {
                            Destroy(wireframeMesh.gameObject);
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

            wireframe.AddComponent<RTSLIgnore>();
            wireframe.layer = m_editor.CameraLayerSettings.ExtraLayer2;
            wireframe.AddComponent<WireframeMesh>();
        }

        private void SetCullingMask(RuntimeWindow window)
        {
            window.Camera.cullingMask = (1 << LayerMask.NameToLayer("UI")) | (1 << m_editor.CameraLayerSettings.ExtraLayer1) | (1 << m_editor.CameraLayerSettings.ExtraLayer2);
            window.Camera.backgroundColor = Color.black;
            window.Camera.clearFlags = CameraClearFlags.SolidColor;
        }

        private void ResetCullingMask(RuntimeWindow window)
        {
            //window.Camera.cullingMask &= ~(1 << m_editor.CameraLayerSettings.ExtraLayer2);
        }

    }

}

