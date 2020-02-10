using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Battlehub.Spline3
{

    public class BaseSplineState
    {
        public Vector3[] ControlPoints;
        public bool IsSelectable;
        public bool IsLooping;    

        public BaseSplineState(Vector3[] controlPoints, bool isSelectable, bool isLooping)
        {
            ControlPoints = controlPoints;
            IsSelectable = isSelectable;
            IsLooping = isLooping;
        }
    }

    public abstract class BaseSpline : MonoBehaviour
    {
        public abstract Vector3[] LocalControlPoints
        {
            get;
            set;
        }

        public abstract int ControlPointCount
        {
            get;
        }

        public abstract int SegmentsCount
        {
            get;
        }

        public abstract bool IsLooping
        {
            get;
            set;
        }

        public abstract bool IsSelectable
        {
            get;
            set;
        }

        public abstract void Append(float distance = 0);
        public abstract void Prepend(float distance = 0);
        public abstract void Remove(int segmentIndex);

        public abstract Vector3 GetPosition(float t);
        public abstract Vector3 GetPosition(int segmentIndex, float t);
        public abstract Vector3 GetLocalPosition(float t);
        public abstract Vector3 GetLocalPosition(int segmentIndex, float t);
        public abstract Vector3 GetTangent(float t);
        public abstract Vector3 GetTangent(int segmentIndex, float t);
        public abstract Vector3 GetLocalTangent(float t);
        public abstract Vector3 GetLocalTangent(int segmentIndex, float t);
        public abstract Vector3 GetDirection(float t);
        public abstract Vector3 GetDirection(int segmentIndex, float t);
        public abstract Vector3 GetLocalDirection(float t);
        public abstract Vector3 GetLocalDirection(int segmentIndex, float t);

        public abstract void SetControlPoint(int index, Vector3 position);
        public abstract void SetLocalControlPoint(int index, Vector3 position);
        public abstract Vector3 GetControlPoint(int index);
        public abstract Vector3 GetLocalControlPoint(int index);

        public abstract BaseSplineState GetState();
        public abstract void SetState(BaseSplineState state);

        protected SplineRenderer m_renderer;

        protected virtual void Awake()
        {
            m_renderer = gameObject.AddComponent<SplineRenderer>();
            m_renderer.enabled = false;
        }

        protected virtual void OnDestroy()
        {
            Destroy(m_renderer);
        }
    }

    [DefaultExecutionOrder(-1)]
    public class CatmullRomSpline : BaseSpline
    {
        [SerializeField]
        private Vector3[] m_controlPoints = null;
        [SerializeField]
        private bool m_isLooping = true;
        [SerializeField]
        private bool m_isSelectable = true;

        public override bool IsSelectable
        {
            get { return m_isSelectable; }
            set { m_isSelectable = value; }
        }

        public override bool IsLooping
        {
            get { return m_isLooping; }
            set { m_isLooping = value; }
        }

        public override Vector3[] LocalControlPoints
        {
            get { return m_controlPoints; }
            set { m_controlPoints = value; }
        }

        public override int ControlPointCount
        {
            get { return m_controlPoints != null ? m_controlPoints.Length : 0; }
        }

        public override int SegmentsCount
        {
            get
            {
                if(m_controlPoints == null || m_controlPoints.Length == 0)
                {
                    return 0;
                }

                if(m_isLooping)
                {
                    return m_controlPoints.Length;
                }
                return Mathf.Max(0, m_controlPoints.Length - 3);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            if(m_controlPoints == null || m_controlPoints.Length == 0)
            {
                m_controlPoints = new[] { Vector3.back * 3f, Vector3.back, Vector3.forward, Vector3.forward * 3f };
            }

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void Append(float distance = 0)
        {
            Vector3 position;
            Vector3 tangent;
            int controlPointsCount = ControlPointCount;
            if (controlPointsCount > 1)
            {
                position = m_controlPoints[m_controlPoints.Length - 1];
                tangent = position - m_controlPoints[m_controlPoints.Length - 2];
            }
            else if(controlPointsCount == 1)
            {
                position = Vector3.forward;
                tangent = Vector3.forward;
            }
            else
            {
                position = Vector3.zero;
                tangent = Vector3.forward;
            }

            Array.Resize(ref m_controlPoints, m_controlPoints.Length + 1);
            m_controlPoints[m_controlPoints.Length - 1] = position + tangent.normalized * distance;

            m_renderer.Refresh(false);
        }

        public override void Prepend(float distance = 0)
        {
            Vector3 position;
            Vector3 tangent;
            int controlPointsCount = ControlPointCount;
            if (controlPointsCount > 1)
            {
                position = m_controlPoints[0];
                tangent = position - m_controlPoints[1];
            }
            else if (controlPointsCount == 1)
            {
                position = Vector3.back;
                tangent = Vector3.back;
            }
            else
            {
                position = Vector3.zero;
                tangent = Vector3.back;
            }

            Array.Resize(ref m_controlPoints, m_controlPoints.Length + 1);
            for(int i = m_controlPoints.Length - 1; i > 0; --i)
            {
                m_controlPoints[i] = m_controlPoints[i - 1];
            }
            m_controlPoints[0] = position + tangent.normalized * distance;
            m_renderer.Refresh(false);
        }

        public override void Remove(int index)
        {
            List<Vector3> controlPoints = m_controlPoints.ToList();
            controlPoints.RemoveAt(index);
            m_controlPoints = controlPoints.ToArray();
            m_renderer.Refresh(false);
        }

        public override Vector3 GetPosition(float t)
        {
            int segmentIndex = GetSegmentIndex(ref t);
            return GetCatmullRomPosition(segmentIndex, t);
        }

        public override Vector3 GetPosition(int segmentIndex, float t)
        {
            t = Mathf.Clamp01(t);
            if (!m_isLooping)
            {
                segmentIndex++;
            }
            segmentIndex = ClampIndex(segmentIndex);
            return GetCatmullRomPosition(segmentIndex, t);
        }

        public override Vector3 GetLocalPosition(float t)
        {
            int segmentIndex = GetSegmentIndex(ref t);
            return GetCatmullRomLocalPosition(segmentIndex, t);
        }

        public override Vector3 GetLocalPosition(int segmentIndex, float t)
        {
            t = Mathf.Clamp01(t);
            if (!m_isLooping)
            {
                segmentIndex++;
            }
            segmentIndex = ClampIndex(segmentIndex);
            return GetCatmullRomLocalPosition(segmentIndex, t);
        }

        public override Vector3 GetTangent(float t)
        {
            int segmentIndex = GetSegmentIndex(ref t);
            return GetCatmullRomTangent(segmentIndex, t);
        }

        public override Vector3 GetTangent(int segmentIndex, float t)
        {
            t = Mathf.Clamp01(t);
            if (!m_isLooping)
            {
                segmentIndex++;
            }
            segmentIndex = ClampIndex(segmentIndex);
            return GetCatmullRomTangent(segmentIndex, t);
        }

        public override Vector3 GetLocalTangent(float t)
        {
            int segmentIndex = GetSegmentIndex(ref t);
            return GetCatmullRomLocalTangent(segmentIndex, t);
        }

        public override Vector3 GetLocalTangent(int segmentIndex, float t)
        {
            t = Mathf.Clamp01(t);
            if (!m_isLooping)
            {
                segmentIndex++;
            }
            segmentIndex = ClampIndex(segmentIndex);
            return GetCatmullRomLocalTangent(segmentIndex, t);
        }

        public override Vector3 GetDirection(float t)
        {
            return GetTangent(t).normalized;
        }

        public override Vector3 GetDirection(int segmentIndex, float t)
        {
            return GetTangent(segmentIndex, t).normalized;
        }

        public override Vector3 GetLocalDirection(float t)
        {
            return GetLocalTangent(t).normalized;
        }

        public override Vector3 GetLocalDirection(int segmentIndex, float t)
        {
            return GetLocalDirection(segmentIndex, t);
        }

        public override void SetControlPoint(int index, Vector3 position)
        {
            position = transform.InverseTransformPoint(position);
            if (m_controlPoints[index] != position)
            {
                m_controlPoints[index] = position;
                m_renderer.Refresh(true);
            }   
        }

        public override void SetLocalControlPoint(int index, Vector3 localPosition)
        {
            if (m_controlPoints[index] != localPosition)
            {
                m_controlPoints[index] = localPosition;
                m_renderer.Refresh(true);
            }
        }

        public override Vector3 GetControlPoint(int index)
        {
            return transform.TransformPoint(m_controlPoints[index]);
        }

        public override Vector3 GetLocalControlPoint(int index)
        {
            return m_controlPoints[index];
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;

            if(m_controlPoints == null)
            {
                return;
            }

            for (int i = 0; i < m_controlPoints.Length; i++)
            {
                if ((i == 0 || i == m_controlPoints.Length - 2 || i == m_controlPoints.Length - 1) && !m_isLooping)
                {
                    continue;
                }

                DisplayCatmullRomSpline(i);
            }
        }

        private void DisplayCatmullRomSpline(int pos)
        {
            Vector3 p0 = transform.TransformPoint(m_controlPoints[ClampIndex(pos - 1)]);
            Vector3 p1 = transform.TransformPoint(m_controlPoints[pos]); 
            Vector3 p2 = transform.TransformPoint(m_controlPoints[ClampIndex(pos + 1)]);
            Vector3 p3 = transform.TransformPoint(m_controlPoints[ClampIndex(pos + 2)]);            
            Vector3 lastPos = p1;

            float resolution = 0.2f;

            int loops = Mathf.FloorToInt(1f / resolution);

            for (int i = 1; i <= loops; i++)
            {
                float t = i * resolution;

                Vector3 newPos = GetCatmullRomPosition(t, p0, p1, p2, p3);

                Gizmos.DrawLine(lastPos, newPos);

                lastPos = newPos;
            }
        }

        private int ClampIndex(int pos)
        {
            if (pos < 0)
            {
                pos = m_controlPoints.Length - 1;
            }

            if (pos > m_controlPoints.Length)
            {
                pos = 1;
            }
            else if (pos > m_controlPoints.Length - 1)
            {
                pos = 0;
            }

            return pos;
        }

        private int GetSegmentIndex(ref float t)
        {
            t = Mathf.Clamp01(t);
            float segmentSize;
            int segmentIndex;
            if (m_isLooping)
            {
                segmentSize = 1.0f / m_controlPoints.Length;
                segmentIndex = ClampIndex(Mathf.FloorToInt(t / segmentSize));
            }
            else
            {
                segmentSize = 1.0f / (m_controlPoints.Length - 3);
                segmentIndex = ClampIndex(Mathf.FloorToInt(t / segmentSize) + 1);
            }

            t = t % segmentSize;
            t = t / segmentSize;
            return segmentIndex;
        }

        private Vector3 GetCatmullRomLocalPosition(int index, float t)
        {
            Vector3 p0 = m_controlPoints[ClampIndex(index - 1)];
            Vector3 p1 = m_controlPoints[index];
            Vector3 p2 = m_controlPoints[ClampIndex(index + 1)];
            Vector3 p3 = m_controlPoints[ClampIndex(index + 2)];

            return GetCatmullRomPosition(t, p0, p1, p2, p3);
        }

        private Vector3 GetCatmullRomPosition(int index, float t)
        {
            Vector3 p0 = transform.TransformPoint(m_controlPoints[ClampIndex(index - 1)]);
            Vector3 p1 = transform.TransformPoint(m_controlPoints[index]);
            Vector3 p2 = transform.TransformPoint(m_controlPoints[ClampIndex(index + 1)]);
            Vector3 p3 = transform.TransformPoint(m_controlPoints[ClampIndex(index + 2)]);

            return GetCatmullRomPosition(t, p0, p1, p2, p3);
        }

        private Vector3 GetCatmullRomLocalTangent(int index, float t)
        {
            Vector3 p0 = m_controlPoints[ClampIndex(index - 1)];
            Vector3 p1 = m_controlPoints[index];
            Vector3 p2 = m_controlPoints[ClampIndex(index + 1)];
            Vector3 p3 = m_controlPoints[ClampIndex(index + 2)];

            return GetCatmullRomTangent(t, p0, p1, p2, p3);
        }

        private Vector3 GetCatmullRomTangent(int index, float t)
        {
            Vector3 p0 = transform.TransformPoint(m_controlPoints[ClampIndex(index - 1)]);
            Vector3 p1 = transform.TransformPoint(m_controlPoints[index]);
            Vector3 p2 = transform.TransformPoint(m_controlPoints[ClampIndex(index + 1)]);
            Vector3 p3 = transform.TransformPoint(m_controlPoints[ClampIndex(index + 2)]);

            return GetCatmullRomTangent(t, p0, p1, p2, p3);
        }

        private Vector3 GetCatmullRomPosition(float t, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            return .5f * (
                (-a + 3f * b - 3f * c + d) * (t * t * t)
                + (2f * a - 5f * b + 4f * c - d) * (t * t)
                + (-a + c) * t
                + 2f * b);
        }

        public static Vector3 GetCatmullRomTangent(float t, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            return 1.5f * (-a + 3f * b - 3f * c + d) * (t * t)
                    + (2f * a - 5f * b + 4f * c - d) * t
                    + .5f * c - .5f * a;
        }

        public override BaseSplineState GetState()
        {
            return new BaseSplineState(m_controlPoints.ToArray(), IsSelectable, IsLooping);
        }

        public override void SetState(BaseSplineState state)
        {
            m_controlPoints = state.ControlPoints.ToArray();
            m_isLooping = state.IsLooping;
            m_isSelectable = state.IsSelectable;
            m_renderer.Refresh();
        }


    }
}
