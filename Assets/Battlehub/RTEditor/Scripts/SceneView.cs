using Battlehub.RTCommon;

namespace Battlehub.RTEditor
{
    public class SceneView : RuntimeWindow
    {      
        protected override void AwakeOverride()
        {
            ActivateOnAnyKey = true;
            WindowType = RuntimeWindowType.Scene;
            base.AwakeOverride();
           
            if(!GetComponent<SceneViewInput>())
            {
                gameObject.AddComponent<SceneViewInput>();
            }    

            if(!GetComponent<SceneViewImpl>())
            {
                gameObject.AddComponent<SceneViewImpl>();
            }
        }

       

        
    }
}
