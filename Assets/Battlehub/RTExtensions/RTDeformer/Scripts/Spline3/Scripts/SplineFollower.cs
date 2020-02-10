using UnityEngine;

namespace Battlehub.Spline3
{
    public class SplineFollower : MonoBehaviour
    {
        [SerializeField]
        private BaseSpline m_spline = null;
        public BaseSpline Spline
        {
            get { return m_spline; }
            set { m_spline = value; }
        }

        [SerializeField]
        private float m_speed = 1.0f;
        public float Speed
        {
            get { return m_speed; }
            set { m_speed = value; }
        }

        private float m_t;

        private void Start()
        {
            float len = 0;
            Vector3 p0 = m_spline.GetPosition(0);
            for(int i = 1; i <= 100; ++i)
            {
                Vector3 p1 = m_spline.GetPosition((float)i / 100);
                len += (p1 - p0).magnitude;
                p0 = p1;
            }
            Debug.Log(name + " length: " + len);
        }

        private void Update()
        {
            Vector3 tangent = m_spline.GetTangent(m_t);
            float v = tangent.magnitude;
            v *= m_spline.SegmentsCount;

            m_t += (Time.deltaTime * m_speed) / v;
            transform.position = m_spline.GetPosition(m_t);
            transform.rotation = Quaternion.LookRotation(tangent);

            if(m_t == 1)
            {
                Destroy(this);
            }
        }
    }

}
