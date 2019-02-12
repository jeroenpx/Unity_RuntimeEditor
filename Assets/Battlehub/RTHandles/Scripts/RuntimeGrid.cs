using Battlehub.RTCommon;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTHandles
{
    /// <summary>
    /// Attach to camera
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class RuntimeGrid : RTEComponent
    {
        public RuntimeHandlesComponent Appearance;
        public float CamOffset = 0.0f;
        public bool AutoCamOffset = true;
        public Vector3 GridOffset;
        public bool UseUIStencilMask = false;

        protected virtual void Start()
        {
            if(UseUIStencilMask)
            {
                Material material = Canvas.GetDefaultCanvasMaterial();
                material.SetFloat("_Stencil", 99);
                material.SetFloat("_StencilComp", (float)CompareFunction.Always);
                material.SetFloat("_StencilOp", (float)StencilOp.Replace);
            }
       
            
            RuntimeHandlesComponent.InitializeIfRequired(ref Appearance);
        }

        private void OnPostRender()
        { 
            if(AutoCamOffset)
            {
                Appearance.DrawGrid(GridOffset, Camera.current.transform.position.y);
            }
            else
            {
                Appearance.DrawGrid(GridOffset, CamOffset);
            }
        }
    }
}

