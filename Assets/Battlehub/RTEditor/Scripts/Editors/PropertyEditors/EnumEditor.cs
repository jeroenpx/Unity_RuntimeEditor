using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Battlehub.RTEditor
{
    public class EnumEditor : PropertyEditor<Enum>
    {
        [SerializeField]
        private Dropdown m_input;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_input.onValueChanged.AddListener(OnValueChanged);
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            if (m_input != null)
            {
                m_input.onValueChanged.RemoveListener(OnValueChanged);
            }
        }

        public Type GetEnumType(object target)
        {
            CustomTypeFieldAccessor fieldAccessor = target as CustomTypeFieldAccessor;
            if (fieldAccessor != null)
            {
                return fieldAccessor.Type;
            }
            else
            {
                return MemberInfoType;
            }
        }

        protected override void InitOverride(object target, MemberInfo memberInfo, string label = null)
        {
            base.InitOverride(target, memberInfo, label);
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();

            Type enumType = GetEnumType(target);
            string[] names = Enum.GetNames(enumType);

            for (int i = 0; i < names.Length; ++i)
            {
                options.Add(new Dropdown.OptionData(names[i]));
            }

            m_input.options = options;
        }

        protected override void SetInputField(Enum value)
        {
            int index = 0;

            Type enumType = GetEnumType(Target);
            index = Array.IndexOf(Enum.GetValues(enumType), value);
            
            m_input.value = index;
        }

        private void OnValueChanged(int index)
        {
            Type enumType = GetEnumType(Target);
            Enum value = (Enum)Enum.GetValues(enumType).GetValue(index);
            SetValue(value);
            EndEdit();
        }
    }
}
