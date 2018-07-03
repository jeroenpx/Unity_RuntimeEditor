using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battlehub.RTCommon
{
    public class InputController : MonoBehaviour
    {
        private static InputController m_instance;
        public static InputController Instance
        {
            get { return m_instance; }
        }

        private bool m_isInputFieldSelected;
        private GameObject m_selectedGameObject;

        public static bool _GetKeyDown(KeyCode key)
        {
            if (m_instance != null)
            {
                if (m_instance.m_isInputFieldSelected)
                {
                    return false;
                }
                return m_instance.GetKeyDown(key);
            }

            return Input.GetKeyDown(key);
        }

        public static bool _GetKeyUp(KeyCode key)
        {
            if (m_instance != null)
            {
                if (m_instance.m_isInputFieldSelected)
                {
                    return false;
                }
                return m_instance.GetKeyUp(key);
            }

            return Input.GetKeyUp(key);
        }

        public static bool _GetKey(KeyCode key)
        {
            if (m_instance != null)
            {
                if (m_instance.m_isInputFieldSelected)
                {
                    return false;
                }
                return m_instance.GetKey(key);
            }

            return Input.GetKey(key);
        }

        public static bool _GetButtonDown(string button)
        {
            if (m_instance != null)
            {
                return m_instance.GetButtonDown(button);
            }
            return Input.GetButtonDown(button);
        }

        public static bool _GetButtonUp(string button)
        {
            if (m_instance != null)
            {
                return m_instance.GetButtonUp(button);
            }
            return Input.GetButtonUp(button);
        }

        public static bool _GetButton(string button)
        {
            if (m_instance != null)
            {
                return m_instance.GetButton(button);
            }
            return Input.GetButton(button);
        }

        public static float _GetAxis(string axis)
        {
            if (m_instance != null)
            {
                return m_instance.GetAxis(axis);
            }
            return Input.GetAxis(axis);
        }

        public static float _GetAxisRaw(string axis)
        {
            return Input.GetAxisRaw(axis);
        }

        public static Vector3 _MousePosition
        {
            get
            {
                if(m_instance != null)
                {
                    return m_instance.MousePosition;
                }
                return Input.mousePosition;
            }
        }

        public static bool _GetMouseButtonDown(int button)
        {
            if (m_instance != null)
            {
                return m_instance.GetMouseButtonDown(button);
            }
            return Input.GetMouseButtonDown(button);
        }

        public static bool _GetMouseButtonUp(int button)
        {
            if (m_instance != null)
            {
                return m_instance.GetMouseButtonUp(button);
            }
            return Input.GetMouseButtonUp(button);
        }

        public static bool _GetMouseButton(int button)
        {
            if (m_instance != null)
            {
                return m_instance.GetMouseButton(button);
            }
            return Input.GetMouseButton(button);
        }

        public virtual bool GetKeyDown(KeyCode key)
        {
            return Input.GetKeyDown(key);
        }

        public virtual bool GetKeyUp(KeyCode key)
        {
            return Input.GetKeyUp(key);
        }

        public virtual bool GetKey(KeyCode key)
        {
            return Input.GetKey(key);
        }

        public virtual bool GetButtonDown(string button)
        {
            return Input.GetButtonDown(button);
        }

        public virtual bool GetButtonUp(string button)
        {
            return Input.GetButtonUp(button);
        }

        public virtual bool GetButton(string button)
        {
            return Input.GetButton(button);
        }

        public virtual float GetAxis(string axis)
        {
            return Input.GetAxis(axis);
        }

        public virtual float GetAxisRaw(string axis)
        {
            return Input.GetAxisRaw(axis);
        }

        public virtual Vector3 MousePosition
        {
            get { return Input.mousePosition; }
        }

        public virtual bool GetMouseButtonDown(int button)
        {
            return Input.GetMouseButtonDown(button);
        }

        public virtual bool GetMouseButtonUp(int button)
        {
            return Input.GetMouseButtonUp(button);
        }

        public virtual bool GetMouseButton(int button)
        {
            return Input.GetMouseButton(button);
        }

        private void Awake()
        {
            if(m_instance != null)
            {
                Debug.LogWarning("Another instance of InputController exists");
            }
            m_instance = this;
        }

        private void OnDestroy()
        {
            if(m_instance == this)
            {
                m_instance = null;
            }
        }

        private void Update()
        {
            if(EventSystem.current != null)
            {
                if(EventSystem.current.currentSelectedGameObject != m_selectedGameObject)
                {
                    m_selectedGameObject = EventSystem.current.currentSelectedGameObject;
                    m_isInputFieldSelected = m_selectedGameObject != null && m_selectedGameObject.GetComponent<InputField>() != null;
                }
            }
        }
    }

}
