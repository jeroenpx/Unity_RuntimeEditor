using Battlehub.RTCommon;
using System;

namespace Battlehub.RTEditor
{
    public struct HeaderDescriptor
    {
        public string DisplayName
        {
            get; set;
        }
        public bool ShowExpander
        {
            get; set;
        }

        public bool ShowResetButton
        {
            get; set;
        }

        public HeaderDescriptor(string displayName, bool showExpander = true, bool showResetButton = true)
        {
            DisplayName = displayName;
            ShowExpander = showExpander;
            ShowResetButton = showResetButton;
        }
    }

    public interface IComponentDescriptor
    {
        HeaderDescriptor HeaderDescriptor { get; }

        Type ComponentType { get; }

        Type GizmoType { get; }

        object CreateConverter(ComponentEditor editor);

        PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter);
    }

    public abstract class ComponentDescriptorBase<TComponent> : IComponentDescriptor
    {
        public virtual HeaderDescriptor HeaderDescriptor
        {
            get
            {
                return new HeaderDescriptor(
                    ComponentType.Name,
                    RuntimeEditorApplication.ComponentEditorSettings.ShowExpander,
                    RuntimeEditorApplication.ComponentEditorSettings.ShowResetButton);
            }
        }

        public virtual Type ComponentType
        {
            get { return typeof(TComponent); }
        }

        public virtual Type GizmoType
        {
            get { return null; }
        }

        public virtual object CreateConverter(ComponentEditor editor)
        {
            return null;
        }

        public abstract PropertyDescriptor[] GetProperties(ComponentEditor editor, object converter);
    }

    public abstract class ComponentDescriptorBase<TComponent, TGizmo> : ComponentDescriptorBase<TComponent>
    {
        public override Type GizmoType
        {
            get { return typeof(TGizmo); }
        }
    }

}