using Battlehub.RTCommon;
using Battlehub.UIControls.MenuControl;
using UnityEngine;

namespace Battlehub.RTEditor
{
    [MenuDefinition]
    public class NavigationInit : EditorExtension
    {
        [SerializeField]
        private GameObject m_prefab = null;

        protected override void OnEditorExist()
        {
            base.OnEditorExist();

            ILocalization lc = IOC.Resolve<ILocalization>();
            lc.LoadStringResources("RTNavigation.StringResources");

            IWindowManager wm = IOC.Resolve<IWindowManager>();
            Sprite icon = Resources.Load<Sprite>("RTN_Header");
            bool isDialog = false;
            RegisterWindow(wm, "NavigationView", "ID_RTNavigation_WM_Header_Navigation", icon, m_prefab, isDialog);
        }

        private void RegisterWindow(IWindowManager wm, string typeName, string header, Sprite icon, GameObject prefab, bool isDialog)
        {
            wm.RegisterWindow(new CustomWindowDescriptor
            {
                IsDialog = isDialog,
                TypeName = typeName,
                Descriptor = new WindowDescriptor
                {
                    Header = header,
                    Icon = icon,
                    MaxWindows = 1,
                    ContentPrefab = prefab
                }
            });
        }

        [MenuCommand("MenuWindow/ID_RTNavigation_WM_Header_Navigation", "", true)]
        public static void Open()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            wm.CreateWindow("NavigationView");
        }
    }
}
