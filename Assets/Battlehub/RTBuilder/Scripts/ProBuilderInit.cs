using Battlehub.RTCommon;
using Battlehub.RTEditor;
using UnityEngine;
namespace Battlehub.RTBuilder
{
    public class ProBuilderInit : EditorOverride
    {
        [SerializeField]
        private GameObject m_proBuilderWindow = null;

        [SerializeField]
        private GameObject m_materialPaletteWindow = null;

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
                RegisterWindow(wm, "ProBuilder", "Builder", 
                    Resources.Load<Sprite>("hammer-24"), m_proBuilderWindow, false); 
                
                RegisterWindow(wm, "MaterialPalette", "Material Editor", 
                    Resources.Load<Sprite>("palette-24"), m_materialPaletteWindow, false);
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


