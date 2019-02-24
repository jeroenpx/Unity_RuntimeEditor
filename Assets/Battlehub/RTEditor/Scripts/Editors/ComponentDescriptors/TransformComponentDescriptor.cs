using UnityEngine;
using System.Reflection;
using Battlehub.Utils;

namespace Battlehub.RTEditor
{
    public class TransformComponentDescriptor : ComponentDescriptorBase<Transform>
    {
        public override object CreateConverter(ComponentEditor editor)
        {
            TransformPropertyConverter converter = new TransformPropertyConverter();
            converter.Component = (Transform)editor.Component;
            return converter;
        }

        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converterObj)
        {
            TransformPropertyConverter converter = (TransformPropertyConverter)converterObj;

            MemberInfo position = Strong.PropertyInfo((Transform x) => x.position, "position");
            MemberInfo rotation = Strong.PropertyInfo((Transform x) => x.rotation, "rotation");
            MemberInfo rotationConverted = Strong.PropertyInfo((TransformPropertyConverter x) => x.Rotation, "Rotation");
            MemberInfo scale = Strong.PropertyInfo((Transform x) => x.localScale, "localScale");

            return new[]
                {
                    new PropertyDescriptor( "Position", editor.Component, position, position) ,
                    new PropertyDescriptor( "Rotation", converter, rotationConverted, rotation),
                    new PropertyDescriptor( "Scale", editor.Component, scale, scale)
                };
        }
    }

    public class TransformPropertyConverter 
    {
        public Vector3 Rotation
        {
            get
            {
                if(Component == null)
                {
                    return Vector3.zero;
                }
                return Component.rotation.eulerAngles;
            }
            set
            {
                if (Component == null)
                {
                    return;
                }
                Component.rotation = Quaternion.Euler(value);
            }
        }

        public Transform Component
        {
            get;
            set;
        }
    }
}

