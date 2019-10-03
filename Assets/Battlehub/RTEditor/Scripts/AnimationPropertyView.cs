using Battlehub.UIControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTEditor
{
    public class AnimationPropertyItem
    {
        public string ComponentType;
        public string ComponentName;
        public string PropertyName;
        public AnimationPropertyItem Parent;
        public List<AnimationPropertyItem> Children;

        private object m_value;
        public object Value
        {
            get
            {
                if (Parent != null)
                {
                    Type propetyType = Parent.Value.GetType();
                    string path = (string)m_value;

                    MemberInfo[] members = propetyType.GetMember(path);
                    if (members[0].MemberType == MemberTypes.Field)
                    {
                        FieldInfo fieldInfo = (FieldInfo)members[0];
                        return fieldInfo.GetValue(Parent.Value);
                    }
                    else if (members[0].MemberType == MemberTypes.Property)
                    {
                        PropertyInfo propInfo = (PropertyInfo)members[0];
                        return propInfo.GetValue(Parent.Value);
                    }
                    throw new InvalidOperationException("Unable to get propery or field: " + path);
                }

                return m_value;
            }
            set
            {
                if (Parent != null)
                {
                    Type propetyType = Parent.Value.GetType();
                    string path = (string)m_value;

                    MemberInfo[] members = propetyType.GetMember(path);
                    if (members[0].MemberType == MemberTypes.Field)
                    {
                        FieldInfo fieldInfo = (FieldInfo)members[0];
                        fieldInfo.SetValue(Parent.Value, value);

                        Parent.Value = Parent.Value;
                    }
                    else if (members[0].MemberType == MemberTypes.Property)
                    {
                        PropertyInfo propInfo = (PropertyInfo)members[0];
                        propInfo.SetValue(Parent.Value, value);

                        Parent.Value = Parent.Value;
                    }
                    else
                    {
                        throw new InvalidOperationException("Unable to get propery or field: " + path);
                    }
                }
                else
                {
                    m_value = value;
                }
            }
        }

        public bool IsValid()
        {
            if (Parent != null)
            {
                Type propetyType = Parent.Value.GetType();
                string path = (string)m_value;

                MemberInfo[] members = propetyType.GetMember(path);
                if (members.Length == 0)
                {
                    Debug.LogError("Unable to find member with name: " + path);
                    return false;
                }
                if (members[0].MemberType != MemberTypes.Field && members[0].MemberType != MemberTypes.Property)
                {
                    Debug.LogError("Member is not field or property: " + path);
                    return false;
                }
            }
            return true;
        }

        public void TryToCreateChildren()
        {
            Type type = m_value.GetType();
            if (Reflection.IsPrimitive(type))
            {
                return;
            }

            if (!Reflection.IsValueType(type))
            {
                return;
            }

            List<AnimationPropertyItem> children = new List<AnimationPropertyItem>();
            FieldInfo[] fields = type.GetFields().Where(f => Reflection.IsPrimitive(f.FieldType)).ToArray();
            for (int i = 0; i < fields.Length; ++i)
            {
                AnimationPropertyItem child = new AnimationPropertyItem
                {
                    Value = fields[i].Name,
                    Parent = this
                };
                children.Add(child);
            }

            PropertyInfo[] properties = type.GetProperties().Where(p => Reflection.IsPrimitive(p.PropertyType)).ToArray();
            for (int i = 0; i < properties.Length; ++i)
            {
                AnimationPropertyItem child = new AnimationPropertyItem
                {
                    Value = properties[i].Name,
                    Parent = this
                };
                children.Add(child);
            }

            Children = children;
        }
    }

    public class AnimationPropertyView : MonoBehaviour
    {        
        [SerializeField]
        private TextMeshProUGUI m_label = null;

        [SerializeField]
        private TMP_InputField m_inputField = null;

        [SerializeField]
        private Toggle m_toggle = null;

        [SerializeField]
        private Button m_addPropertyButton = null;

        [SerializeField]
        private GameObject m_selectionHighlight = null;

        private AnimationPropertyItem m_item;
        public AnimationPropertyItem Item
        {
            get { return m_item; }
            set
            {
                m_item = value;

                if(m_item != null)
                {
                    bool isBool = m_item.Value is bool;
                    if (m_toggle != null)
                    {
                        m_toggle.gameObject.SetActive(isBool);
                    }

                    if (m_inputField != null)
                    {
                        m_inputField.gameObject.SetActive(!isBool);
                        if (!isBool && m_item.Value != null)
                        {
                            Type type = m_item.Value.GetType();
                            if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) || type == typeof(byte))
                            {
                                m_inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
                            }
                            else if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
                            {
                                m_inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
                            }
                            else
                            {
                                m_inputField.contentType = TMP_InputField.ContentType.Standard;
                            }
                        }
                        else
                        {
                            m_inputField.contentType = TMP_InputField.ContentType.Standard;
                        }
                    }

                    if (m_label != null)
                    {
                        if (m_item.Parent == null)
                        {
                            m_label.name = string.Format("{0} : {1}", m_item.ComponentName, m_item.PropertyName);
                        }
                        else
                        {
                            m_label.name = string.Format("{0} : {1}", m_item.Parent.PropertyName, m_item.PropertyName);
                        }
                    }
                }
                else
                {
                    if(m_inputField != null)
                    {
                        m_inputField.gameObject.SetActive(false);
                    }
                    if(m_toggle != null)
                    {
                        m_toggle.gameObject.SetActive(false);
                    }
                    if(m_label != null)
                    {
                        m_label.gameObject.SetActive(false);
                    }
                }
            }
        }

        public bool IsAddPropertyButtonVisible
        {
            get
            {
                if(m_addPropertyButton != null)
                {
                    return m_addPropertyButton.gameObject.activeSelf;
                }
                return false;
            }
            set
            {
                if(m_addPropertyButton != null)
                {
                    m_addPropertyButton.gameObject.SetActive(value);
                }

                if(m_selectionHighlight != null)
                {
                    m_selectionHighlight.SetActive(value);
                }
            }
        }

        public AnimationPropertiesView View
        {
            get;
            set;
        }

        private void Awake()
        {
            UnityEventHelper.AddListener(m_inputField, input => input.onEndEdit, OnInputFieldEndEdit);
            UnityEventHelper.AddListener(m_addPropertyButton, button => button.onClick, OnAddPropertyButtonClick);
            UnityEventHelper.AddListener(m_toggle, toggle => toggle.onValueChanged, OnToggleValueChange);
        }

        private void OnDestroy()
        {
            UnityEventHelper.RemoveListener(m_inputField, input => input.onEndEdit, OnInputFieldEndEdit);
            UnityEventHelper.RemoveListener(m_addPropertyButton, button => button.onClick, OnAddPropertyButtonClick);
            UnityEventHelper.RemoveListener(m_toggle, toggle => toggle.onValueChanged, OnToggleValueChange);
        }

        private void OnAddPropertyButtonClick()
        {
            View.AddProperty(Item);
        }

        private void OnInputFieldEndEdit(string value)
        {

        }

        private void OnToggleValueChange(bool value)
        {

        }
    }
}

