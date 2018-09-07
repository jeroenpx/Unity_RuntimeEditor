using UnityEngine;
using System.Collections;

namespace Battlehub.RTCommon
{
    [AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
    public class MouseOrbit : MonoBehaviour
    {
        protected Camera m_camera;
        public Transform Target;
        public float Distance = 5.0f;
        public float XSpeed = 5.0f;
        public float YSpeed = 5.0f;

        public float YMinLimit = -360f;
        public float YMaxLimit = 360f;

        public float DistanceMin = 0.5f;
        public float DistanceMax = 5000f;

        protected float m_x = 0.0f;
        protected float m_y = 0.0f;

        private void Awake()
        {
            m_camera = GetComponent<Camera>();
        }

        private void Start()
        {
            SyncAngles();
        }

        public virtual void SyncAngles()
        {
            Vector3 angles = transform.eulerAngles;
            m_x = angles.y;
            m_y = angles.x;
        }

        private void LateUpdate()
        {
            float deltaX = InputController._GetAxis("Mouse X");
            float deltaY = InputController._GetAxis("Mouse Y");

            deltaX = deltaX * XSpeed;
            deltaY = deltaY * YSpeed;
            
            m_x += deltaX;
            m_y -= deltaY;
            m_y = Mathf.Clamp(m_y % 360, YMinLimit, YMaxLimit);

            Zoom();
        }

        public virtual void Zoom()
        {
            Quaternion rotation = Quaternion.Euler(m_y, m_x, 0);
            transform.rotation = rotation;

            float mwheel = InputController._GetAxis("Mouse ScrollWheel");

            if (m_camera.orthographic)
            {
                m_camera.orthographicSize -= mwheel * m_camera.orthographicSize;
                if(m_camera.orthographicSize < 0.01f)
                {
                    m_camera.orthographicSize = 0.01f;
                }
            }

            Distance = Mathf.Clamp(Distance - mwheel * Mathf.Max(1.0f, Distance), DistanceMin, DistanceMax);
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -Distance);
            Vector3 position = rotation * negDistance + Target.position;
            transform.position = position;
        }
    }
}
