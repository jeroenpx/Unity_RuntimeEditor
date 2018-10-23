using UnityEngine;

namespace Battlehub.RTCommon
{
    public class Pointer : MonoBehaviour
    {
        public virtual Ray Ray
        {
            get { return m_window.Camera.ScreenPointToRay(m_window.Editor.Input.GetPointerXY(0)); }
        }

        public virtual Vector2 ScreenPoint
        {
            get { return m_window.Editor.Input.GetPointerXY(0); }
        }

        [SerializeField]
        protected RuntimeWindow m_window;
        private void Awake()
        {
            if(m_window == null)
            {
                m_window = GetComponent<RuntimeWindow>();
            }
        }

        public virtual bool WorldToScreenPoint(Vector3 worldPoint, Vector3 point, out Vector2 result)
        {
            result = m_window.Camera.WorldToScreenPoint(point);
            return true;
        }

        public virtual bool XY(Vector3 worldPoint, out Vector2 result)
        {
            result = m_window.Editor.Input.GetPointerXY(0);
            return true;
        }

        public virtual bool ToWorldMatrix(Vector3 worldPoint, out Matrix4x4 matrix)
        {
            matrix = m_window.Camera.cameraToWorldMatrix;
            return true;
        }

        public static implicit operator Ray(Pointer pointer)
        {
            return pointer.Ray;
        }
    }
}