using Battlehub.RTCommon;
using Battlehub.RTEditor;
using UnityEngine;
namespace Battlehub.RTBuilder
{
    public class ProBuilderInit : EditorOverride
    {
        [SerializeField]
        private GameObject m_proBuilderWindow = null;

        protected override void OnEditorExist()
        {
            base.OnEditorExist();
            Register();
        }

        private void Register()
        {
            IWindowManager wm = IOC.Resolve<IWindowManager>();
            if (m_proBuilderWindow != null)
            {
                Sprite icon = Resources.Load<Sprite>("hammer-24");
                RegisterWindow(wm, "ProBuilder", "Builder", icon, m_proBuilderWindow, false);
            }
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
    }
}


