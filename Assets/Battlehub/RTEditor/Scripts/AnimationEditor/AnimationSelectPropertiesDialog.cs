using Battlehub.RTCommon;
using Battlehub.UIControls;
using Battlehub.UIControls.Dialogs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public interface IAnimationSelectPropertiesDialog
    {
        AnimationPropertiesView View
        {
            get;
            set;
        }

        RuntimeAnimation Target
        {
            get;
            set;
        }
    }

    public class AnimationSelectPropertiesDialog : RuntimeWindow, IAnimationSelectPropertiesDialog
    {
        [SerializeField]
        private VirtualizingTreeView m_propertiesTreeView = null;
        private Dialog m_parentDialog;

        public AnimationPropertiesView View
        {
            get;
            set;
        }

        public RuntimeAnimation Target
        {
            get;
            set;
        }

        private VoidComponentEditor m_voidComponentEditor;
        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            IOC.RegisterFallback<IAnimationSelectPropertiesDialog>(this);

            if(m_propertiesTreeView != null)
            {
                m_propertiesTreeView.CanReorder = false;
                m_propertiesTreeView.CanReparent = false;
                m_propertiesTreeView.CanDrag = false;
                m_propertiesTreeView.CanSelectAll = false;
                m_propertiesTreeView.CanMultiSelect = false;
                m_propertiesTreeView.CanEdit = false;

                m_propertiesTreeView.ItemDataBinding += OnItemDatabinding;
                m_propertiesTreeView.ItemExpanding += OnItemExpanding;
            }

             m_voidComponentEditor = gameObject.AddComponent<VoidComponentEditor>();
        }

        protected virtual void Start()
        {
            m_parentDialog = GetComponentInParent<Dialog>();
            if (m_parentDialog != null)
            {
                m_parentDialog.IsOkVisible = true;
            }

            List<AnimationPropertyItem> components = new List<AnimationPropertyItem>();
            IEditorsMap editorsMap = IOC.Resolve<IEditorsMap>();
            Type[] editableTypes = editorsMap.GetEditableTypes();
            for (int i = 0; i < editableTypes.Length; ++i)
            {
                Type editableType = editableTypes[i];
                if (!(typeof(Component).IsAssignableFrom(editableType)) || typeof(Component) == editableType)
                {
                    continue;
                }
                m_voidComponentEditor.Component = Target.GetComponent(editableType);
                if(m_voidComponentEditor.Component == null)
                {
                    continue;
                }

                AnimationPropertyItem component = new AnimationPropertyItem();
                component.ComponentName = editableType.Name;
                component.ComponentType = editableType.FullName;
                component.Children = new List<AnimationPropertyItem>();
                
                PropertyDescriptor[] propertyDescriptors = editorsMap.GetPropertyDescriptors(editableType, m_voidComponentEditor);
                for (int j = 0; j < propertyDescriptors.Length; ++j)
                {
                    PropertyDescriptor propertyDescriptor = propertyDescriptors[j];
                    Type memberType = propertyDescriptor.ComponentMemberType;
                    if(memberType.IsClass || memberType.IsEnum)
                    {
                        continue;
                    }

                    AnimationPropertyItem property = new AnimationPropertyItem();
                    property.Value = m_voidComponentEditor.Component;
                    property.Parent = component;
                    property.PropertyName = propertyDescriptor.Label;
                    
                    component.Children.Add(property);
                }

                if(component.Children.Count > 0)
                {
                    components.Add(component);
                }

                m_voidComponentEditor.Component = null;
            }

            m_propertiesTreeView.Items = components;
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            IOC.UnregisterFallback<IAnimationSelectPropertiesDialog>(this);

            if (m_propertiesTreeView != null)
            {
                m_propertiesTreeView.ItemDataBinding -= OnItemDatabinding;
                m_propertiesTreeView.ItemExpanding -= OnItemExpanding;
            }
        }

        private void OnItemExpanding(object sender, VirtualizingItemExpandingArgs e)
        {
            AnimationPropertyItem item = (AnimationPropertyItem)e.Item;
            e.Children = item.Children;
        }

        private void OnItemDatabinding(object sender, VirtualizingTreeViewItemDataBindingArgs e)
        {
            AnimationComponentView ui = e.ItemPresenter.GetComponent<AnimationComponentView>();
            AnimationPropertyItem item = (AnimationPropertyItem)e.Item;
            ui.Item = item;
            ui.View = View;

            e.HasChildren = item.Children != null && item.Children.Count > 0;
        }
    }
}

