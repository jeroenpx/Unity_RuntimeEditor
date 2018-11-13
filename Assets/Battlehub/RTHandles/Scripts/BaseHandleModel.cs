using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTHandles
{
    public class BaseHandleModel : RTEBehaviour
    {
        private RuntimeHandlesComponent m_appearance;
        public RuntimeHandlesComponent Appearance
        {
            get { return m_appearance; }
            set { m_appearance = value; }
        }

        public RTHColors Colors
        {
            get { return m_appearance.Colors; }
        }

        private float m_modelScale = 1.0f;
        public float ModelScale
        {
            get { return m_modelScale;  }
            set
            {
                if(m_modelScale != value)
                {
                    m_modelScale = value;
                    UpdateModel();
                }   
            }
        }

        private float m_selectionMargin = 1.0f;
        public float SelectionMargin
        {
            get { return m_selectionMargin; }
            set
            {
                if(m_selectionMargin != value)
                {
                    m_selectionMargin = value;
                    UpdateModel();   
                }
            }
        }

        protected RuntimeHandleAxis m_selectedAxis = RuntimeHandleAxis.None;
        protected LockObject m_lockObj = new LockObject();

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            RuntimeGraphicsLayer graphicsLayer = Window.GetComponent<RuntimeGraphicsLayer>();
            if (graphicsLayer == null)
            {
                graphicsLayer = Window.gameObject.AddComponent<RuntimeGraphicsLayer>();
            }

            SetLayer(transform, graphicsLayer.Window.Editor.CameraLayerSettings.RuntimeGraphicsLayer + Window.Index);
        }

        private void SetLayer(Transform t, int layer)
        {
            t.gameObject.layer = layer;
            foreach (Transform child in t)
            {
                SetLayer(child, layer);
            }
        }

        public virtual void SetLock(LockObject lockObj)
        { 
            if(lockObj == null)
            {
                lockObj = new LockObject();
            }
            m_lockObj = lockObj;
        }

        public virtual void Select(RuntimeHandleAxis axis)
        {
            m_selectedAxis = axis;   
        }

        public virtual void SetScale(Vector3 scale)
        {

        }

        public virtual RuntimeHandleAxis HitTest(Ray ray)
        {
            return RuntimeHandleAxis.None;
        }

        protected virtual void Start()
        {

        }


        protected virtual void Update()
        {

        }

        protected virtual void UpdateModel()
        {

        }

        //protected virtual void OnWillRenderObject()
        //{
        //    float screenScale = RuntimeHandlesComponent.GetScreenScale(transform.position, Camera.current);
        //    transform.localScale = Appearance.InvertZAxis ? new Vector3(1, 1, -1) * screenScale : Vector3.one * screenScale;
        //}
    }

}
