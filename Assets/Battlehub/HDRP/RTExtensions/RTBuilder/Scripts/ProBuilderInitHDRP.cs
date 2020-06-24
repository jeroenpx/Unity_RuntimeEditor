using Battlehub.ProBuilderIntegration;
using Battlehub.RTCommon;
using Battlehub.RTEditor;
using System.Collections.Generic;
using UnityEngine.Rendering.HighDefinition;

namespace Battlehub.RTBuilder.HDRP
{
    public class ProBuilderInitHDRP : EditorExtension
    {
        //private RenderersCache m_selectionPickerCache;
        //private CustomPassVolume m_volume;

        protected override void OnEditorExist()
        {
            if (RenderPipelineInfo.Type != RPType.HDRP)
            {
                Destroy(this);
                return;
            }

            base.OnEditorExist();
            // m_selectionPickerCache = gameObject.AddComponent<RenderersCache>();
            //IOC.Register<IRenderersCache>("ProBuilder.SelectionPickerCache", m_selectionPickerCache);

            // RenderCache renderCache = new RenderCache();
            //renderCache.Cache = m_selectionPickerCache;

            /*
            m_volume = gameObject.AddComponent<CustomPassVolume>();
            m_volume.isGlobal = true;
            m_volume.injectionPoint = CustomPassInjectionPoint.BeforePostProcess;
            m_volume.customPasses = new List<CustomPass> { renderCache };
            */

            PBSelectionPicker.Renderer = new PBSelectionPickerRendererHDRP();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            //IOC.Unregister<IRenderersCache>("ProBuilder.SelectionPickerCache", m_selectionPickerCache);
            //Destroy(m_selectionPickerCache);
            //m_selectionPickerCache = null;
           // Destroy(m_volume);
            //m_volume = null;
        }

        protected override void OnEditorClosed()
        {
            base.OnEditorClosed();
            //IOC.Unregister<IRenderersCache>("ProBuilder.SelectionPickerCache", m_selectionPickerCache);
           // Destroy(m_selectionPickerCache);
            //m_selectionPickerCache = null;
            //Destroy(m_volume);
            //m_volume = null;
        }
    }
}
