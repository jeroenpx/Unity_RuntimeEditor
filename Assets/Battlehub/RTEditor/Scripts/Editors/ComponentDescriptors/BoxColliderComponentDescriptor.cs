using UnityEngine;
using System.Reflection;
using System;
using Battlehub.Utils;
using Battlehub.RTGizmos;

namespace Battlehub.RTEditor
{
    public class BoxColliderComponentDescriptor : ComponentDescriptorBase<BoxCollider, BoxColliderGizmo>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            MemberInfo isTriggerInfo = Strong.PropertyInfo((BoxCollider x) => x.isTrigger, "isTrigger");
            MemberInfo materialInfo = Strong.PropertyInfo((BoxCollider x) => x.sharedMaterial, "sharedMaterial");
            MemberInfo centerInfo = Strong.PropertyInfo((BoxCollider x) => x.center, "center");
            MemberInfo sizeInfo = Strong.PropertyInfo((BoxCollider x) => x.size, "size");

            return new[]
            {
                new PropertyDescriptor("Is Trigger", editor.Component, isTriggerInfo, "m_IsTrigger"),
                new PropertyDescriptor("Material", editor.Component, materialInfo),
                new PropertyDescriptor("Center", editor.Component, centerInfo, "m_Center"),
                new PropertyDescriptor("Size", editor.Component, sizeInfo, "m_Size"),
            };
        }
    }
}

