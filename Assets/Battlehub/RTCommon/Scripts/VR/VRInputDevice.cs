using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Battlehub.RTCommon
{
    public class VRInputDevice : MonoBehaviour
    {
        private InputDevice m_inputDevice;
        public InputDevice Device
        {
            get { return m_inputDevice; }
            set { m_inputDevice = value; }
        }

        private void Start()
        {
            List<InputFeatureUsage> usages = new List<InputFeatureUsage>();
            if(m_inputDevice.TryGetFeatureUsages(usages))
            {
                for(int i = 0; i < usages.Count; ++i)
                {
                    Debug.Log(usages[i].name + " " + usages[i].type);
                }
            }
        }

        private void Update()
        {
            bool isTriggerPressed;
            if(m_inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out isTriggerPressed))
            {
                if(isTriggerPressed)
                {
                    Debug.Log("Trigger Pressed");
                }
            }
        }
    }

}

