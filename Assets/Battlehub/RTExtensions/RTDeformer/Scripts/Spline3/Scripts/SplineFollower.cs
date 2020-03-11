using UnityEngine;

namespace Battlehub.Spline3
{
    public class SplineFollower : MonoBehaviour
    {
        [SerializeField]
        protected BaseSpline m_spline = null;
        public BaseSpline Spline
        {
            get { return m_spline; }
            set { m_spline = value; }
        }

        [SerializeField]
        protected float m_speed = 1.0f;
        public float Speed
        {
            get { return m_speed; }
            set { m_speed = value; }
        }

        [SerializeField]
        protected float m_smoothRotation = 1.0f;
        public float SmoothRotation
        {
            get { return m_smoothRotation; }
            set { m_smoothRotation = value; }
        }

        [SerializeField]
        protected bool m_autoDestroy = false;
        public bool AutoDestroy
        {
            get { return m_autoDestroy; }
            set { m_autoDestroy = value; }
        }

        [SerializeField]
        protected bool m_loop = true;
        public bool Loop
        {
            get { return m_loop; }
            set { m_loop = value; }
        }

        protected float m_t;
        public float T
        {
            get { return m_t; }
            set { m_t = value; }
        }

        protected virtual void Start()
        {
            float len = 0;
            Vector3 p0 = m_spline.GetPosition(0);
            for(int i = 1; i <= 100; ++i)
            {
                Vector3 p1 = m_spline.GetPosition((float)i / 100);
                len += (p1 - p0).magnitude;
                p0 = p1;
            }

            Vector3 tangent = m_spline.GetTangent(m_t);

            transform.position = m_spline.GetPosition(0);
            transform.rotation = Quaternion.LookRotation(tangent);
        }

        protected virtual void Update()
        {
            Vector3 tangent = m_spline.GetTangent(m_t);
            float v = tangent.magnitude;
            v *= m_spline.SegmentsCount;
            m_t += (Time.deltaTime * m_speed) / v;

            if (m_t >= 1)
            {
                if (m_autoDestroy)
                {
                    Destroy(this);
                }
                else if (m_loop)
                {
                    m_t %= 1;

                    if(!m_spline.IsLooping)
                    {
                        ResetFollower();
                    }
                    
                }
                else
                {
                    m_t = 1;
                }
            }

            UpdateFollower();
        }

        protected virtual void ResetFollower()
        {
            transform.position = m_spline.GetPosition(m_t);
            transform.rotation = Quaternion.LookRotation(m_spline.GetTangent(m_t));
        }

        protected virtual void UpdateFollower()
        {
            transform.position = m_spline.GetPosition(m_t);

            transform.rotation = SmoothRotation <= 0 ?
                Quaternion.LookRotation(m_spline.GetTangent(m_t)) :
                Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(m_spline.GetTangent(m_t)), Time.deltaTime * Speed / SmoothRotation);


        }
    }

}
