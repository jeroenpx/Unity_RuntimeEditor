using Battlehub.RTCommon;
using Battlehub.RTHandles;
using Battlehub.UIControls.Dialogs;
using Battlehub.Utils;
using UnityEngine;

namespace Battlehub.RTEditor
{
    public interface ISceneSettingsDialog
    {
        RuntimeWindow Scene
        {
            get;
            set;
        }
    }

    public class SceneSettingsDialog : RuntimeWindow, ISceneSettingsDialog
    {
        [SerializeField]
        private BoolEditor m_isGridVisibleEditor = null;

        [SerializeField]
        private BoolEditor m_snapToGridEditor = null;

        [SerializeField]
        private RangeEditor m_gridSizeEditor = null;
        
        public bool IsGridVisible
        {
            get { return m_sceneComponent.IsGridVisible; }
            set { m_sceneComponent.IsGridVisible = value; }
        }

        public bool SnapToGrid
        {
            get { return m_sceneComponent.IsGridEnabled; }
            set { m_sceneComponent.IsGridEnabled = value; }
        }

        public float GridSize
        {
            get { return m_sceneComponent.SizeOfGrid; }
            set { m_sceneComponent.SizeOfGrid = value; }
        }

        private IRuntimeSceneComponent m_sceneComponent;
        private RuntimeWindow m_window;
        public RuntimeWindow Scene
        {
            get { return m_window; }
            set
            {
                m_window = value;
                m_sceneComponent = m_window.IOCContainer.Resolve<IRuntimeSceneComponent>();
            }
        }

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            IOC.RegisterFallback<ISceneSettingsDialog>(this);
        }

        private Dialog m_parentDialog;

        protected virtual void Start()
        {
            m_parentDialog = GetComponentInParent<Dialog>();
            if(m_parentDialog != null)
            {
                m_parentDialog.IsOkVisible = true;
            }

            if(m_isGridVisibleEditor != null)
            {
                m_isGridVisibleEditor.Init(this, this, Strong.PropertyInfo((SceneSettingsDialog x) => x.IsGridVisible), null, "Is Grid Visible");
            }

            if(m_snapToGridEditor != null)
            {
                m_snapToGridEditor.Init(this, this, Strong.PropertyInfo((SceneSettingsDialog x) => x.SnapToGrid), null, "Snap To Grid");
            }

            if(m_gridSizeEditor != null)
            {
                m_gridSizeEditor.Min = 0.1f;
                m_gridSizeEditor.Max = 8;
                m_gridSizeEditor.Init(this, this, Strong.PropertyInfo((SceneSettingsDialog x) => x.GridSize), null, "Grid Size");
            }
        }

        protected override void OnDestroyOverride()
        {
            base.OnDestroyOverride();
            IOC.UnregisterFallback<ISceneSettingsDialog>(this);
        }
    }
}
