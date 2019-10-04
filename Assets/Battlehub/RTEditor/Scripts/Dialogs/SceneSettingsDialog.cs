using Battlehub.RTCommon;
using Battlehub.UIControls.Dialogs;
using Battlehub.Utils;
using UnityEngine;

namespace Battlehub.RTEditor
{
     public class SceneSettingsDialog : RuntimeWindow
    {
        [SerializeField]
        private BoolEditor m_isGridVisibleEditor = null;

        [SerializeField]
        private BoolEditor m_snapToGridEditor = null;

        [SerializeField]
        private RangeEditor m_gridSizeEditor = null;
        
        public bool IsGridVisible
        {
            get { return m_settings.IsGridVisible; }
            set { m_settings.IsGridVisible = value; }
        }

        public bool SnapToGrid
        {
            get { return m_settings.IsGridEnabled; }
            set { m_settings.IsGridEnabled = value; }
        }

        public float GridSize
        {
            get { return m_settings.GridSize; }
            set { m_settings.GridSize = value; }
        }

        private ISceneSettingsComponent m_settings;
        private Dialog m_parentDialog;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            m_settings = IOC.Resolve<ISceneSettingsComponent>();
        }
        
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
    }
}
