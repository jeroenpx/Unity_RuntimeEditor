﻿using UnityEngine;

using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;

namespace Battlehub.RTEditor
{
    public class RangeOptions : Range
    {
        public string[] Options;

        public RangeOptions(params string[] options) : base(-1, -1)
        {
            Options = options;
        }
    }

    public class OptionsEditor : OptionsEditor<int>
    {
        protected override void SetInputField(int value)
        {
            if (HasMixedValues())
            {
                m_mixedValuesIndicator.text = "-";
            }
            else
            {
                m_input.value = value;
                m_mixedValuesIndicator.text = m_input.options[value].text;
            }
        }

        protected override void OnValueChanged(int index)
        {
            SetValue(index);
            SetInputField(index);
            EndEdit();
        }

        protected override void InitOverride(object[] target, object[] accessor, MemberInfo memberInfo, Action<object, object> eraseTargetCallback, string label = null)
        {
            base.InitOverride(target, accessor, memberInfo, eraseTargetCallback, label);
            m_currentValue = -1;
        }
    }

    public class OptionsEditor<T> : PropertyEditor<T>
    {
        [SerializeField]
        protected TMP_Dropdown m_input = null;

        [SerializeField]
        protected TextMeshProUGUI m_mixedValuesIndicator = null;

        public string[] Options = new string[0];

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

        protected override void InitOverride(object[] target, object[] accessor, MemberInfo memberInfo, Action<object, object> eraseTargetCallback, string label = null)
        {
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

            for (int i = 0; i < Options.Length; ++i)
            {
                options.Add(new TMP_Dropdown.OptionData(Options[i]));
            }

            m_input.options = options;

            base.InitOverride(target, accessor, memberInfo, eraseTargetCallback, label);
        }

        protected virtual void OnValueChanged(int index)
        {
           
        }
    }
}
