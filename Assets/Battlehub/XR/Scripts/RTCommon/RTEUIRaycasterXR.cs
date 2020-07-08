using Battlehub.UIControls.XR;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Battlehub.RTCommon.XR
{
    public class RTEUIRaycasterXR : MonoBehaviour, IUIRaycaster
    {
        public Camera eventCamera
        {
            get { return m_raycaster.eventCamera; }
        }

        [SerializeField]
        private GraphicsRaycasterXR m_raycaster = null;
        [SerializeField]
        private XRRayInteractor[] m_rayInteractors = null;

        private IInput m_input;
        private IRTE m_editor;
        private void Awake()
        {
            m_editor = IOC.Resolve<IRTE>();
            m_input = m_editor.Input;
            if (m_raycaster == null)
            {
                m_raycaster = gameObject.GetComponent<GraphicsRaycasterXR>();
                if (m_raycaster == null)
                {
                    m_raycaster = gameObject.AddComponent<GraphicsRaycasterXR>();
                }
            }

            IOC.RegisterFallback<IUIRaycaster>(this);
        }

        private void OnDestroy()
        {
            IOC.UnregisterFallback<IUIRaycaster>(this);
        }

        public void Raycast(List<RaycastResult> results)
        {
            TrackedDeviceModel model;
            if(m_rayInteractors[0].TryGetUIModel(out model))
            {
                TrackedDeviceEventData eventData = new TrackedDeviceEventData(m_editor.EventSystem);
                model.CopyTo(eventData);
                m_raycaster.Raycast(eventData, results);
            }
        }

        public void Raycast(PointerEventData eventData, List<RaycastResult> results)
        {
            eventData.position = m_input.GetPointerXY(0);
            m_raycaster.Raycast(eventData, results);
        }
    }

}
