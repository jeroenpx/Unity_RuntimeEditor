using Battlehub.RTCommon;
using Battlehub.UIControls.DockPanels;
using UnityEngine;

namespace Battlehub.RTEditor
{
    [DefaultExecutionOrder(-91)]
    public class AutoSaveLayout : EditorOverride
    {
        private IWindowManager m_wm;

        private string m_savedLayoutName;

        protected override void OnEditorCreated(object obj)
        {    
            OverrideDefaultLayout();
            m_wm = IOC.Resolve<IWindowManager>();
            m_savedLayoutName = m_wm.DefaultPersistentLayoutName;
        }

        protected override void OnEditorExist()
        {
            OverrideDefaultLayout();

            IRuntimeEditor editor = IOC.Resolve<IRuntimeEditor>();
            if (editor.IsOpened)
            {
                m_wm = IOC.Resolve<IWindowManager>();
                m_wm.SetLayout(DefaultLayout, RuntimeWindowType.Scene.ToString());
            }
        }

        protected override void OnEditorClosed()
        {
            base.OnEditorClosed();
            string layoutName = m_wm.DefaultPersistentLayoutName;
            m_wm.SaveLayout(m_savedLayoutName);
            m_wm = null;
        }

        private void OnApplicationQuit()
        {
            if(m_wm != null)
            {
                m_wm.SaveLayout(m_savedLayoutName);
            }
        }

        private void OverrideDefaultLayout()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.OverrideDefaultLayout(DefaultLayout, RuntimeWindowType.Scene.ToString());
        }

        protected virtual LayoutInfo DefaultLayout(IWindowManager wm)
        {
            if (wm.LayoutExist(m_savedLayoutName))
            {
                LayoutInfo layout = wm.GetLayout(m_savedLayoutName);
                return layout;
            }

            return wm.GetBuiltInDefaultLayout();
        }
    }

}
