using System.Reflection;
using Battlehub.Utils;

namespace Battlehub.RTEditor
{
    public class RuntimeAnimationComponentDescriptor : ComponentDescriptorBase<RuntimeAnimation>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            MemberInfo clipsInfo = Strong.PropertyInfo((RuntimeAnimation x) => x.Clips, "Clips");

            return new[]
            {
                new PropertyDescriptor("Clips", editor.Component, clipsInfo)
            };
        }
    }
}

