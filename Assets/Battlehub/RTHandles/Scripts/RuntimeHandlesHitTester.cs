using Battlehub.RTCommon;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTHandles
{
    public class RuntimeHandlesHitTester : MonoBehaviour
    {
        protected readonly List<BaseHandle> m_handles = new List<BaseHandle>();

        protected BaseHandle m_selectedHandle;
        protected RuntimeHandleAxis m_selectedAxis;

        public static void InitializeIfRequired(RuntimeWindow window, ref RuntimeHandlesHitTester hitTester)
        {
            hitTester = window.GetComponent<RuntimeHandlesHitTester>();
            if(!hitTester)
            {
                hitTester = window.gameObject.AddComponent<RuntimeHandlesHitTester>();
            }
        }

        public virtual void Add(BaseHandle handle)
        {
            m_handles.Add(handle);
        }

        public virtual void Remove(BaseHandle handle)
        {
            m_handles.Remove(handle);
        }

        public virtual RuntimeHandleAxis GetSelectedAxis(BaseHandle handle)
        {
            if(m_selectedHandle == null)
            {
                return RuntimeHandleAxis.None;
            }

            if(m_selectedHandle != handle)
            {
                return RuntimeHandleAxis.None;
            }

            return m_selectedAxis;
            
        }

        protected virtual void Update()
        {
            m_selectedHandle = null;
            m_selectedAxis = RuntimeHandleAxis.None;

            float minDistance = float.PositiveInfinity;
            for(int i = 0; i < m_handles.Count; ++i)
            {
                BaseHandle handle = m_handles[i];

                float distance;
                RuntimeHandleAxis selectedAxis = handle.Hit(out distance);
                if(distance < minDistance)
                {
                    minDistance = distance;
                    m_selectedAxis = selectedAxis;
                    m_selectedHandle = handle;
                }
            }
        }
    }
}


