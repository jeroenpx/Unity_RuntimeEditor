using UnityEngine;

namespace Battlehub.RTCommon
{
 
    /// <summary>
    /// Camera behavior for GL. rendering
    /// </summary>
    [ExecuteInEditMode]
    public class GLCamera : MonoBehaviour
    {
        public int CullingMask = -1;

        private void OnPostRender()
        { 
            if(GLRenderer.Instance != null)
            {
                GLRenderer.Instance.Draw(CullingMask);
            }
        }
    }
}

