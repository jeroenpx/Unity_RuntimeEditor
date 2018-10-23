using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class RuntimeSceneView : RuntimeWindow
    {
        protected override void OnActivated()
        {
            base.OnActivated();
            Debug.Log("On SceneView activated");
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            Debug.Log("On SceneView deactivated");
        }
    }

}
