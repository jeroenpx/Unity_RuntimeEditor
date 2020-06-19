using Battlehub.RTCommon;
using Battlehub.RTCommon.HDRP;
using Battlehub.RTHandles.HDRP;
using Battlehub.UIControls;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;

namespace Battlehub.RTEditor.HDRP
{
    public class HDRPInit : EditorExtension
    {
        private GameObject m_foregroundOutput;
        private IRenderPipelineCameraUtility m_cameraUtility;
    
        protected override void OnEditorExist()
        {
            base.OnEditorExist();
            if (RenderPipelineInfo.Type != RPType.HDRP || RenderPipelineInfo.ForceUseRenderTextures)
            {
                return;
            }

            m_cameraUtility = GetComponent<IRenderPipelineCameraUtility>();

            IRTE rte = IOC.Resolve<IRTE>();
            Canvas foregroundCanvas = IOC.Resolve<IRTEAppearance>().UIForegroundScaler.GetComponent<Canvas>();
            Camera foregroundCamera = foregroundCanvas.worldCamera;
            if (foregroundCamera != null)
            {
                if(m_cameraUtility != null)
                {
                    m_cameraUtility.EnablePostProcessing(foregroundCamera, false);
                    m_cameraUtility.SetBackgroundColor(foregroundCamera, new Color(0, 0, 0, 0));
                }

                GameObject foregroundLayer = new GameObject("ForegroundLayer");
                foregroundLayer.transform.SetParent(rte.Root, false);
                foregroundCanvas = foregroundLayer.AddComponent<Canvas>();
                foregroundCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

                foregroundCamera.gameObject.SetActive(false);
                m_foregroundOutput = new GameObject("Output");
                m_foregroundOutput.transform.SetParent(foregroundCanvas.transform, false);
                m_foregroundOutput.AddComponent<RectTransform>().Stretch();

                RenderTextureCamera renderTextureCamera = foregroundCamera.gameObject.AddComponent<RenderTextureCamera>();
                renderTextureCamera.OutputRoot = foregroundCanvas.gameObject.GetComponent<RectTransform>();
                renderTextureCamera.Output = m_foregroundOutput.AddComponent<RawImage>();
                renderTextureCamera.OverlayMaterial = new Material(Shader.Find("Battlehub/URP/RTEditor/UIForeground"));
                foregroundCamera.gameObject.SetActive(true);

                foregroundCanvas.sortingOrder = -1;
            }

            Canvas backgroundCanvas = IOC.Resolve<IRTEAppearance>().UIBackgroundScaler.GetComponent<Canvas>();
            if(backgroundCanvas != null)
            {
                Camera backgroundCamera = backgroundCanvas.worldCamera;
                if(m_cameraUtility != null)
                {
                    m_cameraUtility.EnablePostProcessing(backgroundCamera, false);
                    m_cameraUtility.SetBackgroundColor(backgroundCamera, new Color(0, 0, 0, 0));
                }
            }   
        }
    }
}

