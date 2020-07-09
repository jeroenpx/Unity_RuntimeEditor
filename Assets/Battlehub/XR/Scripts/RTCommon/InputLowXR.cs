using Battlehub.RTEditor;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace Battlehub.RTCommon.XR
{
    [DefaultExecutionOrder(100)]
    public class InputLowXR : MonoBehaviour, IInput
    {
        [SerializeField]
        private XRController[] m_controllers = null;

        private XRController[] m_activeController = new XRController[1];
        private bool[] m_isPressed = new bool[1];

        private XRController ActiveController(int button)
        {
            if(button < 0 || button >= m_activeController.Length)
            {
                return null;
            }

            return m_activeController[button];
        }

        private void ActiveController(int button, XRController controller)
        {
            if (button < 0 || button >= m_activeController.Length)
            {
                return;
            }

            m_activeController[button] = controller;
        }

        private bool IsPressed(int button)
        {
            return m_isPressed[0];
        }

        private void IsPressed(int button, bool value)
        {
            m_isPressed[0] = value;
        }

        private InputFeatureUsage<bool> GetUsageForPointer(int button)
        {
            return CommonUsages.triggerButton;
        }

        private Canvas m_uiCanvas;
        private void Awake()
        {
            m_uiCanvas = IOC.Resolve<IRTEAppearance>().UIForegroundScaler.GetComponent<Canvas>();
            IOC.RegisterFallback<IInput>(this);
        }

        private void OnDestroy()
        {
            IOC.UnregisterFallback<IInput>(this);
        }

        private void LateUpdate()
        {
            for(int i = 0; i < m_activeController.Length; ++i)
            {
                bool value;
                XRController activeController = m_activeController[i];
                if (activeController != null && activeController.inputDevice.TryGetFeatureValue(GetUsageForPointer(i), out value))
                {
                    if(value != IsPressed(i))
                    {
                        IsPressed(i, value);
                        if(!value)
                        {
                            ActiveController(i, null);
                        }
                    }
                }
            } 
        }


        public float GetAxis(InputAxis axis)
        {
            return 0;
        }

        public bool GetKey(KeyCode key)
        {
            return false;
        }

        public bool GetKeyDown(KeyCode key)
        {
            return false;
        }

        public bool GetKeyUp(KeyCode key)
        {
            return false;
        }

        public bool GetPointerDown(int button)
        {
            XRController activeController = ActiveController(button);
            if (activeController != null)
            {
                bool value;
                if(activeController.inputDevice.TryGetFeatureValue(GetUsageForPointer(button), out value) && value && !IsPressed(button))
                {
                    return true;
                }
                return false;
            }

            for (int i = 0; i < m_controllers.Length; ++i)
            {
                bool value;
                if(m_controllers[i].inputDevice.TryGetFeatureValue(GetUsageForPointer(button), out value) && value && !IsPressed(button))
                {
                    ActiveController(button, m_controllers[i]);
                    return true;
                }
            }
            return false;
        }

        public bool GetPointer(int button)
        {
            XRController activeController = ActiveController(button);
            if (activeController == null)
            {
                return false;
            }

            bool value;
            if(activeController.inputDevice.TryGetFeatureValue(GetUsageForPointer(button), out value))
            {
                return value;
            }
            return false;
        }

        public bool GetPointerUp(int button)
        {
            XRController activeController = ActiveController(button);
            if (activeController == null)
            {
                return false;
            }

            bool value;
            if(activeController.inputDevice.TryGetFeatureValue(GetUsageForPointer(button), out value) && !value && IsPressed(button))
            {
                return true;
            }
            return false;
        }

        public Vector3 GetPointerXY(int pointer)
        {
            if(pointer < 0 || pointer >= m_controllers.Length)
            {
                return new Vector3(float.NaN, float.NaN, float.NaN);
            }

            XRController controller = m_controllers[pointer];
            Ray ray = new Ray(controller.transform.position, controller.transform.forward);

            Plane plane = new Plane(-ray.direction, ray.origin + ray.direction);
            float distance;
            if(plane.Raycast(ray, out distance))
            {
                Vector3 point = ray.GetPoint(distance);

                point = m_uiCanvas.worldCamera.WorldToScreenPoint(point);

                return point;
            }

            return new Vector3(float.NaN, float.NaN, float.NaN);
        }

        public bool IsAnyKeyDown()
        {
            return Input.anyKeyDown;
        }

    }

}
