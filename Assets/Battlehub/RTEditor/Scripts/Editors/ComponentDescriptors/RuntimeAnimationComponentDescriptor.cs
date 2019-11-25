using System.Reflection;
using Battlehub.Utils;

namespace Battlehub.RTEditor
{
    public class RuntimeAnimationComponentDescriptor : ComponentDescriptorBase<RuntimeAnimation>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            MemberInfo playOnAwakeInfo = Strong.PropertyInfo((RuntimeAnimation x) => x.PlayOnAwake, "PlayOnAwake");
            MemberInfo loopInfo = Strong.PropertyInfo((RuntimeAnimation x) => x.Loop, "Loop");
            MemberInfo clipsInfo = Strong.PropertyInfo((RuntimeAnimation x) => x.Clips, "Clips");

            return new[]
            {   
                new PropertyDescriptor("Play On Awake", editor.Component, playOnAwakeInfo),
                new PropertyDescriptor("Loop", editor.Component, loopInfo),
                new PropertyDescriptor("Clips", editor.Component, clipsInfo)
            };
        }
    }
}

