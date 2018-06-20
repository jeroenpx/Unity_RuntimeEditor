using UnityEngine;
using System.Reflection;
using System;
using Battlehub.Utils;

namespace Battlehub.RTEditor
{
    public class RigidbodyComponentDescriptor : IComponentDescriptor
    {
        public string DisplayName
        {
            get { return ComponentType.Name; }
        }

        public Type ComponentType
        {
            get { return typeof(Rigidbody); }
        }

        public Type GizmoType
        {
            get { return null; }
        }

        public object CreateConverter(ComponentEditor editor)
        {
            return null;
        }

        public PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
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
                new PropertyDescriptor("Mass", editor.Component, massInfo, massInfo),
                new PropertyDescriptor("Drag", editor.Component, dragInfo, dragInfo),
                new PropertyDescriptor("Angular Drag", editor.Component, angularDragInfo, angularDragInfo),
                new PropertyDescriptor("Use Gravity", editor.Component, useGravityInfo, useGravityInfo),
                new PropertyDescriptor("Is Kinematic", editor.Component, isKinematicInfo, isKinematicInfo),
                new PropertyDescriptor("Interpolation", editor.Component, interpolationInfo, interpolationInfo),
                new PropertyDescriptor("Collision Detection", editor.Component, collisionDetectionInfo, collisionDetectionInfo),
            };
        }
    }

}

