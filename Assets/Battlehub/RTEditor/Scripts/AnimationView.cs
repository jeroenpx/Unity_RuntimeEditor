using Battlehub.RTCommon;

namespace Battlehub.RTEditor
{
    public class AnimationView : RuntimeWindow
    {
        protected override void AwakeOverride()
        {
            WindowType = RuntimeWindowType.Animation;
            base.AwakeOverride();
        }
    }
}

