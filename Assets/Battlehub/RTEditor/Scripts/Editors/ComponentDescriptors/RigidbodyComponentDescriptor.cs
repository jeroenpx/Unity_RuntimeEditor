using UnityEngine;
using System.Reflection;
using Battlehub.Utils;

namespace Battlehub.RTEditor
{
    public class RigidbodyComponentDescriptor : ComponentDescriptorBase<Rigidbody>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            MemberInfo massInfo = Strong.PropertyInfo((Rigidbody x) => x.mass, "mass");
            MemberInfo dragInfo = Strong.PropertyInfo((Rigidbody x) => x.drag, "drag");
            MemberInfo angularDragInfo = Strong.PropertyInfo((Rigidbody x) => x.angularDrag, "angularDrag");
            MemberInfo useGravityInfo = Strong.PropertyInfo((Rigidbody x) => x.useGravity, "useGravity");
            MemberInfo isKinematicInfo = Strong.PropertyInfo((Rigidbody x) => x.isKinematic, "isKinematic");
            MemberInfo interpolationInfo = Strong.PropertyInfo((Rigidbody x) => x.interpolation, "interpolation");
            MemberInfo collisionDetectionInfo = Strong.PropertyInfo((Rigidbody x) => x.collisionDetectionMode, "collisionDetectionMode");

            return new[]
            {
                new PropertyDescriptor("Mass", editor.Component, massInfo, "m_Mass"),
                new PropertyDescriptor("Drag", editor.Component, dragInfo, "m_Drag"),
                new PropertyDescriptor("Angular Drag", editor.Component, angularDragInfo, "m_AngularDrag"),
                new PropertyDescriptor("Use Gravity", editor.Component, useGravityInfo, "m_UseGravity"),
                new PropertyDescriptor("Is Kinematic", editor.Component, isKinematicInfo, "m_IsKinematic"),
                new PropertyDescriptor("Interpolation", editor.Component, interpolationInfo, interpolationInfo),
                new PropertyDescriptor("Collision Detection", editor.Component, collisionDetectionInfo, collisionDetectionInfo),
            };
        }
    }

}

