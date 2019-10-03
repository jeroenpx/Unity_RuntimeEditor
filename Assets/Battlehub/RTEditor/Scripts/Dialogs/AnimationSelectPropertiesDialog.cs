using Battlehub.RTCommon;
using Battlehub.UIControls;
using Battlehub.UIControls.Dialogs;
using System;
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

        GameObject Target
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

        public GameObject Target
        {
            get;
            set;
        }

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
        }

        protected virtual void Start()
        {
            m_parentDialog = GetComponentInParent<Dialog>();
            if (m_parentDialog != null)
            {
                m_parentDialog.IsOkVisible = true;
            }

            //Type[] editableTypes = IOC.Resolve<IEditorsMap>().GetEditableTypes();

            m_propertiesTreeView.Items = new[]
            {
                new AnimationPropertyItem()
            };         
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

