using Battlehub.Utils;
using System.Reflection;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public class FixedJointComponentDescriptor : ComponentDescriptorBase<FixedJoint>
    {
        public override PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter)
        {
            MemberInfo connectedBodyInfo = Strong.PropertyInfo((FixedJoint x) => x.connectedBody, "connectedBody");
            MemberInfo breakForceInfo = Strong.PropertyInfo((FixedJoint x) => x.breakForce, "breakForce");
            MemberInfo breakTorqueInfo = Strong.PropertyInfo((FixedJoint x) => x.breakTorque, "breakTorque");
            MemberInfo enableCollisionInfo = Strong.PropertyInfo((FixedJoint x) => x.enableCollision, "enableCollision");
            MemberInfo enablePreporcessingInfo = Strong.PropertyInfo((FixedJoint x) => x.enablePreprocessing, "enablePreprocessing");

            return new[]
            {
                new PropertyDescriptor("ConnectedBody", editor.Component, connectedBodyInfo),
                new PropertyDescriptor("Break Force", editor.Component, breakForceInfo, "m_BreakForce"),
                new PropertyDescriptor("Break Torque", editor.Component, breakTorqueInfo, "m_BreakTorque"),
                new PropertyDescriptor("Enable Collision", editor.Component, enableCollisionInfo, "m_EnableCollision"),
                new PropertyDescriptor("Enable Preprocessing", editor.Component, enablePreporcessingInfo, "m_EnablePreprocessing"),
            };            
        }
    }
}
