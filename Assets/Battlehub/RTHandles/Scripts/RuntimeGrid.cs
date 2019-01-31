using Battlehub.RTCommon;
using UnityEngine;

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

        protected virtual void Start()
        {
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

