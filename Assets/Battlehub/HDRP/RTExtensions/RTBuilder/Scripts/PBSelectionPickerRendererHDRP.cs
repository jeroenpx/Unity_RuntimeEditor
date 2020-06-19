using Battlehub.ProBuilderIntegration;
using Battlehub.RTCommon;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Rendering.HighDefinition;

namespace Battlehub.RTBuilder.HDRP
{
    public class PBSelectionPickerRendererHDRP : PBSelectionPickerRenderer
    {
        private IRenderersCache m_cache;
        private CustomPassVolume m_customPassVolume;

        public PBSelectionPickerRendererHDRP(IRenderersCache cache, CustomPassVolume volume)
        {
            m_cache = cache;
            m_customPassVolume = volume;
            m_customPassVolume.enabled = false;
        }

        protected override void PrepareCamera(Camera renderCamera)
        {
            base.PrepareCamera(renderCamera);

            IRenderPipelineCameraUtility cameraUtil = IOC.Resolve<IRenderPipelineCameraUtility>();
            cameraUtil.EnablePostProcessing(renderCamera, false);
        }

        protected override void Render(Shader shader, string tag, Camera renderCam)
        {
            m_customPassVolume.enabled = true;

            //bool invertCulling = GL.invertCulling;

            // GL.invertCulling = true;
            //renderCam.projectionMatrix *= Matrix4x4.Scale(new Vector3(1, -1, 1));

            HDAdditionalCameraData cameraData = renderCam.GetComponent<HDAdditionalCameraData>();
            cameraData.flipYMode = HDAdditionalCameraData.FlipYMode.Automatic;
            cameraData.invertFaceCulling = true;
            
            renderCam.Render();
            //GL.invertCulling = invertCulling;

            m_cache.Clear();

            m_customPassVolume.enabled = false;
        }

        protected override void GenerateEdgePickingObjects(IList<ProBuilderMesh> selection, bool doDepthTest, out Dictionary<uint, SimpleTuple<ProBuilderMesh, Edge>> map, out GameObject[] depthObjects, out GameObject[] pickerObjects)
        {
            base.GenerateEdgePickingObjects(selection, doDepthTest, out map, out depthObjects, out pickerObjects);
            if (depthObjects != null)
            {
                m_cache.Add(depthObjects.SelectMany(go => go.GetComponentsInChildren<Renderer>()).ToArray());
            }
            if (pickerObjects != null)
            {
                m_cache.Add(pickerObjects.SelectMany(go => go.GetComponentsInChildren<Renderer>()).ToArray());
            }
        }

        protected override GameObject[] GenerateFacePickingObjects(IList<ProBuilderMesh> selection, out Dictionary<uint, SimpleTuple<ProBuilderMesh, Face>> map)
        {
            GameObject[] pickerObjects = base.GenerateFacePickingObjects(selection, out map);
            m_cache.Add(pickerObjects.SelectMany(go => go.GetComponentsInChildren<Renderer>()).ToArray());
            return pickerObjects;
        }

        protected override void GenerateVertexPickingObjects(IList<ProBuilderMesh> selection, bool doDepthTest, out Dictionary<uint, SimpleTuple<ProBuilderMesh, int>> map, out GameObject[] depthObjects, out GameObject[] pickerObjects)
        {
            base.GenerateVertexPickingObjects(selection, doDepthTest, out map, out depthObjects, out pickerObjects);
            if (depthObjects != null)
            {
                m_cache.Add(depthObjects.SelectMany(go => go.GetComponentsInChildren<Renderer>()).ToArray());
            }
            if (pickerObjects != null)
            {
                m_cache.Add(pickerObjects.SelectMany(go => go.GetComponentsInChildren<Renderer>()).ToArray());
            }
        }
    }

}
