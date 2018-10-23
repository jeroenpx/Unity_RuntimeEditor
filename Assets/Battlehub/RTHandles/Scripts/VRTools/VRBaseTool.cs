using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTHandles
{
    public class VRBaseTool : RTEBehaviour
    {
        public RuntimeSceneVRComponent Component
        {
            get;
            set;
        }

        protected override void OnWindowRegistered(RuntimeWindow window)
        {
            base.OnWindowRegistered(window);
            if (IsSupported(window))
            {
                RuntimeGraphicsLayer graphicsLayer = window.GetComponent<RuntimeGraphicsLayer>();
                if (graphicsLayer == null)
                {
                    graphicsLayer = window.gameObject.AddComponent<RuntimeGraphicsLayer>();
                }

                SetLayer(transform, graphicsLayer.Window.Editor.CameraLayerSettings.RuntimeHandlesLayer);
            }
        }

        private void SetLayer(Transform t, int layer)
        {
            t.gameObject.layer = layer;
            foreach (Transform child in t)
            {
                SetLayer(child, layer);
            }
        }
    }
}
